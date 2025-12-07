using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameCardEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("CARD GAME/GameCardEditor")]
    public static void ShowExample()
    {
        GameCardEditor wnd = GetWindow<GameCardEditor>();
        wnd.titleContent = new GUIContent("GameCardEditor");
    }

    private ListView _cardListView;
    private VisualElement _detailPanel;
    private Button _addButton;
    private Button _removeButton;
    private Button _saveButton;
    private Label _statusLabel;

    private readonly Dictionary<string, BaseConfig> _configs = new();
    private readonly List<CardDataConfig> _cardItems = new();
    private CardDataConfig _selectedCard;
    private bool _hasUnsavedChanges;
    private enum CardConfigVariant
    {
        Base,
        StatModifiers,
        Treasure,
        Door,
        Event,
        Monster
    }

    public async void CreateGUI()
    {
        var ui = m_VisualTreeAsset.CloneTree();
        rootVisualElement.Add(ui);

        CacheUIReferences();
        ConfigureListView();
        RegisterButtonCallbacks();

        await LoadConfigsAsync();
        RefreshCardList();
        UpdateDetailPanel(null);
        UpdateButtonsState();
    }

    private void CacheUIReferences()
    {
        _cardListView = rootVisualElement.Q<ListView>("CardList");
        _detailPanel = rootVisualElement.Q<VisualElement>("DetailPanel");
        _addButton = rootVisualElement.Q<Button>("AddButton");
        _removeButton = rootVisualElement.Q<Button>("RemoveButton");
        _saveButton = rootVisualElement.Q<Button>("SaveButton");
        _statusLabel = rootVisualElement.Q<Label>("StatusLabel");
    }

    private void ConfigureListView()
    {
        if (_cardListView == null)
            return;

        _cardListView.itemsSource = _cardItems;
        _cardListView.selectionType = SelectionType.Single;
        _cardListView.makeItem = () => new Label();
        _cardListView.bindItem = (element, index) =>
        {
            if (element is Label label && index >= 0 && index < _cardItems.Count)
            {
                label.text = FormatCardLabel(_cardItems[index]);
            }
        };
        _cardListView.unbindItem = (element, _) =>
        {
            if (element is Label label)
            {
                label.text = string.Empty;
            }
        };
        _cardListView.onSelectionChange += OnSelectionChanged;
    }

    private void RegisterButtonCallbacks()
    {
        if (_addButton != null)
        {
            _addButton.clicked += OnAddCard;
        }

        if (_removeButton != null)
        {
            _removeButton.clicked += OnRemoveCard;
        }

        if (_saveButton != null)
        {
            _saveButton.clicked += OnSaveCards;
        }
    }

    private async System.Threading.Tasks.Task LoadConfigsAsync()
    {
        _configs.Clear();
        _cardItems.Clear();

        await DataSaveHelper.LoadJsonData(_configs);

        foreach (var config in _configs.Values.OfType<CardDataConfig>())
        {
            EnsureCollections(config);
            _cardItems.Add(config);
        }

        _cardItems.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
    }

    private void EnsureCollections(CardDataConfig card)
    {
        if (card == null)
            return;

        card.ConfigType = card.GetType().Name;
        card.CardTypeId ??= ResolveDefaultTypeId(card);

        if (card is CardWithStatModifiersConfig statsCard)
        {
            statsCard.StatModifiers ??= new List<StatModifier>();
        }

        if (card is TreasureCardConfig treasureCard)
        {
            treasureCard.Equipment ??= new EquipmentConfig();
            treasureCard.Equipment.Overrides ??= new List<AppearanceOverride>();
        }

        if (card is MonsterCardConfig monsterCard)
        {
            monsterCard.Parameters ??= new MonsterParameters();
        }
    }

    private void OnSelectionChanged(IEnumerable<object> selectedItems)
    {
        var card = selectedItems?.FirstOrDefault() as CardDataConfig;
        _selectedCard = card;
        UpdateDetailPanel(card);
        UpdateButtonsState();
    }

    private void UpdateDetailPanel(CardDataConfig card)
    {
        if (_detailPanel == null)
            return;

        _detailPanel.Clear();

        if (card == null)
        {
            _detailPanel.Add(new Label("Выберите карту из списка или создайте новую."));
            return;
        }

        EnsureCollections(card);

        var header = new Label($"{card.Id}") { name = "CardHeader" };
        header.AddToClassList("custom-label");
        _detailPanel.Add(header);

        var variantField = new EnumField("Тип конфигурации", GetVariant(card));
        variantField.RegisterValueChangedCallback(evt =>
        {
            ApplyVariantChange(card, (CardConfigVariant)evt.newValue);
        });
        _detailPanel.Add(variantField);

        _detailPanel.Add(CreateIdField(card));
        _detailPanel.Add(CreateTextField("Card Type Id", card.CardTypeId, newValue =>
        {
            card.CardTypeId = newValue;
            MarkDirty();
        }));
        _detailPanel.Add(CreateTextField("Name", card.Name, newValue =>
        {
            card.Name = newValue;
            MarkDirty();
            RefreshCardList(card);
        }));
        _detailPanel.Add(CreateMultilineField("Description", card.Description, newValue =>
        {
            card.Description = newValue;
            MarkDirty();
        }));

        _detailPanel.Add(CreateEnumField("Kind", card.Kind, newValue =>
        {
            card.Kind = (CardKind)newValue;
            MarkDirty();
        }));

        _detailPanel.Add(CreateLayerSelector(
            "Main Layer Id",
            card,
            c => c.MainLayerId,
            (c, value) => c.MainLayerId = value));

        _detailPanel.Add(CreateLayerSelector(
            "Background Layer Id",
            card,
            c => c.BackgroundLayerId,
            (c, value) => c.BackgroundLayerId = value));

        if (card is TreasureCardConfig treasureCard)
        {
            _detailPanel.Add(CreateEquipmentSection(treasureCard));
        }

        if (card is CardWithStatModifiersConfig statsCard)
        {
            _detailPanel.Add(CreateStatModifiersSection(statsCard));
        }

        if (card is MonsterCardConfig monsterCard)
        {
            _detailPanel.Add(CreateMonsterSection(monsterCard));
        }
    }

    private VisualElement CreateIdField(CardDataConfig card)
    {
        return CreateTextField("Id", card.Id, newValue => UpdateCardId(card, newValue));
    }

    private VisualElement CreateTextField(string label, string initialValue, Action<string> onChanged)
    {
        var field = new TextField(label) { value = initialValue ?? string.Empty };
        field.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke(evt.newValue);
        });
        return field;
    }

    private VisualElement CreateMultilineField(string label, string initialValue, Action<string> onChanged)
    {
        var field = new TextField(label)
        {
            value = initialValue ?? string.Empty,
            multiline = true
        };
        field.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke(evt.newValue);
        });
        return field;
    }

    private VisualElement CreateEnumField(string label, Enum currentValue, Action<Enum> onChanged)
    {
        var field = new EnumField(label, currentValue);
        field.RegisterValueChangedCallback(evt =>
        {
            onChanged?.Invoke((Enum)evt.newValue);
        });
        return field;
    }

    private VisualElement CreateLayerSelector(
        string fieldLabel,
        CardDataConfig card,
        Func<CardDataConfig, string> getter,
        Action<CardDataConfig, string> setter,
        bool refreshListOnChange = false)
    {
        var sectionName = fieldLabel.Replace(" ", string.Empty);
        var container = new VisualElement { name = $"{sectionName}Section" };
        container.style.marginBottom = 6;

        var textField = new TextField(fieldLabel)
        {
            value = getter(card) ?? string.Empty
        };

        var iconPreview = new Image
        {
            name = $"{sectionName}Preview",
            scaleMode = ScaleMode.ScaleToFit
        };
        iconPreview.AddToClassList("icon-frame");
        iconPreview.style.width = 128;
        iconPreview.style.height = 128;
        UpdateIconPreview(iconPreview, textField.value);

        var objectField = new ObjectField("Sprite")
        {
            objectType = typeof(Sprite),
            allowSceneObjects = false
        };
        objectField.SetValueWithoutNotify(LoadSpriteAsset(textField.value));

        void ApplyPath(string path)
        {
            var normalized = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim();
            setter(card, normalized);
            MarkDirty();
            UpdateIconPreview(iconPreview, normalized);

            if (refreshListOnChange)
            {
                RefreshCardList(card);
            }
        }

        textField.RegisterValueChangedCallback(evt =>
        {
            var newPath = evt.newValue ?? string.Empty;
            ApplyPath(newPath);
            objectField.SetValueWithoutNotify(LoadSpriteAsset(newPath));
        });

        objectField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue is Sprite sprite)
            {
                if (!TryGetResourcesPath(sprite, out var resourcePath))
                {
                    EditorUtility.DisplayDialog("Ошибка ресурсов", "Выбранный ассет должен находиться внутри папки Resources.", "OK");
                    objectField.SetValueWithoutNotify(evt.previousValue as Sprite);
                    return;
                }

                textField.SetValueWithoutNotify(resourcePath);
                ApplyPath(resourcePath);
            }
            else
            {
                textField.SetValueWithoutNotify(string.Empty);
                ApplyPath(string.Empty);
            }
        });

        var reloadButton = new Button(() =>
        {
            var currentPath = getter(card) ?? string.Empty;
            textField.SetValueWithoutNotify(currentPath);
            objectField.SetValueWithoutNotify(LoadSpriteAsset(currentPath));
            UpdateIconPreview(iconPreview, currentPath);
        })
        {
            text = "Обновить превью"
        };

        container.Add(textField);
        container.Add(iconPreview);
        container.Add(objectField);
        container.Add(reloadButton);

        return container;
    }

    private void UpdateIconPreview(Image iconPreview, string resourcePath)
    {
        if (iconPreview == null)
            return;

        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            iconPreview.image = null;
            iconPreview.tooltip = "Путь не задан";
            return;
        }

        var sprite = LoadSpriteAsset(resourcePath);
        if (sprite != null && sprite.texture != null)
        {
            iconPreview.image = sprite.texture;
            iconPreview.tooltip = resourcePath;
        }
        else
        {
            iconPreview.image = null;
            iconPreview.tooltip = $"Не удалось загрузить ресурс: {resourcePath}";
        }
    }

    private Sprite LoadSpriteAsset(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
            return null;

        return Resources.Load<Sprite>(resourcePath);
    }

    private bool TryGetResourcesPath(UnityEngine.Object asset, out string resourcePath)
    {
        resourcePath = string.Empty;
        if (asset == null)
            return false;

        var assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
            return false;

        const string resourcesMarker = "/Resources/";
        var index = assetPath.IndexOf(resourcesMarker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return false;

        var startIndex = index + resourcesMarker.Length;
        var extensionIndex = assetPath.LastIndexOf('.');
        if (extensionIndex <= startIndex)
        {
            resourcePath = assetPath.Substring(startIndex).Replace("\\", "/");
        }
        else
        {
            resourcePath = assetPath.Substring(startIndex, extensionIndex - startIndex).Replace("\\", "/");
        }

        return true;
    }

    private VisualElement CreateEquipmentSection(TreasureCardConfig card)
    {
        EnsureCollections(card);

        var foldout = new Foldout { text = "Equipment", value = true };

        var slotField = new EnumField("Slot", card.Equipment.Slot);
        slotField.RegisterValueChangedCallback(evt =>
        {
            card.Equipment.Slot = (EquipmentSlot)evt.newValue;
            MarkDirty();
        });
        foldout.Add(slotField);

        foldout.Add(CreateTextField("Description", card.Equipment.Description, newValue =>
        {
            card.Equipment.Description = newValue;
            MarkDirty();
        }));

        foldout.Add(CreateAppearanceOverridesSection(card));

        return foldout;
    }

    private VisualElement CreateAppearanceOverridesSection(TreasureCardConfig card)
    {
        var overridesFoldout = new Foldout { text = "Appearance Overrides", value = false };

        var listContainer = new VisualElement { name = "OverridesContainer" };
        listContainer.style.flexDirection = FlexDirection.Column;

        void Rebuild()
        {
            listContainer.Clear();

            for (int i = 0; i < card.Equipment.Overrides.Count; i++)
            {
                var overrideData = card.Equipment.Overrides[i];
                var row = new VisualElement { name = $"Override_{i}" };
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var itemField = new TextField("Item") { value = overrideData.Item ?? string.Empty };
                itemField.style.flexGrow = 1;
                itemField.RegisterValueChangedCallback(evt =>
                {
                    overrideData.Item = evt.newValue;
                    MarkDirty();
                });

                var indexField = new IntegerField("Index") { value = overrideData.Index };
                indexField.RegisterValueChangedCallback(evt =>
                {
                    overrideData.Index = evt.newValue;
                    MarkDirty();
                });

                var removeButton = new Button(() =>
                {
                    card.Equipment.Overrides.RemoveAt(i);
                    MarkDirty();
                    Rebuild();
                })
                {
                    text = "X",
                    tooltip = "Удалить переопределение"
                };

                row.Add(itemField);
                row.Add(indexField);
                row.Add(removeButton);
                listContainer.Add(row);
            }
        }

        Rebuild();

        var addButton = new Button(() =>
        {
            card.Equipment.Overrides.Add(new AppearanceOverride());
            MarkDirty();
            Rebuild();
        })
        {
            text = "Добавить"
        };

        overridesFoldout.Add(listContainer);
        overridesFoldout.Add(addButton);

        return overridesFoldout;
    }

    private VisualElement CreateStatModifiersSection(CardWithStatModifiersConfig card)
    {
        var foldout = new Foldout { text = "Stat Modifiers", value = false };
        var listContainer = new VisualElement { name = "StatModifiersContainer" };
        listContainer.style.flexDirection = FlexDirection.Column;

        void Rebuild()
        {
            listContainer.Clear();

            for (int i = 0; i < card.StatModifiers.Count; i++)
            {
                var modifier = card.StatModifiers[i];
                var row = new VisualElement { name = $"StatModifier_{i}" };
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var statField = new EnumField("Stat", modifier.Stat);
                statField.RegisterValueChangedCallback(evt =>
                {
                    modifier.Stat = (PlayerStat)evt.newValue;
                    MarkDirty();
                });

                var valueField = new FloatField("Value") { value = modifier.Value };
                valueField.RegisterValueChangedCallback(evt =>
                {
                    modifier.Value = evt.newValue;
                    MarkDirty();
                });

                var removeButton = new Button(() =>
                {
                    card.StatModifiers.RemoveAt(i);
                    MarkDirty();
                    Rebuild();
                })
                {
                    text = "X",
                    tooltip = "Удалить модификатор"
                };

                statField.style.flexGrow = 1;
                valueField.style.flexGrow = 1;

                row.Add(statField);
                row.Add(valueField);
                row.Add(removeButton);
                listContainer.Add(row);
            }
        }

        Rebuild();

        var addButton = new Button(() =>
        {
            card.StatModifiers.Add(new StatModifier { Stat = PlayerStat.Health, Value = 0f });
            MarkDirty();
            Rebuild();
        })
        {
            text = "Добавить"
        };

        foldout.Add(listContainer);
        foldout.Add(addButton);

        return foldout;
    }

    private VisualElement CreateMonsterSection(MonsterCardConfig card)
    {
        var foldout = new Foldout { text = "Monster", value = true };

        foldout.Add(CreateTextField("View Id", card.ViewId, newValue =>
        {
            card.ViewId = newValue;
            MarkDirty();
        }));

        var healthField = new FloatField("Health") { value = card.Parameters.Health };
        healthField.RegisterValueChangedCallback(evt =>
        {
            card.Parameters.Health = evt.newValue;
            MarkDirty();
        });
        foldout.Add(healthField);

        var damageField = new FloatField("Damage") { value = card.Parameters.Damage };
        damageField.RegisterValueChangedCallback(evt =>
        {
            card.Parameters.Damage = evt.newValue;
            MarkDirty();
        });
        foldout.Add(damageField);

        var rewardField = new FloatField("Reward") { value = card.Parameters.Reward };
        rewardField.RegisterValueChangedCallback(evt =>
        {
            card.Parameters.Reward = evt.newValue;
            MarkDirty();
        });
        foldout.Add(rewardField);

        return foldout;
    }

    private CardConfigVariant GetVariant(CardDataConfig card)
    {
        return card switch
        {
            MonsterCardConfig => CardConfigVariant.Monster,
            EventCardConfig => CardConfigVariant.Event,
            TreasureCardConfig => CardConfigVariant.Treasure,
            DoorCardConfig => CardConfigVariant.Door,
            CardWithStatModifiersConfig => CardConfigVariant.StatModifiers,
            _ => CardConfigVariant.Base
        };
    }

    private void ApplyVariantChange(CardDataConfig source, CardConfigVariant targetVariant)
    {
        if (source == null)
            return;

        var newCard = ConvertCardVariant(source, targetVariant);
        if (newCard == null || ReferenceEquals(source, newCard))
            return;

        ReplaceCardReference(source, newCard);
        _selectedCard = newCard;
        RefreshCardList(newCard);
        MarkDirty();
    }

    private CardDataConfig ConvertCardVariant(CardDataConfig source, CardConfigVariant targetVariant)
    {
        var currentVariant = GetVariant(source);
        if (currentVariant == targetVariant)
            return source;

        CardDataConfig CreateInstance(CardConfigVariant variant) => variant switch
        {
            CardConfigVariant.Base => new CardDataConfig(),
            CardConfigVariant.StatModifiers => new CardWithStatModifiersConfig(),
            CardConfigVariant.Treasure => new TreasureCardConfig(),
            CardConfigVariant.Door => new DoorCardConfig(),
            CardConfigVariant.Event => new EventCardConfig(),
            CardConfigVariant.Monster => new MonsterCardConfig(),
            _ => new CardDataConfig()
        };

        var target = CreateInstance(targetVariant);
        CopyBaseCardData(source, target, targetVariant);

        if (target is CardWithStatModifiersConfig statsTarget)
        {
            statsTarget.StatModifiers = CloneStatModifiers(source as CardWithStatModifiersConfig);
        }

        if (target is TreasureCardConfig treasureTarget)
        {
            treasureTarget.Equipment = CloneEquipment((source as TreasureCardConfig)?.Equipment);
        }

        if (target is MonsterCardConfig monsterTarget && source is MonsterCardConfig monsterSource)
        {
            monsterTarget.Parameters = CloneMonsterParameters(monsterSource.Parameters);
            monsterTarget.ViewId = monsterSource.ViewId;
        }

        EnsureCollections(target);
        return target;
    }

    private void ReplaceCardReference(CardDataConfig oldCard, CardDataConfig newCard)
    {
        if (oldCard == null || newCard == null)
            return;

        if (_configs.ContainsKey(oldCard.Id))
        {
            _configs[oldCard.Id] = newCard;
        }

        var index = _cardItems.IndexOf(oldCard);
        if (index >= 0)
        {
            _cardItems[index] = newCard;
        }
    }

    private static void CopyBaseCardData(CardDataConfig source, CardDataConfig target, CardConfigVariant targetVariant)
    {
        target.Id = source.Id;
        target.ConfigType = target.GetType().Name;
        target.Kind = ResolveKindForVariant(targetVariant, source.Kind);
        target.CardTypeId = ResolveTypeIdForVariant(targetVariant, source.CardTypeId, target);
        target.Name = source.Name;
        target.Description = source.Description;
        target.MainLayerId = source.MainLayerId;
        target.BackgroundLayerId = source.BackgroundLayerId;
    }

    private static CardKind ResolveKindForVariant(CardConfigVariant variant, CardKind current)
    {
        return variant switch
        {
            CardConfigVariant.Treasure => CardKind.Treasure,
            CardConfigVariant.Door => CardKind.Door,
            CardConfigVariant.Event => CardKind.Event,
            CardConfigVariant.Monster => CardKind.Monster,
            _ => current
        };
    }

    private static string ResolveTypeIdForVariant(CardConfigVariant variant, string currentTypeId, CardDataConfig target)
    {
        return variant switch
        {
            CardConfigVariant.Treasure => CardTypeIds.Equipment,
            CardConfigVariant.Event => CardTypeIds.Event,
            CardConfigVariant.Monster => CardTypeIds.Monster,
            CardConfigVariant.Door => CardTypeIds.Door,
            _ => ResolveDefaultTypeId(target, currentTypeId)
        };
    }

    private static string ResolveDefaultTypeId(CardDataConfig config, string current = null)
    {
        if (!string.IsNullOrWhiteSpace(current))
            return current;

        if (config == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(config.CardTypeId))
            return config.CardTypeId;

        if (!string.IsNullOrWhiteSpace(config.ConfigType))
            return config.ConfigType;

        return config.GetType().Name;
    }

    private static List<StatModifier> CloneStatModifiers(CardWithStatModifiersConfig source)
    {
        if (source?.StatModifiers == null)
            return new List<StatModifier>();

        return source.StatModifiers
            .Select(modifier => new StatModifier { Stat = modifier.Stat, Value = modifier.Value })
            .ToList();
    }

    private static EquipmentConfig CloneEquipment(EquipmentConfig equipment)
    {
        if (equipment == null)
            return new EquipmentConfig();

        return new EquipmentConfig
        {
            Slot = equipment.Slot,
            Description = equipment.Description,
            Overrides = CloneAppearanceOverrides(equipment.Overrides)
        };
    }

    private static MonsterParameters CloneMonsterParameters(MonsterParameters parameters)
    {
        if (parameters == null)
            return new MonsterParameters();

        return new MonsterParameters
        {
            Health = parameters.Health,
            Damage = parameters.Damage,
            Reward = parameters.Reward
        };
    }

    private static List<AppearanceOverride> CloneAppearanceOverrides(IEnumerable<AppearanceOverride> overrides)
    {
        if (overrides == null)
            return new List<AppearanceOverride>();

        return overrides
            .Select(item => new AppearanceOverride { Item = item.Item, Index = item.Index })
            .ToList();
    }

    private void OnAddCard()
    {
        var newCard = new TreasureCardConfig
        {
            Id = GenerateUniqueId(),
            Kind = CardKind.Treasure,
            CardTypeId = CardTypeIds.Equipment,
            Name = "New Card",
            Description = string.Empty,
            MainLayerId = string.Empty,
            BackgroundLayerId = string.Empty,
            Equipment = new EquipmentConfig(),
            StatModifiers = new List<StatModifier>()
        };

        EnsureCollections(newCard);
        _configs[newCard.Id] = newCard;
        _cardItems.Add(newCard);
        _cardItems.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

        _cardListView?.RefreshItems();
        SelectCard(newCard);
        MarkDirty();
    }

    private void OnRemoveCard()
    {
        if (_selectedCard == null)
            return;

        if (!EditorUtility.DisplayDialog("Удалить конфиг", $"Вы уверены, что хотите удалить карту {_selectedCard.Id}?", "Удалить", "Отмена"))
            return;

        _configs.Remove(_selectedCard.Id);
        _cardItems.Remove(_selectedCard);
        var cardToSelect = _cardItems.FirstOrDefault();

        _cardListView?.RefreshItems();
        SelectCard(cardToSelect);
        MarkDirty();
    }

    private void OnSaveCards()
    {
        DataSaveHelper.PatchSourceData(_configs);
        _hasUnsavedChanges = false;
        UpdateButtonsState();
        UpdateStatus();
    }

    private void UpdateCardId(CardDataConfig card, string newId)
    {
        if (card == null)
            return;

        newId = newId?.Trim();
        if (string.IsNullOrEmpty(newId))
        {
            EditorUtility.DisplayDialog("Недопустимый идентификатор", "Идентификатор не может быть пустым.", "OK");
            UpdateDetailPanel(card);
            return;
        }

        if (string.Equals(newId, card.Id, StringComparison.Ordinal))
            return;

        if (_configs.ContainsKey(newId))
        {
            EditorUtility.DisplayDialog("Недопустимый идентификатор", $"Конфигурация с Id \"{newId}\" уже существует.", "OK");
            UpdateDetailPanel(card);
            return;
        }

        _configs.Remove(card.Id);
        card.Id = newId;
        _configs[newId] = card;

        MarkDirty();
        RefreshCardList(card);
    }

    private void RefreshCardList(CardDataConfig cardToSelect = null)
    {
        _cardItems.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        _cardListView?.RefreshItems();
        if (cardToSelect != null)
        {
            SelectCard(cardToSelect);
        }
    }

    private void SelectCard(CardDataConfig card)
    {
        _selectedCard = card;
        if (_cardListView == null)
            return;

        if (card == null)
        {
            _cardListView.ClearSelection();
            UpdateDetailPanel(null);
            return;
        }

        var index = _cardItems.IndexOf(card);
        if (index >= 0)
        {
            _cardListView.SetSelection(index);
        }
        else
        {
            _cardListView.ClearSelection();
        }

        UpdateDetailPanel(card);
    }

    private void UpdateButtonsState()
    {
        _removeButton?.SetEnabled(_selectedCard != null);
        _saveButton?.SetEnabled(_hasUnsavedChanges);
    }

    private void MarkDirty()
    {
        _hasUnsavedChanges = true;
        UpdateButtonsState();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (_statusLabel == null)
            return;

        _statusLabel.text = _hasUnsavedChanges ? "Есть несохранённые изменения" : string.Empty;
    }

    private static string FormatCardLabel(CardDataConfig card)
    {
        if (card == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(card.Name) ? card.Id : $"{card.Id} — {card.Name}";
    }

    private string GenerateUniqueId()
    {
        const string prefix = "card_new_";
        var counter = 1;
        string candidate;
        do
        {
            candidate = $"{prefix}{counter}";
            counter++;
        } while (_configs.ContainsKey(candidate));

        return candidate;
    }
}
