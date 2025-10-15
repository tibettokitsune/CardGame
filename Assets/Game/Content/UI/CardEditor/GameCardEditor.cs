using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using UnityEditor;
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
        card.ConfigType = nameof(CardDataConfig);
        card.StatModifiers ??= new List<StatModifier>();
        if (card.Equipment == null)
        {
            card.Equipment = new EquipmentConfig();
        }

        card.Equipment.Overrides ??= new List<AppearanceOverride>();
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

        var header = new Label($"{card.Id}") { name = "CardHeader" };
        header.AddToClassList("custom-label");
        _detailPanel.Add(header);

        _detailPanel.Add(CreateIdField(card));
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

        _detailPanel.Add(CreateTextField("Main Layer Id", card.MainLayerId, newValue =>
        {
            card.MainLayerId = newValue;
            MarkDirty();
        }));

        _detailPanel.Add(CreateTextField("Background Layer Id", card.BackgroundLayerId, newValue =>
        {
            card.BackgroundLayerId = newValue;
            MarkDirty();
        }));

        _detailPanel.Add(CreateEquipmentSection(card));
        _detailPanel.Add(CreateStatModifiersSection(card));
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

    private VisualElement CreateEquipmentSection(CardDataConfig card)
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

    private VisualElement CreateAppearanceOverridesSection(CardDataConfig card)
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

    private VisualElement CreateStatModifiersSection(CardDataConfig card)
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

    private void OnAddCard()
    {
        var newCard = new CardDataConfig
        {
            Id = GenerateUniqueId(),
            ConfigType = nameof(CardDataConfig),
            Kind = CardKind.Treasure,
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
