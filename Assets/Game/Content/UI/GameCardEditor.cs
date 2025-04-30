using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
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

    private ListView _listView;
    private VisualElement _detailPanel;
    private List<CardDataConfig> _cardItems;
    private readonly Dictionary<string, BaseConfig> _dictionary = new();
    public async void CreateGUI()
    {
        var ui = m_VisualTreeAsset.CloneTree();
        rootVisualElement.Add(ui);

        _listView = rootVisualElement.Q<ListView>("ListView");
        _detailPanel = rootVisualElement.Q<VisualElement>("DetailPanel");

        await LoadJsonData();

        _cardItems = _dictionary
            .Where(x => x.Value is CardDataConfig)
            .Select(x => x.Value as CardDataConfig)
            .ToList();

        _listView.itemsSource = _cardItems;
        _listView.makeItem = () => new Label();
        _listView.bindItem = (el, index) =>
        {
            (el as Label).text = _cardItems[index].Id;
        };

        _listView.selectionType = SelectionType.Single;

        _listView.selectionChanged += OnItemSelected;
    }

    private void OnItemSelected(IEnumerable<object> selectedItems)
    {
        _detailPanel.Clear();

        var selected = selectedItems.FirstOrDefault() as CardDataConfig;
        if (selected == null) return;
        
        var idField = new TextField("ID") { value = selected.Id };
        idField.RegisterValueChangedCallback(evt =>
        {
            selected.Id = evt.newValue;
        });
        
        var descField = new TextField("Description") { value = selected.Description };
        idField.RegisterValueChangedCallback(evt =>
        {
            selected.Id = evt.newValue;
        });
        
        var mainLayerField = new TextField("MainLayerId") { value = selected.MainLayerId };
        idField.RegisterValueChangedCallback(evt =>
        {
            selected.Id = evt.newValue;
        });
        
        var bgLayerField = new TextField("BackgroundLayerId") { value = selected.BackgroundLayerId };
        idField.RegisterValueChangedCallback(evt =>
        {
            selected.Id = evt.newValue;
        });
        
        _detailPanel.Add(idField);
        _detailPanel.Add(descField);
        _detailPanel.Add(mainLayerField);
        _detailPanel.Add(bgLayerField);
    }
    
    private async Task LoadJsonData()
    {
        var req = Resources.LoadAsync<TextAsset>("configs");
        await req;

        var jsonFile = req.asset as TextAsset;
        if (jsonFile == null)
        {
            Debug.LogError("Не удалось загрузить JSON файл.");
            return;
        }

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        var data = JsonConvert.DeserializeObject<ConfigsWrapper>(jsonFile.text, settings);

        foreach (var config in data.UIConfigs)
            _dictionary.TryAdd(config.Id, config);

        foreach (var config in data.CardLayersConfigs)
            _dictionary.TryAdd(config.Id, config);

        foreach (var config in data.CardConfigs)
            _dictionary.TryAdd(config.Id, config);
    }

}
