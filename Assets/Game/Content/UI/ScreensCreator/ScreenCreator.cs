using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class ScreenCreator : EditorWindow
{
    private TextField _idField;
    private ObjectField _scriptField;
    private ObjectField _prefabField;
    private DropdownField _layerDropdown;
    private Button _saveButton;

    private readonly Dictionary<string, BaseConfig> _dictionary = new();

    [MenuItem("CARD GAME/ScreenCreator")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<ScreenCreator>();
        wnd.titleContent = new GUIContent("Screen Creator");
        wnd.minSize = new Vector2(350, 250);
    }

    public void CreateGUI()
    {
        // Основной контейнер с отступами
        var container = new VisualElement
        {
            style =
            {
                paddingTop = 10,
                paddingLeft = 10,
                paddingRight = 10,
                paddingBottom = 10
            }
        };
        rootVisualElement.Add(container);

        // Поле для ввода ID
        _idField = new TextField("ID экрана")
        {
            tooltip = "Уникальный идентификатор экрана"
        };
        container.Add(_idField);

        // Поле выбора скрипта
        _scriptField = new ObjectField("Скрипт экрана")
        {
            objectType = typeof(MonoScript),
            allowSceneObjects = false
        };
        container.Add(_scriptField);

        // Поле выбора префаба
        _prefabField = new ObjectField("Префаб экрана")
        {
            objectType = typeof(GameObject),
            allowSceneObjects = false
        };
        container.Add(_prefabField);

        // Выпадающий список для слоев
        _layerDropdown = new DropdownField("Слой UI")
        {
            choices = new List<string>(Enum.GetNames(typeof(UILayer))),
            value = UILayer.Window.ToString(),
            index = 0
        };
        container.Add(_layerDropdown);

        // Кнопка сохранения
        _saveButton = new Button(OnSaveClicked)
        {
            text = "Сохранить конфигурацию",
            style =
            {
                marginTop = 20,
                height = 30,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        container.Add(_saveButton);

        // Добавляем разделители для улучшения визуального восприятия
        AddSeparator(container);
    }

    private void AddSeparator(VisualElement parent)
    {
        parent.Add(new VisualElement
        {
            style =
            {
                height = 1,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                marginTop = 10,
                marginBottom = 10
            }
        });
    }

    private string GetFullTypeName(MonoScript monoScript)
    {
        var scriptType = monoScript.GetClass();
        return scriptType?.FullName;
    }

    private async void OnSaveClicked()
    {
        string id = _idField.value;
        MonoScript selectedScript = _scriptField.value as MonoScript;
        GameObject selectedPrefab = _prefabField.value as GameObject;
        string layerInfo = _layerDropdown.value;
        if (string.IsNullOrEmpty(id))
        {
            EditorUtility.DisplayDialog("Ошибка", "Введите ID!", "OK");
            return;
        }

        if (selectedScript == null || selectedPrefab == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Выберите скрипт и префаб!", "OK");
            return;
        }

        Debug.Log($"Сохранено: ID={id}, Script={selectedScript.name}, Prefab={selectedPrefab.name}");

        await DataSaveHelper.LoadJsonData(_dictionary);
        string path = AssetDatabase.GetAssetPath(selectedPrefab);
        Enum.TryParse(layerInfo, out UILayer layer);
        var screenConfig = new UIDataConfig()
        {
            Id = id,
            PrefabPath = path,
            Layer = layer,
            ScreenType = GetFullTypeName(selectedScript)
        };
        _dictionary.Add(id, screenConfig);
        DataSaveHelper.PatchSourceData(_dictionary);
        EditorUtility.DisplayDialog("Успех", $"Объект '{id}' создан!", "OK");
    }
}