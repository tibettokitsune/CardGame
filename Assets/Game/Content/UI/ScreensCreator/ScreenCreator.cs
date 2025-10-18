using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class ScreenCreator : EditorWindow
{
    private const string DefaultNamespace = "Game.Scripts.Gameplay.ViewsLayer";
    private const string DefaultScriptFolderPath = "Assets/Game/Scripts/Gameplay/ViewsLayer";
    private const string DefaultPrefabFolderPath = "Assets/Game/Resources/Prefabs/UI";
    private const string DefaultBasePrefabPath = "Assets/Game/Resources/Prefabs/UI/BaseCanvas.prefab";
    private const string PendingCreationKey = "CARD_GAME_SCREEN_CREATOR_PENDING";
    private const float ItemSpacing = 8f;
    private static readonly Regex IdentifierRegex = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    private TextField _idField;
    private TextField _classField;
    private TextField _namespaceField;
    private ObjectField _scriptFolderField;
    private ObjectField _prefabFolderField;
    private ObjectField _basePrefabField;
    private DropdownField _layerDropdown;
    private Toggle _hideOtherToggle;
    private Button _saveButton;
    private bool _classNameEdited;

    private readonly Dictionary<string, BaseConfig> _configsCache = new();

    [Serializable]
    private class PendingCreationData
    {
        public string ClassName;
        public string FullTypeName;
        public string PrefabAssetPath;
        public string BasePrefabPath;
    }

    private class CreationRequest
    {
        public string ConfigId;
        public string ClassName;
        public string Namespace;
        public string ScriptAssetPath;
        public string PrefabAssetPath;
        public string ResourcesPath;
        public string BasePrefabPath;
        public UILayer Layer;
        public bool HideOtherScreensOnLayer;
    }

    static ScreenCreator()
    {
        EditorApplication.update += TryFinalizePendingCreation;
    }

    [MenuItem("CARD GAME/Screen Creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScreenCreator>();
        window.titleContent = new GUIContent("Screen Creator");
        window.minSize = new Vector2(380, 420);
    }

    public void CreateGUI()
    {
        rootVisualElement.Clear();

        var container = new ScrollView
        {
            style =
            {
                paddingLeft = 10,
                paddingRight = 10,
                paddingTop = 10,
                paddingBottom = 10
            }
        };
        container.contentContainer.style.flexDirection = FlexDirection.Column;
        rootVisualElement.Add(container);

        _idField = new TextField("ID экрана")
        {
            tooltip = "Уникальный идентификатор конфигурации и ресурса"
        };
        _idField.RegisterValueChangedCallback(evt =>
        {
            if (!_classNameEdited)
            {
                _classField.value = BuildDefaultClassName(evt.newValue);
            }
        });
        AddWithSpacing(container, _idField);

        _classField = new TextField("Имя класса")
        {
            value = BuildDefaultClassName(string.Empty),
            tooltip = "C# класс, который будет создан и помечен атрибутом UIScreen"
        };
        _classField.RegisterValueChangedCallback(_ => _classNameEdited = true);
        AddWithSpacing(container, _classField);

        _namespaceField = new TextField("Namespace")
        {
            value = DefaultNamespace
        };
        AddWithSpacing(container, _namespaceField);

        _scriptFolderField = CreateFolderField("Папка со скриптом", DefaultScriptFolderPath);
        AddWithSpacing(container, _scriptFolderField);

        _prefabFolderField = CreateFolderField("Папка для префаба", DefaultPrefabFolderPath);
        AddWithSpacing(container, _prefabFolderField);

        _basePrefabField = new ObjectField("Базовый префаб")
        {
            objectType = typeof(GameObject),
            allowSceneObjects = false,
            value = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBasePrefabPath)
        };
        AddWithSpacing(container, _basePrefabField);

        _layerDropdown = new DropdownField("Слой UI", new List<string>(Enum.GetNames(typeof(UILayer))), 0);
        AddWithSpacing(container, _layerDropdown);

        _hideOtherToggle = new Toggle("Скрывать другие окна слоя")
        {
            value = false
        };
        AddWithSpacing(container, _hideOtherToggle);

        AddWithSpacing(container, CreateSeparator());
        AddWithSpacing(container, CreateInfoLabel());

        _saveButton = new Button(OnCreateClicked)
        {
            text = "Создать окно",
            style =
            {
                height = 34,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginTop = 12
            }
        };
        container.Add(_saveButton);
    }

    private static ObjectField CreateFolderField(string label, string defaultPath)
    {
        return new ObjectField(label)
        {
            objectType = typeof(DefaultAsset),
            allowSceneObjects = false,
            value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultPath)
        };
    }

    private static VisualElement CreateSeparator()
    {
        return new VisualElement
        {
            style =
            {
                height = 1,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                marginTop = 10,
                marginBottom = 10
            }
        };
    }

    private static Label CreateInfoLabel()
    {
        return new Label("Инструмент создаст скрипт, конфигурацию и префаб-варинат.\n" +
                         "Префаб появится после завершения компиляции.")
        {
            style =
            {
                whiteSpace = WhiteSpace.Normal,
                unityFontStyleAndWeight = FontStyle.Italic,
                color = new Color(0.75f, 0.75f, 0.75f)
            }
        };
    }

    private async void OnCreateClicked()
    {
        _saveButton.SetEnabled(false);

        try
        {
            var request = BuildRequest();

            _configsCache.Clear();
            await DataSaveHelper.LoadJsonData(_configsCache);

            if (_configsCache.ContainsKey(request.ConfigId))
            {
                throw new InvalidOperationException($"Конфигурация с ID '{request.ConfigId}' уже существует.");
            }

            CreateScriptFile(request);
            QueuePrefabCreation(request);

            var config = new UIDataConfig
            {
                Id = request.ConfigId,
                PrefabPath = request.ResourcesPath,
                Layer = request.Layer,
                HideOtherScreensOnLayer = request.HideOtherScreensOnLayer,
                ScreenType = $"{request.Namespace}.{request.ClassName}"
            };

            _configsCache[request.ConfigId] = config;
            DataSaveHelper.PatchSourceData(_configsCache);

            EditorUtility.DisplayDialog(
                "Готово",
                $"Экран '{request.ClassName}' добавлен.\nПрефаб появится после компиляции скриптов.",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("Ошибка создания экрана", ex.Message, "OK");
        }
        finally
        {
            _saveButton.SetEnabled(true);
        }
    }

    private static void AddWithSpacing(VisualElement parent, VisualElement element)
    {
        if (element == null)
            return;

        element.style.marginBottom = ItemSpacing;
        parent.Add(element);
    }

    private CreationRequest BuildRequest()
    {
        var id = (_idField.value ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Введите ID экрана.");

        var className = (_classField.value ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(className))
            throw new InvalidOperationException("Введите имя класса.");

        if (!IdentifierRegex.IsMatch(className))
            throw new InvalidOperationException("Имя класса может содержать только буквы, цифры и символ '_', начинаться с буквы.");

        var ns = string.IsNullOrWhiteSpace(_namespaceField.value)
            ? DefaultNamespace
            : _namespaceField.value.Trim();

        if (!IsValidNamespace(ns))
            throw new InvalidOperationException("Namespace содержит недопустимые символы.");

        var scriptFolderPath = ResolveFolderPath(_scriptFolderField.value as DefaultAsset);
        if (string.IsNullOrEmpty(scriptFolderPath))
            throw new InvalidOperationException("Выберите папку для скрипта.");

        var prefabFolderPath = ResolveFolderPath(_prefabFolderField.value as DefaultAsset);
        if (string.IsNullOrEmpty(prefabFolderPath))
            throw new InvalidOperationException("Выберите папку для префаба.");

        var basePrefab = _basePrefabField.value as GameObject;
        if (basePrefab == null)
            throw new InvalidOperationException("Укажите базовый префаб.");

        if (!Enum.TryParse(_layerDropdown.value, out UILayer layer))
            layer = UILayer.Window;

        var scriptAssetPath = Path.Combine(scriptFolderPath, $"{className}.cs").Replace("\\", "/");
        if (File.Exists(scriptAssetPath))
            throw new InvalidOperationException($"Скрипт уже существует: {scriptAssetPath}");

        var prefabAssetPath = Path.Combine(prefabFolderPath, $"{className}.prefab").Replace("\\", "/");
        if (File.Exists(prefabAssetPath))
            throw new InvalidOperationException($"Префаб уже существует: {prefabAssetPath}");

        var resourcesPath = ToResourcesPath(prefabAssetPath);

        return new CreationRequest
        {
            ConfigId = id,
            ClassName = className,
            Namespace = ns,
            ScriptAssetPath = scriptAssetPath,
            PrefabAssetPath = prefabAssetPath,
            ResourcesPath = resourcesPath,
            BasePrefabPath = AssetDatabase.GetAssetPath(basePrefab),
            Layer = layer,
            HideOtherScreensOnLayer = _hideOtherToggle.value
        };
    }

    private static string ResolveFolderPath(DefaultAsset folderAsset)
    {
        if (folderAsset == null)
            return null;

        var path = AssetDatabase.GetAssetPath(folderAsset);
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            return null;

        return path;
    }

    private static bool IsValidNamespace(string ns)
    {
        if (string.IsNullOrWhiteSpace(ns))
            return false;

        var parts = ns.Split('.');
        return parts.All(part => IdentifierRegex.IsMatch(part));
    }

    private static string BuildDefaultClassName(string value)
    {
        var safe = ToPascalCase(value);
        return string.IsNullOrEmpty(safe) ? "NewUIScreen" : $"{safe}Screen";
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var tokens = value.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var token in tokens)
        {
            if (token.Length == 0)
                continue;

            sb.Append(char.ToUpperInvariant(token[0]));
            if (token.Length > 1)
                sb.Append(token.Substring(1));
        }

        return sb.ToString();
    }

    private static string ToResourcesPath(string assetPath)
    {
        const string marker = "/Resources/";
        var index = assetPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            throw new InvalidOperationException("Папка для префаба должна находиться внутри Resources.");

        var start = index + marker.Length;
        var relative = assetPath[start..].Replace("\\", "/");
        const string suffix = ".prefab";
        if (relative.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            relative = relative[..^suffix.Length];
        }
        return relative;
    }

    private static string GenerateScriptContents(CreationRequest request)
    {
        return
$@"using Game.Scripts.Infrastructure.UI;
using Game.Scripts.UI;
using UnityEngine;

namespace {request.Namespace}
{{
    [UIScreen(""{request.ConfigId}"")]
    public class {request.ClassName} : UIScreen
    {{
        // TODO: добавить ссылки на элементы интерфейса и обработчики
    }}
}}
";
    }

    private static void CreateScriptFile(CreationRequest request)
    {
        var directory = Path.GetDirectoryName(request.ScriptAssetPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(request.ScriptAssetPath, GenerateScriptContents(request));
    }

    private static void QueuePrefabCreation(CreationRequest request)
    {
        var pending = new PendingCreationData
        {
            ClassName = request.ClassName,
            FullTypeName = $"{request.Namespace}.{request.ClassName}",
            PrefabAssetPath = request.PrefabAssetPath,
            BasePrefabPath = request.BasePrefabPath
        };

        var payload = JsonUtility.ToJson(pending);
        SessionState.SetString(PendingCreationKey, payload);

        AssetDatabase.ImportAsset(request.ScriptAssetPath);
    }

    private static void TryFinalizePendingCreation()
    {
        if (EditorApplication.isCompiling)
            return;

        var payload = SessionState.GetString(PendingCreationKey, string.Empty);
        if (string.IsNullOrEmpty(payload))
            return;

        var data = JsonUtility.FromJson<PendingCreationData>(payload);
        if (data == null)
        {
            SessionState.EraseString(PendingCreationKey);
            return;
        }

        var screenType = ResolveScreenType(data.FullTypeName);
        if (screenType == null)
            return;

        SessionState.EraseString(PendingCreationKey);
        FinalizePrefabCreation(data, screenType);
    }

    private static Type ResolveScreenType(string fullTypeName)
    {
        var type = Type.GetType(fullTypeName);
        if (type != null)
            return type;

        foreach (var candidate in EnumerateScreenTypes())
        {
            if (candidate.FullName == fullTypeName)
                return candidate;
        }

        return null;
    }

    private static IEnumerable<Type> EnumerateScreenTypes()
    {
#if UNITY_2020_1_OR_NEWER
        return TypeCache.GetTypesDerivedFrom<Game.Scripts.UI.UIScreen>();
#else
        var baseType = typeof(Game.Scripts.UI.UIScreen);
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    return exception.Types?.Where(t => t != null) ?? Array.Empty<Type>();
                }
            })
            .Where(type => type != null && baseType.IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();
#endif
    }

    private static void FinalizePrefabCreation(PendingCreationData data, Type screenType)
    {
        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.BasePrefabPath);
        if (basePrefab == null)
        {
            Debug.LogError($"Base prefab not found: {data.BasePrefabPath}");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = data.ClassName;

        if (instance.GetComponent(screenType) == null)
        {
            instance.AddComponent(screenType);
        }

        PrefabUtility.SaveAsPrefabAsset(instance, data.PrefabAssetPath);
        Object.DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Префаб окна создан: {data.PrefabAssetPath}");
    }
}
