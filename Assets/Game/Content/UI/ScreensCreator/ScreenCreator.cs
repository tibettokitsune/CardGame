using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ScreenCreator : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset;

    private TextField _idField;
    private ObjectField _scriptField;
    private ObjectField _prefabField;
    private Button _saveButton;

    [MenuItem("CARD GAME/ScreenCreator")]
    public static void ShowWindow()
    {
        ScreenCreator wnd = GetWindow<ScreenCreator>();
        wnd.titleContent = new GUIContent("Screen Creator");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Создание ObjectField для скриптов
        _scriptField = new ObjectField("Выберите скрипт");
        _scriptField.objectType = typeof(MonoScript);
        rootVisualElement.Add(_scriptField);

        // Создание ObjectField для префабов
        _prefabField = new ObjectField("Выберите префаб");
        _prefabField.objectType = typeof(GameObject);
        rootVisualElement.Add(_prefabField);

        // Загружаем UXML
        if (m_VisualTreeAsset != null)
        {
            VisualElement ui = m_VisualTreeAsset.Instantiate();
            root.Add(ui);

            // Получаем элементы UI
            _idField = root.Q<TextField>("id-field");
            _saveButton = root.Q<Button>("save-button");

            // Настраиваем ObjectField для скрипта
            _scriptField.objectType = typeof(MonoScript);
            _scriptField.label = "Выберите скрипт";

            // Настраиваем ObjectField для префаба
            _prefabField.objectType = typeof(GameObject);
            _prefabField.label = "Выберите префаб";

            // Обработчик кнопки сохранения
            _saveButton.clicked += OnSaveClicked;
        }
    }

    private void OnSaveClicked()
    {
        string id = _idField.value;
        MonoScript selectedScript = _scriptField.value as MonoScript;
        GameObject selectedPrefab = _prefabField.value as GameObject;

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

        // Здесь можно добавить логику сохранения (например, в ScriptableObject или JSON)
        Debug.Log($"Сохранено: ID={id}, Script={selectedScript.name}, Prefab={selectedPrefab.name}");

        // Пример: Создание нового объекта с выбранным префабом и скриптом
        GameObject newObj = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
        if (newObj != null)
        {
            newObj.name = $"Screen_{id}";
            if (!newObj.TryGetComponent(selectedScript.GetClass(), out _))
            {
                newObj.AddComponent(selectedScript.GetClass());
            }

            EditorUtility.DisplayDialog("Успех", $"Объект '{newObj.name}' создан!", "OK");
        }
    }
}