// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Database.DTO;
using Synty.SidekickCharacters.Enums;
using Synty.SidekickCharacters.Serialization;
using Synty.SidekickCharacters.SkinnedMesh;
using Synty.SidekickCharacters.UI;
using Synty.SidekickCharacters.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Synty.SidekickCharacters
{
    public class ModularCharacterWindow : EditorWindow
    {
        private const string _BASE_COLOR_SET_NAME = "Species";
        private const string _BASE_COLOR_SET_PATH = "Assets/Synty/SidekickCharacters/Resources/Species";
        private const string _BASE_MESH_NAME = "Meshes/SK_BaseModel";
        private const string _BASE_MATERIAL_NAME = "Materials/M_BaseMaterial";
        private const string _BLEND_GENDER_NAME = "masculineFeminine";
        private const string _BLEND_MUSCLE_NAME = "defaultBuff";
        private const string _BLEND_SHAPE_HEAVY_NAME = "defaultHeavy";
        private const string _BLEND_SHAPE_SKINNY_NAME = "defaultSkinny";
        private const string _AUTO_OPEN_STATE = "syntySkAutoOpenState";
        private const string _OUTPUT_MODEL_NAME = "Combined Character";
        private const string _PART_COUNT_BODY = " parts in library";
        private const string _TEXTURE_COLOR_NAME = "ColorMap.png";
        private const string _TEXTURE_METALLIC_NAME = "MetallicMap.png";
        private const string _TEXTURE_SMOOTHNESS_NAME = "SmoothnessMap.png";
        private const string _TEXTURE_REFLECTION_NAME = "ReflectionMap.png";
        private const string _TEXTURE_EMISSION_NAME = "EmissionMap.png";
        private const string _TEXTURE_OPACITY_NAME = "OpacityMap.png";
        private const string _TEXTURE_PREFIX = "T_";

        private static readonly int _COLOR_MAP = Shader.PropertyToID("_ColorMap");
        private static readonly int _METALLIC_MAP = Shader.PropertyToID("_MetallicMap");
        private static readonly int _SMOOTHNESS_MAP = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int _REFLECTION_MAP = Shader.PropertyToID("_ReflectionMap");
        private static readonly int _EMISSION_MAP = Shader.PropertyToID("_EmissionMap");
        private static readonly int _OPACITY_MAP = Shader.PropertyToID("_OpacityMap");
        private static bool _openWindowOnStart = true;
        private readonly List<SidekickColorRow> _visibleColorRows = new List<SidekickColorRow>();

        private List<SidekickColorRow> _allColorRows = new List<SidekickColorRow>();
        private List<SidekickSpecies> _allSpecies;
        private bool _applyingPreset = false;
        private List<string> _availablePartList;
        private ObjectField _baseModelField;
        private Dictionary<string, Vector3> _blendShapeRigMovement = new Dictionary<string, Vector3>();
        private ToolbarToggle _bodyPartsTab;
        private ToolbarToggle _bodyPresetTab;
        private ToolbarToggle _bodyShapeTab;
        private ScrollView _bodyShapeView;
        private float _bodySizeHeavyBlendValue;
        private float _bodySizeSkinnyBlendValue;
        private Slider _bodySizeSlider;
        private Slider _bodyTypeSlider;
        private VisualElement _colorSelectionRowView;
        private ToolbarToggle _colorSelectionTab;
        private ScrollView _colorSelectionView;
        private DropdownField _colorSetsDropdown;
        // Currently turned off due to early access
        // private bool _combineMeshes = false;
        private Dictionary<string, SidekickBodyShapePreset> _currentBodyPresetDictionary = new Dictionary<string, SidekickBodyShapePreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorSpeciesPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorOutfitsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorAttachmentsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorMaterialsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorElementsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private SidekickColorSet _currentColorSet;
        private bool _currentGlobalLockStatus;
        private Material _currentMaterial;
        private Dictionary<CharacterPartType, string> _currentPartSelections;
        private ColorPartType _currentPartType;
        private Dictionary<string, SidekickPartPreset> _currentHeadPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private Dictionary<string, SidekickPartPreset> _currentUpperBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private Dictionary<string, SidekickPartPreset> _currentLowerBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private SidekickSpecies _currentSpecies;
        private TabView _currentTab;
        private Dictionary<ColorPartType, List<Vector2>> _currentUVDictionary = new Dictionary<ColorPartType, List<Vector2>>();
        private List<Vector2> _currentUVList = new List<Vector2>();
        private DatabaseUpdateController _dbController;
        private DatabaseManager _dbManager;
        private string _dbPath;
        private ToolbarToggle _decalSelectionTab;
        private ScrollView _decalSelectionView;
        private StyleSheet _editorStyle;
        private bool _showAllColourProperties = false;
        private float _bodyTypeBlendValue;
        private ObjectField _materialField;
        private float _musclesBlendValue;
        private Slider _musclesSlider;
        private GameObject _newModel;
        private VisualElement _newSetNameContainer;
        private ScrollView _optionSelectionView;
        private ToolbarToggle _optionTab;
        private Label _partCountLabel;
        private Dictionary<CharacterPartType, SkinnedMeshRenderer> _partDictionary;
        private Dictionary<CharacterPartType, Dictionary<string, string>> _partLibrary;
        private Dictionary<string, List<string>> _partOutfitMap;
        private Dictionary<CharacterPartType, PopupField<string>> _partSelectionDictionary;
        private Dictionary<SidekickSpecies, List<string>> _partSpeciesMap;
        private Dictionary<string, bool> _partOutfitToggleMap;
        private ScrollView _partView;
        private Dictionary<string, string> _presetDefaultValues = new Dictionary<string, string>();
        private ScrollView _presetView;
        private Toggle _previewToggle;
        private bool _processingSpeciesChange = false;
        private DropdownField _speciesField;
        private DropdownField _speciesPresetField;
        private Label _statusLabel;
        private List<SidekickColorSet> _visibleColorSets = new List<SidekickColorSet>();

        /// <inheritdoc cref="Awake" />
        private void Awake()
        {
            InitializeEditorWindow();
        }

        /// <inheritdoc cref="OnDestroy" />
        private void OnDestroy()
        {
            // ensures we release the file lock on the database
            _dbManager.CloseConnection();
        }

#if UNITY_EDITOR
        /// <inheritdoc cref="OnEnable" />
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += StateChange;
        }

        /// <summary>
        ///     Processes the callback from the play mode state change.
        /// </summary>
        /// <param name="stateChange">The current PlayModeStateChange</param>
        private void StateChange(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.EnteredEditMode)
            {
                AddBodyShapeTabContent(_bodyShapeView);
            }
        }
#endif

        /// <inheritdoc cref="CreateGUI" />
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            if (_editorStyle != null)
            {
                root.styleSheets.Add(_editorStyle);
            }

            _statusLabel = new Label("Initializing...")
            {
                style =
                {
                    fontSize = 20,
                    marginTop = 15,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            root.Add(_statusLabel);

            InitializeDatabase();
            // if we still can't connect, something's gone wrong, don't keep building the GUI
            if (_dbManager?.GetCurrentDbConnection() == null)
            {
                return;
            }

            // TODO : here's the part where https://redmine.tools.syntystudios.com/issues/1237 can have issues
            // Debug.Log(_dbManager.GetCurrentDbConnection().State);

            // Maintains a linking to the model if the editor window is closed and re-opened.
            _newModel = GameObject.Find(_OUTPUT_MODEL_NAME);

            _allSpecies = SidekickSpecies.GetAll(_dbManager);
            _currentSpecies = _allSpecies[0];

            _currentPartSelections = new Dictionary<CharacterPartType, string>();
            foreach (CharacterPartType type in Enum.GetValues(typeof(CharacterPartType)))
            {
                _currentPartSelections.Add(type, "None");
            }

            _partCountLabel = new Label("")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            Image bannerImage = new Image
            {
                image = (Texture2D) Resources.Load("UI/T_SidekickTitle"),
                scaleMode = ScaleMode.ScaleToFit,
            };
            VisualElement bannerLayout = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(209f/256, 34f/256, 51f/256),
                    minHeight = 150,
                    paddingBottom = 5,
                    paddingTop = 5,
                }
            };
            bannerLayout.Add(bannerImage);
            root.Add(bannerLayout);

            _presetView = new ScrollView(ScrollViewMode.Vertical);
            _partView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            _bodyShapeView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            _colorSelectionView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionView = new ScrollView(ScrollViewMode.Vertical)
            // {
            //     style =
            //     {
            //         display = DisplayStyle.None
            //     }
            // };

            _optionSelectionView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            // TODO: Replace this tabbed menu code with TabView when 2023 LTS in the minimum supported version.
            Toolbar tabBar = new Toolbar
            {
                style =
                {
                    width = Length.Percent(100)
                }
            };

            _bodyPresetTab = new ToolbarToggle
            {
                text = "Presets",
                tooltip = "Create a character using preset combinations of parts, body types and colors"
            };

            _bodyPartsTab = new ToolbarToggle
            {
                text = "Parts",
                tooltip = "Edit individual character parts on your character"
            };

            _bodyShapeTab = new ToolbarToggle
            {
                text = "Body",
                tooltip = "Edit the body type, size, musculature of your character"
            };

            _colorSelectionTab = new ToolbarToggle
            {
                text = "Colors",
                tooltip = "Edit individual colors of your character"
            };

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionTab = new ToolbarToggle
            // {
            //     text = "Decals",
            //     tooltip = "Edit the decals applied to your character"
            // };

            _optionTab = new ToolbarToggle
            {
                text = "Options",
                tooltip = "Change the options of the tool"
            };

            tabBar.Add(_bodyPresetTab);
            tabBar.Add(_bodyPartsTab);
            tabBar.Add(_bodyShapeTab);
            tabBar.Add(_colorSelectionTab);
            // TODO: Uncomment when Decals added to the system.
            // tabBar.Add(_decalSelectionTab);
            tabBar.Add(_optionTab);
            root.Add(tabBar);

            _bodyPresetTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Preset && _bodyPresetTab.value)
                    {
                        SwitchToTab(TabView.Preset);
                    }
                }
            );

            _bodyPartsTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Parts && _bodyPartsTab.value)
                    {
                        SwitchToTab(TabView.Parts);
                    }
                }
            );

            _bodyShapeTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Body && _bodyShapeTab.value)
                    {
                        AddBodyShapeTabContent(_bodyShapeView);
                        SwitchToTab(TabView.Body);
                    }
                }
            );

            _colorSelectionTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Colors && _colorSelectionTab.value)
                    {
                        // always re-populate the color rows with the latest when switching tabs
                        if (_allColorRows.Count == 0)
                        {
                            PopulateColorRowsFromTextures();
                        }
                        else
                        {
                            PopulatePartColorRows();
                            RefreshVisibleColorRows();
                        }

                        SwitchToTab(TabView.Colors);
                    }
                }
            );

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionTab.RegisterValueChangedCallback(
            //     delegate
            //     {
            //         if (_currentTab != TabView.Decals && _decalSelectionTab.value)
            //         {
            //             SwitchToTab(TabView.Decals);
            //         }
            //     }
            // );

            _optionTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Options && _optionTab.value)
                    {
                        SwitchToTab(TabView.Options);
                    }
                    // If currently on this tab, and button is toggled to "off", toggle back to "on"
                    else if (_currentTab == TabView.Options && !_optionTab.value)
                    {
                        _optionTab.value = true;
                    }
                }
            );

            root.Add(_presetView);
            root.Add(_partView);
            root.Add(_bodyShapeView);
            root.Add(_colorSelectionView);
            // TODO: Uncomment when Decals added to the system.
            // root.Add(_decalSelectionView);
            root.Add(_optionSelectionView);

            // set the default colour set (this will change on various input triggers, but we need it to not be null)
            _currentColorSet = SidekickColorSet.GetDefault(_dbManager);

            AddBodyShapeTabContent(_bodyShapeView);
            AddColorTabContent(_colorSelectionView);
            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionView.Add(new Label("Decal selections will go here."));
            AddOptionsTabContent(_optionSelectionView);

            _partDictionary = new Dictionary<CharacterPartType, SkinnedMeshRenderer>();
            PopulatePartLibrary();
            PopulatePresetUI();

            _bodyPresetTab.value = true;
            SwitchToTab(TabView.Preset);

            VisualElement saveLoadButtons = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    alignContent = new StyleEnum<Align>(Align.FlexStart),
                    alignItems = new StyleEnum<Align>(Align.FlexStart),
                    flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
                    justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween),
                    minHeight = 30,
                    marginTop = 20
                }
            };

            root.Add(saveLoadButtons);

            Button loadCharacterButton = new Button(LoadCharacter)
            {
                text = "Load Character",
                style =
                {
                    minHeight = 30,
                    width = Length.Percent(48)
                }
            };
            saveLoadButtons.Add(loadCharacterButton);

            Button saveCharacterButton = new Button(SaveCharacter)
            {
                text = "Save Character",
                style =
                {
                    minHeight = 30,
                    width = Length.Percent(48)
                }
            };

            saveLoadButtons.Add(saveCharacterButton);


            Button createCharacterButton = new Button(CreateCharacterPrefab)
            {
                text = "Export Character as Prefab",
                style =
                {
                    minHeight = 50,
                    marginTop = 5
                }
            };
            root.Add(createCharacterButton);
        }

        /// <summary>
        ///     Initializes all the database setup, and provides the status label reference for users
        /// </summary>
        private void InitializeDatabase()
        {
            // TODO: always reinstantiate instead?
            if (_dbController != null)
            {
                return;
            }

            _dbController = new DatabaseUpdateController(_statusLabel);
            _dbManager = _dbController.DatabaseManager;
        }

        /// <summary>
        ///     Initializes the editor window.
        /// </summary>
        private void InitializeEditorWindow()
        {
            _editorStyle = Resources.Load<StyleSheet>("Styles/EditorStyles");
            _openWindowOnStart = EditorPrefs.GetBool(_AUTO_OPEN_STATE, true);
        }

        /// <summary>
        ///     Adds the contents and change listeners for the body shape tab.
        /// </summary>
        /// <param name="view">The tabview to add the content to.</param>
        private void AddBodyShapeTabContent(ScrollView view)
        {
            view.Clear();
            if (Application.isEditor && Application.isPlaying)
            {
                Label warningNotice = new Label("Blend shapes not able to be changed at runtime.");
                _bodyShapeView.Add(warningNotice);
                return;
            }

            _bodyTypeSlider = new Slider("Body Type", -100, 100)
            {
                value = _bodyTypeBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto)
                },
                showInputField = true,
                tooltip = "Blend the body type of the character between masculine and feminine"
            };

            VisualElement bodyTypeLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    paddingLeft = 155
                }
            };

            Label labelMasculine = new Label("Masculine")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            bodyTypeLabels.Add(labelMasculine);

            Label labelFeminine = new Label("Feminine")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            bodyTypeLabels.Add(labelFeminine);

            _bodySizeSlider = new Slider("Body Size", -100, 100)
            {
                value = _bodySizeSkinnyBlendValue > 0 ? -_bodySizeSkinnyBlendValue : _bodySizeHeavyBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto),
                    marginTop = 30
                },
                showInputField = true,
                tooltip = "Blend the body size of the character between slim and heavy"
            };

            VisualElement bodySizeLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    paddingLeft = 155
                }
            };

            Label labelSlim = new Label("Slim")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            bodySizeLabels.Add(labelSlim);

            Label labelHeavy = new Label("Heavy")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            bodySizeLabels.Add(labelHeavy);

            _musclesSlider = new Slider("Musculature", -100, 100)
            {
                value = _musclesBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto),
                    marginTop = 30
                },
                showInputField = true,
                tooltip = "Blend the musculature of the character between lean and muscular"
            };

            VisualElement muscleLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    height = 20,
                    paddingLeft = 155
                }
            };

            Label labelLean = new Label("Lean")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            muscleLabels.Add(labelLean);

            Label labelBulk = new Label("Muscular")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            muscleLabels.Add(labelBulk);

            view.Add(_bodyTypeSlider);
            view.Add(bodyTypeLabels);
            view.Add(_bodySizeSlider);
            view.Add(bodySizeLabels);
            view.Add(_musclesSlider);
            view.Add(muscleLabels);

            _bodyTypeSlider.RegisterValueChangedCallback(
                evt =>
                {
                    _bodyTypeBlendValue = evt.newValue;

                    if (_newModel == null) _newModel = CreateCharacter(false, true);

                    UpdateBlendShapes(_newModel);
                    ProcessRigMovementOnBlendShapeChange();
                    ProcessBoneMovement(_newModel);
                }
            );

            _bodySizeSlider.RegisterValueChangedCallback(
                evt =>
                {
                    float newValue = evt.newValue;
                    if (newValue > 0)
                    {
                        _bodySizeHeavyBlendValue = newValue;
                        _bodySizeSkinnyBlendValue = 0;
                    }
                    else if (newValue < 0)
                    {
                        _bodySizeHeavyBlendValue = 0;
                        _bodySizeSkinnyBlendValue = -newValue;
                    }
                    else
                    {
                        _bodySizeHeavyBlendValue = 0;
                        _bodySizeSkinnyBlendValue = 0;
                    }

                    if (_newModel == null)
                    {
                        _newModel = CreateCharacter(false, true);
                    }

                    UpdateBlendShapes(_newModel);
                    ProcessRigMovementOnBlendShapeChange();
                    ProcessBoneMovement(_newModel);
                }
            );

            _musclesSlider.RegisterValueChangedCallback(
                evt =>
                {
                    _musclesBlendValue = evt.newValue;

                    if (_newModel == null)
                    {
                        _newModel = CreateCharacter(false, true);
                    }

                    UpdateBlendShapes(_newModel);
                    ProcessRigMovementOnBlendShapeChange();
                    ProcessBoneMovement(_newModel);
                }
            );
        }

        /// <summary>
        ///     Processes the movement of rig joints based on blend shape changes.
        /// </summary>
        private void ProcessRigMovementOnBlendShapeChange()
        {
            GameObject donorModel = (GameObject) _baseModelField.value;
            Transform modelRootBone = donorModel.transform.Find("root");
            Hashtable boneNameMap = Combiner.CreateBoneNameMap(modelRootBone.gameObject);

            foreach (KeyValuePair<CharacterPartType, string> entry in BlendshapeJointAdjustment.PART_TYPE_JOINT_MAP)
            {
                Transform bone = (Transform) boneNameMap[entry.Value];
                Vector3 bonePosition = bone.position;
                float shapeBlendValue = 0f;
                if (_bodySizeHeavyBlendValue > 0)
                {
                    shapeBlendValue = _bodySizeHeavyBlendValue / 100;
                }
                else
                {
                    shapeBlendValue = -(_bodySizeSkinnyBlendValue / 100);
                }

                float feminineBlendValue = _bodyTypeBlendValue > 0 ? _bodyTypeBlendValue / 100 : 0;

                Vector3 allMovement = BlendshapeJointAdjustment.GetCombinedOffsetValue(
                    feminineBlendValue,
                    shapeBlendValue,
                    (_musclesBlendValue + 100) / 2 / 100,
                    bonePosition,
                    entry.Key
                );

                _blendShapeRigMovement[entry.Value] = allMovement;
            }
        }

        private void ProcessBoneMovement(GameObject model)
        {
            Transform modelRootBone = model.transform.Find("root");
            Hashtable boneNameMap = Combiner.CreateBoneNameMap(modelRootBone.gameObject);
            Combiner.ProcessBoneMovement(boneNameMap, _blendShapeRigMovement);
        }

        /// <summary>
        ///     Adds the content to the Color tab.
        /// </summary>
        /// <param name="view">The view to add the content to.</param>
        private void AddColorTabContent(ScrollView view)
        {
            _colorSetsDropdown = new DropdownField
            {
                style =
                {
                    maxWidth = Length.Percent(65)
                }
            };

            Label filterPartsLabel = new Label("Filter - Parts")
            {
                tooltip = "Filter the displayed colors to focus on specific areas of the character and reduce the number of properties"
            };
            view.Add(filterPartsLabel);
            DropdownField partTypeDropdown = new DropdownField();
            string[] colorPartTypes = Enum.GetNames(typeof(ColorPartType));
            // Enum names can't have spaces so we add in the space manually for display.
            for (int i = 0; i < colorPartTypes.Length; i++)
            {
                colorPartTypes[i] = StringUtils.AddSpacesBeforeCapitalLetters(colorPartTypes[i]);
            }

            partTypeDropdown.choices = colorPartTypes.ToList();
            partTypeDropdown.value = colorPartTypes[0];
            view.Add(partTypeDropdown);

            partTypeDropdown.RegisterValueChangedCallback(
                (evt) =>
                {
                    if (Enum.TryParse(typeof(ColorPartType), evt.newValue.Replace(" ", ""), out object newType))
                    {
                        _currentPartType = (ColorPartType) newType;
                        _colorSetsDropdown.value = "Custom";
                    }

                    PopulatePartColorRows();
                    RefreshVisibleColorRows();
                }
            );

            _currentPartType = ColorPartType.AllParts;

            // TODO: Hidden due to early access, enable when feature complete
            // Label colorSetsLabel = new Label("Color Sets");
            // view.Add(colorSetsLabel);
            //
            // VisualElement colorSetsRow = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
            //         width = Length.Percent(100),
            //         alignContent = new StyleEnum<Align>(Align.FlexStart),
            //         alignItems = new StyleEnum<Align>(Align.FlexStart),
            //         flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
            //         justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween),
            //         marginBottom = 10
            //     }
            // };

            UpdateVisibleColorSets();

            // TODO: Hidden due to early access, enable when feature complete
            // colorSetsRow.Add(_colorSetsDropdown);
            //
            // _colorSetsDropdown.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         if (evt.newValue != "Custom")
            //         {
            //             SidekickColorSet newSet = _visibleColorSets.First(set => set.Name == evt.newValue);
            //             List<SidekickColorRow> newRows = SidekickColorRow.GetAllBySet(_dbManager, newSet);
            //             if (_currentPartType == ColorPartType.AllParts && _allColorRows.All(row => row.IsLocked == false))
            //             {
            //                 _currentColorSet = newSet;
            //                 _allColorRows = newRows;
            //                 if (_allColorRows.Count == 0)
            //                 {
            //                     PopulateColorRowsFromTextures();
            //                 }
            //                 else
            //                 {
            //                     PopulatePartColorRows();
            //                     RefreshVisibleColorRows();
            //                 }
            //             }
            //             else
            //             {
            //                 foreach (SidekickColorRow row in _visibleColorRows)
            //                 {
            //                     SidekickColorRow newRow = newRows.FirstOrDefault(r => r.ColorProperty.ID == row.ColorProperty.ID);
            //                     if (newRow != null)
            //                     {
            //                         row.NiceColor = newRow.NiceColor;
            //                         row.NiceMetallic = newRow.NiceMetallic;
            //                         row.NiceSmoothness = newRow.NiceSmoothness;
            //                         row.NiceReflection = newRow.NiceReflection;
            //                         row.NiceEmission = newRow.NiceEmission;
            //                         row.NiceOpacity = newRow.NiceOpacity;
            //                     }
            //                 }
            //
            //                 RefreshVisibleColorRows();
            //             }
            //
            //             UpdateAllVisibleColors();
            //         }
            //     }
            // );
            //
            // Button previousSetButton = new Button(
            //     () =>
            //     {
            //         if (_colorSetsDropdown.index > 0)
            //         {
            //             _colorSetsDropdown.index -= 1;
            //         }
            //     }
            // )
            // {
            //     tooltip = "Previous Color Set"
            // };
            //
            // previousSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("tab_prev", "|Previous Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(previousSetButton);
            // Button nextSetButton = new Button(
            //     () =>
            //     {
            //         if (_colorSetsDropdown.index < _colorSetsDropdown.choices.Count - 1)
            //         {
            //             _colorSetsDropdown.index += 1;
            //         }
            //     }
            // )
            // {
            //     tooltip = "Next Color Set"
            // };
            //
            // nextSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("tab_next", "|Next Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(nextSetButton);
            // Button resetSetButton = new Button(ResetColorSet)
            // {
            //     tooltip = "Reset Color Set from Disk"
            // };
            // resetSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("Refresh", "|Reset Color Set From Disk").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(resetSetButton);
            // Button newSetButton = new Button(ShowCreateNewColorSet)
            // {
            //     tooltip = "Create New Color Set"
            // };
            // newSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("Toolbar Plus", "|Create New Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(newSetButton);
            // Button deleteSetButton = new Button(DeleteColorSet)
            // {
            //     tooltip = "Delete Color Set"
            // };
            // deleteSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("close", "|Delete Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(deleteSetButton);
            // Button saveSetButton = new Button(SaveColorSet)
            // {
            //     tooltip = "Save Color Set"
            // };
            // saveSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("SaveAs", "|Save Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(saveSetButton);
            // view.Add(colorSetsRow);
            //
            // _newSetNameContainer = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
            //         width = Length.Percent(100),
            //         alignContent = new StyleEnum<Align>(Align.FlexStart),
            //         alignItems = new StyleEnum<Align>(Align.FlexStart),
            //         flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
            //         marginBottom = 10,
            //         display = DisplayStyle.None
            //     }
            // };
            //
            // TextField newNameField = new TextField("New Set Name")
            // {
            //     style =
            //     {
            //         minWidth = Length.Percent(70)
            //     }
            // };
            // _newSetNameContainer.Add(newNameField);
            //
            // Button newSetCreateButton = new Button(
            //     () =>
            //     {
            //         CreateNewColorSet(newNameField.value);
            //         List<string> choices = _colorSetsDropdown.choices;
            //         choices.Add(newNameField.value);
            //         _colorSetsDropdown.choices = choices;
            //         _colorSetsDropdown.value = newNameField.value;
            //         _newSetNameContainer.style.display = DisplayStyle.None;
            //     }
            // )
            // {
            //     text = "Create Set"
            // };
            //
            // newNameField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         newSetCreateButton.SetEnabled(!SidekickColorSet.DoesNameExist(_dbManager, evt.newValue));
            //     }
            // );
            //
            // _newSetNameContainer.Add(newSetCreateButton);
            // view.Add(_newSetNameContainer);
            //
            // VisualElement allRow = new VisualElement();
            // allRow.AddToClassList("colorSelectionRow");
            //
            // Label allItemsLabel = new Label("All");
            // allItemsLabel.AddToClassList("colorSelectionRowLabel");
            // allRow.Add(allItemsLabel);
            //
            // VisualElement allRowContent = new VisualElement();
            // allRowContent.AddToClassList("colorSelectionRowContent");
            // allRow.Add(allRowContent);
            //
            // Button btnAllLock = new Button
            // {
            //     style =
            //     {
            //         left = 0
            //     }
            // };
            // allRowContent.Add(btnAllLock);
            // Image lockButtonImage = new Image
            // {
            //     image = _currentGlobalLockStatus
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image,
            //     scaleMode = ScaleMode.ScaleToFit
            // };
            //
            // btnAllLock.Add(lockButtonImage);
            //
            // btnAllLock.clickable.clicked += () =>
            // {
            //     foreach (SidekickColorRow colorRow in _visibleColorRows)
            //     {
            //         colorRow.IsLocked = !_currentGlobalLockStatus;
            //         if (colorRow.ButtonImage != null)
            //         {
            //             colorRow.ButtonImage.image =
            //                 colorRow.IsLocked
            //                     ? EditorGUIUtility.IconContent("Locked").image
            //                     : EditorGUIUtility.IconContent("Unlocked").image;
            //         }
            //     }
            //
            //     _currentGlobalLockStatus = !_currentGlobalLockStatus;
            //     lockButtonImage.image = _currentGlobalLockStatus
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image;
            // };
            //
            // TODO: Uncomment once all colors options are re-enabled
            // Button randomAllButton = new Button
            // {
            //     text = "R",
            //     style =
            //     {
            //         right = 0
            //     }
            // };
            // allRowContent.Add(randomAllButton);
            //
            // view.Add(allRow);

            _colorSelectionRowView = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100)
                }
            };

            view.Add(_colorSelectionRowView);

            UpdateColorTabContent();
        }

        /// <summary>
        ///     Adds the content to the options tab.
        /// </summary>
        /// <param name="view">The view to add the content to.</param>
        private void AddOptionsTabContent(VisualElement view)
        {
            Label baseAssetLabel = new Label
            {
                style =
                {
                    marginTop = 5,
                    marginLeft = 12,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                text = "Base Assets",
                tooltip = "These assets are used to construct the character"
            };

            view.Add(baseAssetLabel);

            _baseModelField = new ObjectField
            {
                style =
                {
                    marginLeft = 15,
                    marginRight = 15
                },
                tooltip = "The rigged character model used when constructing a character",
                objectType = typeof(GameObject),
                label = "Model"
            };

            _baseModelField.RegisterCallback<ChangeEvent<Object>>(
                changeEvent =>
                {
                    // TODO: Check the model has a minimum of 1 SkinnedMeshRenderer as a child.
                }
            );
            view.Add(_baseModelField);

            _baseModelField.value = Resources.Load<GameObject>(_BASE_MESH_NAME);

            _materialField = new ObjectField
            {
                tooltip = "The material used when constructing a character",
                objectType = typeof(Material),
                label = "Material",
                style =
                {
                    marginLeft = 15,
                    marginRight = 15
                }
            };

            view.Add(_materialField);

            _materialField.value = Resources.Load<Material>(_BASE_MATERIAL_NAME);

            VisualElement updateLibraryLayout = new VisualElement
            {
                style =
                {
                    minHeight = 20,
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    marginTop = 10,
                    marginLeft = 15,
                    marginRight = 2
                }
            };

            Button uploadLibraryButton = new Button(PopulatePartLibrary)
            {
                text = "Update Part Library",
                tooltip = "Re-scans the project folders to update the parts list"
            };

            updateLibraryLayout.Add(uploadLibraryButton);
            updateLibraryLayout.Add(_partCountLabel);
            view.Add(updateLibraryLayout);

            // TODO: Hidden due to early access, enable when feature complete
            // Toggle combineToggle = new Toggle("Combine Character Meshes")
            // {
            //     value = _combineMeshes,
            //     style =
            //     {
            //         marginTop = 10,
            //         marginLeft = 15
            //     }
            // };
            //
            // combineToggle.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         _combineMeshes = evt.newValue;
            //     }
            // );
            //
            // view.Add(combineToggle);

            _previewToggle = new Toggle("Auto Build Model")
            {
                value = true,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                }
            };

            //view.Add(_previewToggle);

            Toggle filterColorsToggle = new Toggle("Show all color properties")
            {
                value = _showAllColourProperties,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Display all color properties in the color tab rather than limited to only what the current character is using"
            };

            filterColorsToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _showAllColourProperties = evt.newValue;
                }
            );

            view.Add(filterColorsToggle);

            Toggle autoOpenToggle = new Toggle("Open tool on startup")
            {
                value = _openWindowOnStart,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Opens the Sidekick character tool on Unity startup"
            };

            autoOpenToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _openWindowOnStart = evt.newValue;
                    EditorPrefs.SetBool(_AUTO_OPEN_STATE, _openWindowOnStart);
                }
            );

            view.Add(autoOpenToggle);

            Label dbVersion = new Label($"Current database: v{_dbManager.GetDatabaseVersion()}")
            {
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "The current iteration of the database, the database holds information about parts, presets and colors"
            };

            view.Add(dbVersion);

            VisualElement row = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    justifyContent = new StyleEnum<Justify>(Justify.SpaceAround),
                    marginTop = 30
                }
            };

            Button documentationButton = new Button
            {
                text = "Documentation",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "Open documentation for Sidekick characters"
            };

            documentationButton.clickable.clicked += delegate
            {
                Application.OpenURL("file:Assets/Synty/SidekickCharacters/Documentation/SidekickCharacters_UserGuide.pdf");
            };

            Button storeButton = new Button
            {
                text = "Synty Store",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "www.syntystore.com"
            };

            storeButton.clickable.clicked += delegate
            {
                Application.OpenURL("https://syntystore.com");
            };

            Button tutorialButton = new Button
            {
                text = "Tutorials",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "Sidekick Characters - Quick start guide"
            };

            tutorialButton.clickable.clicked += delegate
            {
                // TODO: Change to direct link to tutorial playlist, when available
                Application.OpenURL("https://www.youtube.com/@syntystudios");
            };

            row.Add(documentationButton);
            row.Add(storeButton);
            row.Add(tutorialButton);
            view.Add(row);
        }

        /// <summary>
        ///     Reset the color rows for this color set back to the colors stored on the saved textures.
        /// </summary>
        private void ResetColorSet()
        {
            PopulateColorRowsFromTextures();
        }

        /// <summary>
        ///     Shows the name field and creation button for creating a new color set.
        /// </summary>
        private void ShowCreateNewColorSet()
        {
            _newSetNameContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     Creates a new color set with the given name.
        /// </summary>
        /// <param name="setName">The name for the color set.</param>
        private void CreateNewColorSet(string setName)
        {
            ResetCurrentColorSet(setName);
            SaveColorSet();
        }

        /// <summary>
        ///     Sets the current color set to a new, in-memory set of rows independent of the database
        /// </summary>
        /// <param name="setName">Name of the new color set</param>
        private void ResetCurrentColorSet(string setName = "Custom")
        {
            _currentColorSet = new SidekickColorSet
            {
                ID = -1,
                Species = _currentSpecies,
                Name = setName
            };

            foreach (SidekickColorRow row in _allColorRows)
            {
                row.ID = -1;
                row.ColorSet = _currentColorSet;
            }
        }

        /// <summary>
        ///     Deletes a color set from the database. The color set will still be available in the app until the app is closed.
        /// </summary>
        private void DeleteColorSet()
        {
            List<SidekickColorRow> rowsToDelete = SidekickColorRow.GetAllBySet(_dbManager, _currentColorSet);
            foreach (SidekickColorRow row in rowsToDelete)
            {
                row.Delete(_dbManager);
            }

            _currentColorSet.Delete(_dbManager);
            UpdateVisibleColorSets();
            ResetCurrentColorSet();

        }

        /// <summary>
        ///     Saves the current color row to the database. If it is a new color row, it is inserted into the DB; otherwise it is updated in the DB.
        /// </summary>
        private void SaveColorSet()
        {
            string path = Path.Combine(_BASE_COLOR_SET_PATH, _currentSpecies.Name);
            path = Path.Combine(path, _currentColorSet.Name.Replace(" ", "_"));

            SaveTexturesToDisk(path);

            _currentColorSet.Save(_dbManager);
            foreach (SidekickColorRow row in _allColorRows)
            {
                row.Save(_dbManager);
            }

            UpdateVisibleColorSets(false);

            // TODO : refresh the project inspector window so the new textures show up
        }

        /// <summary>
        ///     Saves texture files to disk at the given path.
        /// </summary>
        /// <param name="path">The path to save the textures to.</param>
        private void SaveTexturesToDisk(string path, string additionalNaming = "")
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // if no parts are selected, don't try and save a non-existent material, instead create one
            if (_currentMaterial == null)
            {
                _currentMaterial = (Material) _materialField.value;
            }

            string filename = _TEXTURE_PREFIX;

            if (!string.IsNullOrEmpty(additionalNaming))
            {
                filename += additionalNaming;
            }

            string filePath = Path.Combine(path, filename + _TEXTURE_COLOR_NAME);
            Texture2D texture = (Texture2D) _currentMaterial.GetTexture(_COLOR_MAP);
            File.WriteAllBytes(filePath, texture.EncodeToPNG());
            _currentColorSet.SourceColorPath = filePath;
            // TODO: Hidden due to early access, enable when feature complete
            // filePath = Path.Combine(path, filename + _TEXTURE_METALLIC_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_METALLIC_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceMetallicPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_SMOOTHNESS_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_SMOOTHNESS_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceSmoothnessPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_REFLECTION_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_REFLECTION_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceReflectionPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_EMISSION_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_EMISSION_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceEmissionPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_OPACITY_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_OPACITY_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceOpacityPath = filePath;
        }

        /// <summary>
        ///     Updates the color sets that are selectable in the dropdown on the colors tab
        /// </summary>
        /// <param name="setDropdownToCustom">Whether to set the color sets dropdown value to 'Custom'</param>
        private void UpdateVisibleColorSets(bool setDropdownToCustom = true)
        {
            List<SidekickColorSet> sets = SidekickColorSet.GetAllBySpecies(_dbManager, _currentSpecies);
            if (sets.Count == 0)
            {
                sets.Add(SidekickColorSet.GetDefault(_dbManager));
            }

            List<string> setNames = sets.Select(set => set.Name).ToList();
            _colorSetsDropdown.choices = setNames;
            _visibleColorSets = sets;

            if (setDropdownToCustom)
            {
                _colorSetsDropdown.value = "Custom";
            }
        }

        /// <summary>
        ///     Updates all the color rows currently visible in the UI.
        /// </summary>
        private void UpdateAllVisibleColors()
        {
            foreach (SidekickColorRow row in _visibleColorRows)
            {
                UpdateAllColors(row);
            }
        }

        /// <summary>
        ///     Updates all the color types for a given color row.
        /// </summary>
        /// <param name="colorRow">The color row to update.</param>
        private void UpdateAllColors(SidekickColorRow colorRow)
        {
            foreach (ColorType colorType in Enum.GetValues(typeof(ColorType)))
            {
                UpdateColor(colorType, colorRow);
            }
        }

        /// <summary>
        ///     Updates the texture on the given color row for the specified color type.
        /// </summary>
        /// <param name="colorType">The color type to update.</param>
        /// <param name="colorRow">The color row to get the updated color from.</param>
        private void UpdateColor(ColorType colorType, SidekickColorRow colorRow)
        {
            if (colorRow == null)
            {
                return;
            }

            if (_materialField.value == null)
            {
                return;
            }

            // if no parts are selected, don't try and update a non-existent material, instead create one to sync up later
            if (_currentMaterial == null)
            {
                _currentMaterial = (Material) _materialField.value;
            }

            switch (colorType)
            {
                case ColorType.Metallic:
                    Texture2D metallic = (Texture2D) _currentMaterial.GetTexture(_METALLIC_MAP);
                    UpdateTexture(metallic, colorRow.NiceMetallic, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_METALLIC_MAP, metallic);
                    break;
                case ColorType.Smoothness:
                    Texture2D smoothness = (Texture2D) _currentMaterial.GetTexture(_SMOOTHNESS_MAP);
                    UpdateTexture(smoothness, colorRow.NiceSmoothness, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_SMOOTHNESS_MAP, smoothness);
                    break;
                case ColorType.Reflection:
                    Texture2D reflection = (Texture2D) _currentMaterial.GetTexture(_REFLECTION_MAP);
                    UpdateTexture(reflection, colorRow.NiceReflection, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_REFLECTION_MAP, reflection);
                    break;
                case ColorType.Emission:
                    Texture2D emission = (Texture2D) _currentMaterial.GetTexture(_EMISSION_MAP);
                    UpdateTexture(emission, colorRow.NiceEmission, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_EMISSION_MAP, emission);
                    break;
                case ColorType.Opacity:
                    Texture2D opacity = (Texture2D) _currentMaterial.GetTexture(_OPACITY_MAP);
                    UpdateTexture(opacity, colorRow.NiceOpacity, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_OPACITY_MAP, opacity);
                    break;
                case ColorType.MainColor:
                default:
                    Texture2D color = (Texture2D) _currentMaterial.GetTexture(_COLOR_MAP);
                    UpdateTexture(color, colorRow.NiceColor, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_COLOR_MAP, color);
                    break;
            }
        }

        /// <summary>
        ///     Updates the color on the texture with the given new color.
        /// </summary>
        /// <param name="texture">The texture to update.</param>
        /// <param name="newColor">The color to assign to the texture.</param>
        /// <param name="u">The u positioning on the texture to update.</param>
        /// <param name="v">The v positioning on the texture to update.</param>
        private void UpdateTexture(Texture2D texture, Color newColor, int u, int v)
        {
            int scaledU = u * 2;
            int scaledV = v * 2;
            texture.SetPixel(scaledU, scaledV, newColor);
            texture.SetPixel(scaledU + 1, scaledV, newColor);
            texture.SetPixel(scaledU, scaledV + 1, newColor);
            texture.SetPixel(scaledU + 1, scaledV + 1, newColor);
            texture.Apply();
        }

        /// <summary>
        ///     Populates the part color rows based on the filter being used.
        /// </summary>
        private void PopulatePartColorRows()
        {
            List<SidekickColorProperty> propertiesToShow = new List<SidekickColorProperty>();

            switch (_currentPartType)
            {
                case ColorPartType.Species:
                    List<SidekickColorProperty> speciesProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Species);
                    foreach (SidekickColorProperty property in speciesProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Outfit:
                    List<SidekickColorProperty> outfitProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Outfits);
                    foreach (SidekickColorProperty property in outfitProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Attachments:
                    List<SidekickColorProperty> attachmentProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Attachments);
                    foreach (SidekickColorProperty property in attachmentProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Materials:
                    List<SidekickColorProperty> materialProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Materials);
                    foreach (SidekickColorProperty property in materialProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                // case ColorPartType.Elements:
                //     List<SidekickColorProperty> elementProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Elements);
                //     foreach (SidekickColorProperty property in elementProperties)
                //     {
                //         Vector2 uv = new Vector2(property.U, property.V);
                //         if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                //         {
                //             propertiesToShow.Add(property);
                //         }
                //     }
                //
                //     break;
                case ColorPartType.CharacterHead:
                    List<SidekickColorProperty> headProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterHead.GetPartTypes())
                    {
                        headProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in headProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.CharacterUpperBody:
                    List<SidekickColorProperty> upperProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterUpperBody.GetPartTypes())
                    {
                        upperProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in upperProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.CharacterLowerBody:
                    List<SidekickColorProperty> lowerProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterLowerBody.GetPartTypes())
                    {
                        lowerProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in lowerProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Head:
                case ColorPartType.Hair:
                case ColorPartType.EyebrowLeft:
                case ColorPartType.EyebrowRight:
                case ColorPartType.EyeLeft:
                case ColorPartType.EyeRight:
                case ColorPartType.EarLeft:
                case ColorPartType.EarRight:
                case ColorPartType.FacialHair:
                case ColorPartType.Torso:
                case ColorPartType.ArmUpperLeft:
                case ColorPartType.ArmUpperRight:
                case ColorPartType.ArmLowerLeft:
                case ColorPartType.ArmLowerRight:
                case ColorPartType.HandLeft:
                case ColorPartType.HandRight:
                case ColorPartType.Hips:
                case ColorPartType.LegLeft:
                case ColorPartType.LegRight:
                case ColorPartType.FootLeft:
                case ColorPartType.FootRight:
                case ColorPartType.AttachmentHead:
                case ColorPartType.AttachmentFace:
                case ColorPartType.AttachmentBack:
                case ColorPartType.AttachmentHipsFront:
                case ColorPartType.AttachmentHipsBack:
                case ColorPartType.AttachmentHipsLeft:
                case ColorPartType.AttachmentHipsRight:
                case ColorPartType.AttachmentShoulderLeft:
                case ColorPartType.AttachmentShoulderRight:
                case ColorPartType.AttachmentElbowLeft:
                case ColorPartType.AttachmentElbowRight:
                case ColorPartType.AttachmentKneeLeft:
                case ColorPartType.AttachmentKneeRight:
                case ColorPartType.Nose:
                case ColorPartType.Teeth:
                case ColorPartType.Tongue:/*
                case ColorPartType.Wrap:
                case ColorPartType.AttachmentHandLeft:
                case ColorPartType.AttachmentHandRight: */
                    propertiesToShow = SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[_currentPartType]);
                    break;
                case ColorPartType.AllParts:
                default:
                    propertiesToShow = _showAllColourProperties
                        ? SidekickColorProperty.GetAll(_dbManager)
                        : SidekickColorProperty.GetByUVs(_dbManager, _currentUVList);
                    break;
            }

            _visibleColorRows.Clear();

            // when filtering the view to a specific UV dictionary, we need to reset the property order
            propertiesToShow.Sort((a, b) => a.ID.CompareTo(b.ID));

            foreach (SidekickColorProperty property in propertiesToShow)
            {
                foreach (SidekickColorRow row in _allColorRows.Where(row => row.ColorProperty.ID == property.ID))
                {
                    _visibleColorRows.Add(row);
                }
            }
        }

        /// <summary>
        ///     Refreshes the visible color rows in the UI.
        /// </summary>
        private void RefreshVisibleColorRows()
        {
            _colorSelectionRowView.Clear();

            foreach (ColorGroup group in Enum.GetValues(typeof(ColorGroup)))
            {
                List<SidekickColorProperty> properties = _visibleColorRows
                    .Select(row => row.ColorProperty)
                    .Where(prop => prop.Group == group)
                    .ToList();

                string tooltipText = "";

                switch (group)
                {
                    case ColorGroup.Species:
                        tooltipText = "Species colors make up the character as if it has no outfit on. (for example - skin, teeth, tongue, fingernails etc)";
                        break;
                    case ColorGroup.Outfits:
                        tooltipText = "Outfit colors make up the clothing on the character. (for example - Torso outfit, arm outfit, hand outfit etc)";
                        break;
                    case ColorGroup.Attachments:
                        tooltipText = "Attachment colors make up the additional parts attached to a character. (for example - a backpack, shoulder pads, elbow pads, knee pads etc)";
                        break;
                    case ColorGroup.Materials:
                        tooltipText = "material colors make up a collection of shared standard materials (for example - wood, metal, leather, paper, bone etc)";
                        break;
                }

                if (properties.Count > 0)
                {
                    Label groupLabel = new Label(group.ToString())
                    {
                        style =
                        {
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                            marginBottom = 4,
                            marginTop = 6
                        },
                        tooltip = tooltipText
                    };
                    _colorSelectionRowView.Add(groupLabel);
                    foreach (SidekickColorProperty property in properties)
                    {
                        foreach (SidekickColorRow row in _visibleColorRows.Where(row => row.ColorProperty.ID == property.ID))
                        {
                            CreateColorRow(_colorSelectionRowView, row);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Populates the color rows from texture files on the disk.
        /// </summary>
        private void PopulateColorRowsFromTextures()
        {
            TextureImporter textureImporter = null;

            Texture2D mainColor = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceColorPath);
            if (mainColor != null)
            {
                mainColor.filterMode = FilterMode.Point;
                if (!mainColor.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceColorPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceMetallicPath);
            if (metallic != null)
            {
                metallic.filterMode = FilterMode.Point;
                if (!metallic.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceMetallicPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D smoothness = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceSmoothnessPath);
            if (smoothness != null)
            {
                smoothness.filterMode = FilterMode.Point;
                if (!smoothness.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceSmoothnessPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D reflection = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceReflectionPath);
            if (reflection != null)
            {
                reflection.filterMode = FilterMode.Point;
                if (!reflection.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceReflectionPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D emission = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceEmissionPath);
            if (emission != null)
            {
                emission.filterMode = FilterMode.Point;
                if (!emission.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceEmissionPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D opacity = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceOpacityPath);
            if (opacity != null)
            {
                opacity.filterMode = FilterMode.Point;
                if (!opacity.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceOpacityPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            List<SidekickColorRow> newColorRows = new List<SidekickColorRow>();

            List<SidekickColorRow> currentSetColors = SidekickColorRow.GetAllBySet(_dbManager, _currentColorSet);

            // TODO : if textures don't exist BUT color rows exist in DB, ask user if they want to re-save the textures from the DB values, loop back and reimport
            // TODO : if textures don't exist AND color rows don't exist in DB, delete the colorset entry in DB/dropdown, advance to next on list and reload

            foreach (SidekickColorProperty property in SidekickColorProperty.GetAll(_dbManager))
            {
                SidekickColorRow existingRow = currentSetColors.FirstOrDefault(row => row.ColorProperty.ID == property.ID);
                SidekickColorRow newRow = new SidekickColorRow
                {
                    ID = existingRow?.ID ?? -1,
                    ColorSet = _currentColorSet,
                    ColorProperty = property,
                    // TODO remove null checks when we know we have textures
                    NiceColor = mainColor?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceColor ?? Color.red,
                    NiceMetallic = metallic?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceMetallic ?? Color.red,
                    NiceSmoothness = smoothness?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceSmoothness ?? Color.red,
                    NiceReflection = reflection?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceReflection ?? Color.red,
                    NiceEmission = emission?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceEmission ?? Color.red,
                    NiceOpacity = opacity?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceOpacity ?? Color.red
                };

                newRow.Save(_dbManager);
                newColorRows.Add(newRow);
            }

            _allColorRows = newColorRows;
            PopulatePartColorRows();
            RefreshVisibleColorRows();
        }

        /// <summary>
        ///     Adds a color row to the given view.
        /// </summary>
        /// <param name="view">The view to add the color row to.</param>
        /// <param name="colorRow">The color row to populate this UI element with.</param>
        private void CreateColorRow(VisualElement view, SidekickColorRow colorRow)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("colorSelectionRow");

            Label rowLabel = new Label(colorRow.ColorProperty.Name);
            rowLabel.AddToClassList("colorSelectionRowLabel");
            row.Add(rowLabel);

            VisualElement rowContent = new VisualElement();
            rowContent.AddToClassList("colorSelectionRowContent");

            row.Add(rowContent);

            // TODO: uncomment when locking is required.
            // Button btnLock = new Button();
            //
            // Image lockImage = new Image
            // {
            //     image = colorRow.IsLocked ? EditorGUIUtility.IconContent("Locked").image : EditorGUIUtility.IconContent("Unlocked").image,
            //     scaleMode = ScaleMode.ScaleToFit
            // };
            //
            // btnLock.Add(lockImage);
            // colorRow.ButtonImage = lockImage;
            // rowContent.Add(btnLock);
            // btnLock.clickable.clicked += () =>
            // {
            //     colorRow.IsLocked = !colorRow.IsLocked;
            //     lockImage.image = colorRow.IsLocked
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image;
            // };

            ColorField colorField = new ColorField
            {
                value = colorRow.NiceColor,
                tooltip = colorRow.ColorProperty.Name + " Color",
                style =
                {
                    // TODO: shrink to 50 once all colors options are re-enabled
                    maxWidth = 100
                }
            };
            rowContent.Add(colorField);
            colorField.RegisterValueChangedCallback(
                evt =>
                {
                    colorRow.NiceColor = evt.newValue;
                    UpdateColor(ColorType.MainColor, colorRow);
                }
            );

            // TODO: Uncomment once all colors options are re-enabled
            // ColorField metallicField = new ColorField
            // {
            //     value = colorRow.NiceMetallic,
            //     tooltip = colorRow.ColorProperty.Name + " Metallic",
            //     style =
            //     {
            //         width = 50
            //     }
            // };
            // rowContent.Add(metallicField);
            // metallicField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceMetallic = evt.newValue;
            //         UpdateColor(ColorType.Metallic, colorRow);
            //     }
            // );
            //
            // ColorField smoothnessField = new ColorField
            // {
            //     value = colorRow.NiceSmoothness,
            //     tooltip = colorRow.ColorProperty.Name + " Smoothness",
            //     style =
            //     {
            //         width = 50
            //     }
            // };
            // rowContent.Add(smoothnessField);
            // smoothnessField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceSmoothness = evt.newValue;
            //         UpdateColor(ColorType.Smoothness, colorRow);
            //     }
            // );
            //
            // ColorField reflectionField = new ColorField
            // {
            //     value = colorRow.NiceReflection,
            //     tooltip = colorRow.ColorProperty.Name + " Reflection",
            //     style =
            //     {
            //         width = 50
            //     }
            // };
            // rowContent.Add(reflectionField);
            // reflectionField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceReflection = evt.newValue;
            //         UpdateColor(ColorType.Reflection, colorRow);
            //     }
            // );
            //
            // ColorField emissionField = new ColorField
            // {
            //     value = colorRow.NiceEmission,
            //     tooltip = colorRow.ColorProperty.Name + " Emission",
            //     style =
            //     {
            //         width = 50
            //     }
            // };
            // rowContent.Add(emissionField);
            // emissionField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceEmission = evt.newValue;
            //         UpdateColor(ColorType.Emission, colorRow);
            //     }
            // );
            //
            // ColorField opacityField = new ColorField
            // {
            //     value = colorRow.NiceOpacity,
            //     tooltip = colorRow.ColorProperty.Name + " Opacity",
            //     style =
            //     {
            //         width = 50
            //     }
            // };
            // rowContent.Add(opacityField);
            // opacityField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceOpacity = evt.newValue;
            //         UpdateColor(ColorType.Opacity, colorRow);
            //     }
            // );
            //
            // Button randomButton = new Button
            // {
            //     text = "R",
            //     style =
            //     {
            //         right = 0
            //     }
            // };
            // rowContent.Add(randomButton);

            view.Add(row);
        }

        /// <summary>
        ///     Switches the currently visible tab to the given tab.
        /// </summary>
        /// <param name="newTab">The tab to switch to.</param>
        private void SwitchToTab(TabView newTab)
        {
            if (_currentTab == newTab)
            {
                return;
            }

            _currentTab = newTab;

            _bodyPresetTab.value = _currentTab == TabView.Preset;
            _bodyPartsTab.value = _currentTab == TabView.Parts;
            _bodyShapeTab.value = _currentTab == TabView.Body;
            _colorSelectionTab.value = _currentTab == TabView.Colors;
            // _decalSelectionTab.value = _currentTab == TabView.Decals;
            _optionTab.value = _currentTab == TabView.Options;

            _presetView.style.display = _bodyPresetTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _partView.style.display = _bodyPartsTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _bodyShapeView.style.display = _bodyShapeTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _colorSelectionView.style.display = _colorSelectionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            //_decalSelectionView.style.display = _decalSelectionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _optionSelectionView.style.display = _optionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        ///     Populate the preset tab content.
        /// </summary>
        private void PopulatePresetUI()
        {
            _presetView.Clear();

            List<PopupField<string>> dropdowns = new List<PopupField<string>>();

            Foldout speciesFoldout = new Foldout
            {
                text = "Select - Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                //tooltip = "Select the species of your character"
            };

            List<string> speciesNames = _allSpecies.Select(species => species.Name).ToList();

            _speciesPresetField = new DropdownField
            {
                label = "Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                },
                tooltip = "Select the species of your character"
            };
            _speciesPresetField.choices = speciesNames;
            _speciesPresetField.RegisterValueChangedCallback(
                evt =>
                {
                    _speciesField.value = _speciesPresetField.value;
                    ProcessSpeciesChange(evt.newValue);
                }
            );

            _speciesPresetField.index = _currentSpecies != null && speciesNames.Count > 0 ? _speciesPresetField.choices.IndexOf(_currentSpecies.Name) : 0;
            speciesFoldout.Add(_speciesPresetField);
            _presetView.Add(speciesFoldout);

            // TODO: Uncomment when Outfit Filters are properly implemented
            // Foldout filterFoldout = new Foldout()
            // {
            //     text = "Select - Outfit Filter",
            //     style =
            //     {
            //         unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
            //     }
            // };
            //
            // foreach (KeyValuePair<string, List<string>> outfitMap in _partOutfitMap)
            // {
            //     Toggle outfitToggle = new Toggle(outfitMap.Key)
            //     {
            //         value = _partOutfitToggleMap[outfitMap.Key]
            //     };
            //
            //     if (outfitToggle.value)
            //     {
            //         _availablePartList.AddRange(outfitMap.Value);
            //     }
            //
            //     outfitToggle.RegisterValueChangedCallback(
            //         evt =>
            //         {
            //             _partOutfitToggleMap[outfitMap.Key] = evt.newValue;
            //             PopulatePartUI();
            //             PopulatePresetUI();
            //         }
            //     );
            //
            //     filterFoldout.Add(outfitToggle);
            // }
            //
            // _presetView.Add(filterFoldout);

            Foldout generateFoldout = new Foldout
            {
                text = "Randomize Character",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                //tooltip = "Create a character based on the selected species"
            };

            Button generateButton = new Button()
            {
                style =
                {
                    minHeight = 50,
                    marginRight = 18,
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    alignItems = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    justifyContent = new StyleEnum<Justify>(Justify.Center)
                },
                tooltip = "Generate a character at the push of a button"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            generateButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1),
                        paddingRight = 5,
                        alignSelf = new StyleEnum<Align>(Align.Center)
                    }
                }
            );
            generateButton.Add(
                new Label
                {
                    text = "Randomize Character",
                    style =
                    {
                        alignSelf = new StyleEnum<Align>(Align.Center)
                    }
                }
            );

            generateFoldout.Add(generateButton);
            _presetView.Add(generateFoldout);

            Foldout presetsFoldout = new Foldout()
            {
                text = "Presets",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                //tooltip = "Select from a number of collections of parts, body types and colors"
            };

            Label partTitle = new Label("Parts")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Collections of character parts ie. Head attachment, Torso, Nose that make up the character"
            };

            string tooltipText = "";

            presetsFoldout.Add(partTitle);
            _presetView.Add(presetsFoldout);
            _currentHeadPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            _currentUpperBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            _currentLowerBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            foreach (PartGroup partGroup in Enum.GetValues(typeof(PartGroup)))
            {
                // only filter head part presets by species
                List<SidekickPartPreset> presets = partGroup is PartGroup.Head
                    ? SidekickPartPreset.GetAllBySpeciesAndGroup(_dbManager, _currentSpecies, partGroup)
                    : SidekickPartPreset.GetAllByGroup(_dbManager, partGroup);
                List<string> presetNames = new List<string>();
                foreach (SidekickPartPreset preset in presets)
                {
                    List<SidekickPartPresetRow> presetRows = SidekickPartPresetRow.GetAllByPreset(_dbManager, preset);
                    List<string> outfits = presetRows.Select(row => GetOutfitNameFromPartName(row.PartName)).ToList();

                    if (outfits.Any(outfit => _partOutfitToggleMap.ContainsKey(outfit) && _partOutfitToggleMap[outfit]))
                    {
                        switch (partGroup)
                        {
                            case PartGroup.Head:
                                _currentHeadPresetDictionary.Add(preset.Name, preset);
                                tooltipText = "Select a head preset for you character - a head preset is made up of parts like a head, nose, eyes and teeth etc.";
                                break;
                            case PartGroup.UpperBody:
                                _currentUpperBodyPresetDictionary.Add(preset.Name, preset);
                                tooltipText = "Select an upper body preset for you character - an upper body preset is made up of parts like a torso, arms, hands and a back attachment etc.";
                                break;
                            case PartGroup.LowerBody:
                                _currentLowerBodyPresetDictionary.Add(preset.Name, preset);
                                tooltipText = "Select a lower body preset for you character - a lower body preset is made up of parts like hips, legs, feet and hip attachments etc.";
                                break;
                        }

                        presetNames.Add(preset.Name);
                    }
                }

                string defaultValue = "None";

                if (presetNames.Count > 0)
                {
                    if (_presetDefaultValues.TryGetValue(partGroup.ToString(), out string value))
                    {
                        defaultValue = value;
                    }
                    else if (_processingSpeciesChange && partGroup == PartGroup.Head)
                    {
                        defaultValue = presetNames[Random.Range(0, presetNames.Count - 1)];
                    }
                }

                presetNames.Sort();

                dropdowns.Add(CreatePresetRow(presetsFoldout, partGroup.ToString(), tooltipText, presetNames, true, defaultValue, PresetDropdownType.Part));
            }

            Label bodyTitle = new Label("Body")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Preset bodies in a number of types, sizes and musculature"
            };

            presetsFoldout.Add(bodyTitle);
            _currentBodyPresetDictionary = new Dictionary<string, SidekickBodyShapePreset>();
            List<SidekickBodyShapePreset> bodyShapes = SidekickBodyShapePreset.GetAll(_dbManager);
            List<string> bodyShapeNames = bodyShapes.Select(b => b.Name).ToList();
            for (int i = 0; i < bodyShapeNames.Count; i++)
            {
                _currentBodyPresetDictionary.Add(bodyShapeNames[i], bodyShapes[i]);
            }

            string bodyTypeLabel = "Body Type";

            string bodyShapeDefaultValue = "Androgynous Medium";

            if (bodyShapeNames.Count > 0)
            {
                bodyShapeDefaultValue = _presetDefaultValues.TryGetValue(bodyTypeLabel, out string bodyShapeValue)
                    ? bodyShapeValue
                    : bodyShapeDefaultValue;
            }

            bodyShapeNames.Sort();

            tooltipText = "Select a body preset for you character - a body type preset is made up of combinations of body type, size and musculature.";

            dropdowns.Add(CreatePresetRow(presetsFoldout, bodyTypeLabel, tooltipText, bodyShapeNames, false, bodyShapeDefaultValue, PresetDropdownType.Body));

            Label colorTitle = new Label("Colors")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Collections of character colors ie. Skin color, teeth color, eye color, hair color that make up the characters colors"
            };

            presetsFoldout.Add(colorTitle);
            _currentColorSpeciesPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorOutfitsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorAttachmentsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorMaterialsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorElementsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            foreach (ColorGroup colorGroup in Enum.GetValues(typeof(ColorGroup)))
            {
                List<SidekickColorPreset> colorPresets = colorGroup is ColorGroup.Species
                    ? SidekickColorPreset.GetAllByColorGroupAndSpecies(_dbManager, colorGroup, _currentSpecies)
                    : SidekickColorPreset.GetAllByColorGroup(_dbManager, colorGroup);

                List<string> colorPresetNames = colorPresets.Select(cp => cp.Name).ToList();
                for (int i = 0; i < colorPresetNames.Count; i++)
                {
                    switch (colorGroup)
                    {
                        case ColorGroup.Species:
                            _currentColorSpeciesPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select a species color preset for your character - a species color preset is made up of the colors that would make up the character if it had no outfit on. (for example - skin, teeth, tongue, fingernails etc)";
                            break;
                        case ColorGroup.Outfits:
                            _currentColorOutfitsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select an outfit color preset for your character - an outfit color preset is made up of the colors that make up the clothing on the character. (for example - torso outfit, arm outfit, hand outfit etc)";
                            break;
                        case ColorGroup.Attachments:
                            _currentColorAttachmentsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select an attachments color preset for your character - an attachments color preset is made up of the colors used on additional parts on the character. (for example - shoulder attachments, back attachments, hip attachments etc)";
                            break;
                        case ColorGroup.Materials:
                            _currentColorMaterialsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select a materials color preset for your character - a materials color preset is made up of the colors that make up general materials of the outfit and attachments. (for example - metal, wood, leather, plastic, bone etc)";
                            break;/*
                        case ColorGroup.Elements:
                            _currentColorElementsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            break;*/
                    }
                }

                string defaultValue = "None";

                if (colorPresetNames.Count > 0)
                {
                    defaultValue = _presetDefaultValues.TryGetValue(colorGroup.ToString(), out string value)
                        ? value
                        : defaultValue;
                }

                colorPresetNames.Sort();

                if (_processingSpeciesChange && colorGroup == ColorGroup.Species)
                {
                    defaultValue = colorPresetNames[0];
                }

                dropdowns.Add(CreatePresetRow(presetsFoldout, colorGroup.ToString(), tooltipText, colorPresetNames, true, defaultValue, PresetDropdownType
                    .Color));
            }

            /* TODO: When decals are added (issue 1108), uncomment and update this section
            Label textureTitle = new Label("Textures")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            presetsFoldout.Add(textureTitle);
            CreatePresetRow(presetsFoldout, "Skin", new List<string>(), true, "None", PresetDropdownType.Texture);
            CreatePresetRow(presetsFoldout, "Outfit", new List<string>(), true, "None", PresetDropdownType.Texture);
            */

            generateButton.clickable.clicked += delegate
            {
                foreach (PopupField<string> dropdown in dropdowns)
                {
                    List<string> values = dropdown.choices;
                    values.Remove("None");
                    string newValue = values[Random.Range(0, values.Count - 1)];
                    dropdown.value = newValue;
                }
            };

            if (_processingSpeciesChange)
            {
                _processingSpeciesChange = false;
            }
        }

        /// <summary>
        ///     Create a selection row for the preset tab with the given values.
        /// </summary>
        /// <param name="view">The view to add the row to.</param>
        /// <param name="rowLabel">The label to put in the row.</param>
        /// <param name="tooltipText">The tooltip to display for this row.</param>
        /// <param name="dropdownValues">The values for the dropdown in the row.</param>
        /// <param name="includeNoneValue">Whether to include `None` as a value in the dropdown.</param>
        /// <param name="defaultValue">The default value to select from the dropdown.</param>
        /// <param name="dropdownType">What section of the preset UI this dropdown is part of.</param>
        /// <returns>The dropdown selection UI element.</returns>
        private PopupField<string> CreatePresetRow(
            VisualElement view,
            string rowLabel,
            string tooltipText,
            List<string> dropdownValues,
            bool includeNoneValue,
            string defaultValue,
            PresetDropdownType dropdownType)
        {
            VisualElement partContainer = new VisualElement
            {
                style =
                {
                    minHeight = 20,
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    marginTop = 2,
                    marginLeft = 15,
                    marginRight = 2,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                }
            };

            Label partTypeTitle = new Label(rowLabel.ToString())
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    width = 150
                },
                tooltip = tooltipText
            };

            Button removeButton = new Button()
            {
                tooltip = "Remove this preset, resets selection to None"
            };

            removeButton.Add(
                new Image
                {
                    image = Resources.Load<Texture2D>("UI/T_Clear"),
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            int marginLeft = 0;
            if (dropdownType != PresetDropdownType.Part)
            {
                marginLeft = 36;
            }

            Button previousButton = new Button()
            {
                tooltip = "Select the previous preset",
                style =
                {
                    marginLeft = marginLeft
                }
            };

            previousButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_prev", "|Previous Preset").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button nextButton = new Button()
            {
                tooltip = "Select the next preset"
            };

            nextButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_next", "|Next Preset").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button randomButton = new Button()
            {
                tooltip = "Randomly select a preset"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            randomButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1)
                    }
                }
            );

            if (!dropdownValues.Contains(defaultValue))
            {
                defaultValue = includeNoneValue ? "None" : dropdownValues[0];
            }

            List<string> popupValues = new List<string>();
            if (includeNoneValue)
            {
                popupValues.Add("None");
            };

            popupValues.AddRange(dropdownValues);

            PopupField<string> partSelection = new PopupField<string>(popupValues, 0)
            {
                value = "None",
                style =
                {
                    minWidth = 180
                },
                tooltip = tooltipText
            };

            partSelection.RegisterValueChangedCallback(
                evt =>
                {
                    _presetDefaultValues[rowLabel] = evt.newValue;

                    // Correctly enable/disable next and previous buttons based on selection
                    previousButton.SetEnabled(partSelection.index > 0);
                    nextButton.SetEnabled(partSelection.index < popupValues.Count - 1);

                    switch (dropdownType)
                    {
                        case PresetDropdownType.Part:
                            _applyingPreset = true;

                            bool hasErrors = false;
                            string errorMessage = "The following parts could not be found in your project:\n";
                            Enum.TryParse(rowLabel, out PartGroup group);
                            List<CharacterPartType> partTypesToRemove = group.GetPartTypes();
                            SidekickPartPreset currentPartPreset = null;
                            List<SidekickPartPresetRow> presetParts = new List<SidekickPartPresetRow>();
                            // NOTE : need to ensure evt.newValue is always in the dictionary ahead of this, or change to GetValueOrDefault()
                            if (evt.newValue != "None")
                            {
                                switch (group)
                                {
                                    case PartGroup.Head:
                                        currentPartPreset = _currentHeadPresetDictionary[evt.newValue];
                                        break;
                                    case PartGroup.UpperBody:
                                        currentPartPreset = _currentUpperBodyPresetDictionary[evt.newValue];
                                        break;
                                    case PartGroup.LowerBody:
                                        currentPartPreset = _currentLowerBodyPresetDictionary[evt.newValue];
                                        break;
                                }
                                presetParts = SidekickPartPresetRow.GetAllByPreset(_dbManager, currentPartPreset);
                            }

                            foreach (SidekickPartPresetRow presetPart in presetParts)
                            {
                                CharacterPartType partType = (CharacterPartType) int.Parse(presetPart.PartType.Substring(0, 2));
                                if (_partSelectionDictionary.TryGetValue(partType, out PopupField<string> currentField))
                                {
                                    UpdateResult result = UpdatePartDropdown(currentField, presetPart.PartName ?? "None", errorMessage, hasErrors);

                                    hasErrors = result.HasErrors;
                                    errorMessage = result.ErrorMessage;
                                    partTypesToRemove.Remove(partType);
                                }
                            }

                            foreach (CharacterPartType partType in partTypesToRemove)
                            {
                                if (_partSelectionDictionary.TryGetValue(partType, out PopupField<string> currentField))
                                {
                                    UpdateResult result = UpdatePartDropdown(currentField, "None", errorMessage, hasErrors);

                                    hasErrors = result.HasErrors;
                                    errorMessage = result.ErrorMessage;
                                }
                            }

                            if (hasErrors)
                            {
                                EditorUtility.DisplayDialog(
                                    "Assets Missing",
                                    errorMessage,
                                    "OK"
                                );
                            }

                            _applyingPreset = false;
                            if (_newModel != null)
                            {
                                DestroyImmediate(_newModel);
                            }

                            _newModel = CreateCharacter(false, true);
                            break;
                        case PresetDropdownType.Body:
                            if (Application.isEditor && Application.isPlaying)
                            {
                                return;
                            }

                            SidekickBodyShapePreset bodyShapePreset = _currentBodyPresetDictionary[evt.newValue];
                            _bodyTypeSlider.value = bodyShapePreset.BodyType;
                            _bodySizeSlider.value = bodyShapePreset.BodySize;
                            _musclesSlider.value = bodyShapePreset.Musculature;
                            break;
                        case PresetDropdownType.Color:
                            if (evt.newValue == "None")
                            {
                                return;
                            }

                            ResetCurrentColorSet();

                            Enum.TryParse(rowLabel, out ColorGroup colorGroup);
                            SidekickColorPreset colorPreset = null;
                            // NOTE : need to ensure evt.newValue is always in the dictionary ahead of this, or change to GetValueOrDefault()
                            switch (colorGroup)
                            {
                                case ColorGroup.Species:
                                    colorPreset = _currentColorSpeciesPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Outfits:
                                    colorPreset = _currentColorOutfitsPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Attachments:
                                    colorPreset = _currentColorAttachmentsPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Materials:
                                    colorPreset = _currentColorMaterialsPresetDictionary[evt.newValue];
                                    break;/*
                                case ColorGroup.Elements:
                                    colorPreset = _currentColorElementsPresetDictionary[evt.newValue];
                                    break;*/
                            }

                            List<SidekickColorPresetRow> presetColorRows = SidekickColorPresetRow.GetAllByPreset(_dbManager, colorPreset);
                            foreach (SidekickColorPresetRow row in presetColorRows)
                            {
                                SidekickColorRow existingRow = _allColorRows.Find(r => r.ColorProperty.ID == row.ColorProperty.ID);
                                if (existingRow == null)
                                {
                                    existingRow = new SidekickColorRow()
                                    {
                                        ID = -1,
                                        ColorSet = _currentColorSet,
                                        ColorProperty = row.ColorProperty,
                                        NiceColor = row.NiceColor,
                                        NiceMetallic = row.NiceMetallic,
                                        NiceSmoothness = row.NiceSmoothness,
                                        NiceReflection = row.NiceReflection,
                                        NiceEmission = row.NiceEmission,
                                        NiceOpacity = row.NiceOpacity
                                    };

                                    _allColorRows.Add(existingRow);
                                }
                                else
                                {
                                    existingRow.NiceColor = row.NiceColor;
                                    existingRow.NiceMetallic = row.NiceMetallic;
                                    existingRow.NiceSmoothness = row.NiceSmoothness;
                                    existingRow.NiceReflection = row.NiceReflection;
                                    existingRow.NiceEmission = row.NiceEmission;
                                    existingRow.NiceOpacity = row.NiceOpacity;
                                }

                                UpdateAllColors(existingRow);
                            }

                            PopulatePartColorRows();
                            RefreshVisibleColorRows();
                            break;
                        case PresetDropdownType.Texture:
                            // TODO: Add texture setting functionality once decal system in place
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dropdownType), dropdownType, null);
                    }
                }
            );

            removeButton.clickable.clicked += delegate
            {
                // TODO: Change to default character when available
                partSelection.value = "None";
            };

            previousButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index - 1;
                if (newIndex <= 0)
                {
                    newIndex = 0;
                }

                partSelection.index = newIndex;
            };

            nextButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index + 1;
                if (newIndex >= partSelection.choices.Count - 1)
                {
                    newIndex = partSelection.choices.Count - 1;
                }

                partSelection.index = newIndex;
            };

            randomButton.clickable.clicked += delegate
            {
                int currentIndex = partSelection.index;
                if (partSelection.choices.Count - 1 > 1)
                {
                    while (partSelection.index == currentIndex)
                    {
                        partSelection.index = Random.Range(1, partSelection.choices.Count);
                    }
                }
            };

            partContainer.Add(partTypeTitle);
            if (dropdownType == PresetDropdownType.Part)
            {
                partContainer.Add(removeButton);
            }
            partContainer.Add(previousButton);
            partContainer.Add(nextButton);
            partContainer.Add(randomButton);
            partContainer.Add(partSelection);
            view.Add(partContainer);

            partSelection.value = defaultValue;

            return partSelection;
        }

        /// <summary>
        ///     Updates the required parts of the UI on a species change.
        /// </summary>
        /// <param name="newSpecies">The name of the species being changed to.</param>
        private void ProcessSpeciesChange(string newSpecies)
        {
            // don't need to re-process if multiple callbacks are triggered
            if (_currentSpecies.Name == newSpecies)
            {
                return;
            }

            _currentSpecies = _allSpecies.FirstOrDefault(species => species.Name == newSpecies);
            UpdateVisibleColorSets();
            ResetCurrentColorSet();
            if (_allColorRows.Count == 0)
            {
                PopulateColorRowsFromTextures();
            }

            // removing the default values here ensures we don't have key issues from preset dropdown choice changes
            _presetDefaultValues.Remove("Head");
            _presetDefaultValues.Remove("Species");
            _processingSpeciesChange = true;
            PopulatePartUI();
            PopulatePresetUI();
        }

        /// <summary>
        ///     Populates the parts UI.
        /// </summary>
        private void PopulatePartUI()
        {
            _partView.Clear();

            _availablePartList = new List<string>();
            _partSelectionDictionary = new Dictionary<CharacterPartType, PopupField<string>>();

            Foldout speciesFoldout = new Foldout
            {
                text = "Select - Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            List<string> speciesNames = _allSpecies.Select(species => species.Name).ToList();

            _speciesField = new DropdownField
            {
                label = "Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                },
                tooltip = "Select the species of your character"
            };
            _speciesField.choices = speciesNames;
            _speciesField.RegisterValueChangedCallback(
                evt =>
                {
                    _speciesPresetField.value = _speciesField.value;
                    ProcessSpeciesChange(evt.newValue);
                }
            );

            _speciesField.index = _currentSpecies != null && speciesNames.Count > 0 ? _speciesField.choices.IndexOf(_currentSpecies.Name) : 0;
            speciesFoldout.Add(_speciesField);
            _partView.Add(speciesFoldout);

            // TODO: Uncomment when Outfit Filters are properly implemented
            // Foldout filterFoldout = new Foldout()
            // {
            //     text = "Select - Outfit Filter",
            //     style =
            //     {
            //         unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
            //     }
            // };
            //
            foreach (KeyValuePair<string, List<string>> outfitMap in _partOutfitMap)
            {
                Toggle outfitToggle = new Toggle(outfitMap.Key)
                {
                    value = _partOutfitToggleMap[outfitMap.Key]
                };

                if (outfitToggle.value)
                {
                    _availablePartList.AddRange(outfitMap.Value);
                }

                // TODO: Hidden due to early access, enable when feature complete
                //
                //     outfitToggle.RegisterValueChangedCallback(
                //         evt =>
                //         {
                //             _partOutfitToggleMap[outfitMap.Key] = evt.newValue;
                //             PopulatePartUI();
                //         }
                //     );
                //
                //     filterFoldout.Add(outfitToggle);
            }

            // TODO: Hidden due to early access, enable when feature complete
            //
            // _partView.Add(filterFoldout);

            Foldout partsFoldout = new Foldout()
            {
                text = "Select - Parts",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            foreach (PartGroup partGroup in Enum.GetValues(typeof(PartGroup)))
            {
                string labelText = StringUtils.AddSpacesBeforeCapitalLetters(partGroup.ToString());
                Foldout partGroupFoldout = new Foldout
                {
                    text = labelText,
                    style =
                    {
                        marginLeft = 15,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                    }
                };

                foreach (CharacterPartType value in partGroup.GetPartTypes())
                {
                    VisualElement partContainer = new VisualElement
                    {
                        style =
                        {
                            minHeight = 20,
                            display = DisplayStyle.Flex,
                            flexDirection = FlexDirection.Row,
                            marginBottom = 2,
                            marginTop = 2,
                            marginRight = 2,
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                        }
                    };
                    BuildPartDetails(value, partContainer);
                    partGroupFoldout.Add(partContainer);
                }
                partsFoldout.Add(partGroupFoldout);
            }

            _partView.Add(partsFoldout);
        }

        /// <summary>
        ///     Builds the UI details for each different part type.
        /// </summary>
        /// <param name="type">The type of the part to build the UI for.</param>
        /// <param name="partContainer">The container to add the UI to.</param>
        private void BuildPartDetails(CharacterPartType type, VisualElement partContainer)
        {
            Dictionary<string, string> partsList = _partLibrary[type];

            Label partTypeTitle = new Label(type.ToString())
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    width = 150
                },
                tooltip = type.GetTooltipForPartType()
            };

            Button removeButton = new Button()
            {
                tooltip = "Remove this part"
            };

            removeButton.Add(
                new Image
                {
                    image = Resources.Load<Texture2D>("UI/T_Clear"),
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button previousButton = new Button()
            {
                tooltip = "Select the previous part"
            };

            previousButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_prev", "|Previous Part").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button nextButton = new Button()
            {
                tooltip = "Select the next part"
            };

            nextButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_next", "|Next Part").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button randomButton = new Button()
            {
                tooltip = "Randomly select a part"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            randomButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1)
                    }
                }
            );

            List<string> popupValues = new List<string>
            {
                "None"
            };

            foreach (string key in partsList.Keys.ToList())
            {
                if ((!(type.IsSpeciesSpecificPartType() || PartUtils.IsBaseSpeciesPart(key))
                        || _currentSpecies == SidekickPart.GetSpeciesForPart(_allSpecies, key))
                    && _availablePartList.Contains(key))
                {
                    popupValues.Add(key);
                }
            }

            string currentSelection = _currentPartSelections[type];
            if (!popupValues.Contains(currentSelection))
            {
                if (_processingSpeciesChange && PartUtils.IsBaseSpeciesPart(currentSelection))
                {
                    currentSelection = popupValues.Find(n => n.Contains("BASE"))?? "None";
                }
                else
                {
                    currentSelection = "None";
                }
            }



            PopupField<string> partSelection = new PopupField<string>(popupValues, 0)
            {
                value = currentSelection
            };
            partSelection.RegisterCallback<ChangeEvent<string>>(
                changeEvent =>
                {
                    if (partsList.TryGetValue(changeEvent.newValue, out string partPath))
                    {
                        GameObject selectedPart = AssetDatabase.LoadAssetAtPath<GameObject>(partPath);
                        SkinnedMeshRenderer selectedMesh = selectedPart.GetComponentInChildren<SkinnedMeshRenderer>();
                        _partDictionary[type] = selectedMesh;
                        _currentPartSelections[type] = changeEvent.newValue;
                    }
                    else
                    {
                        _partDictionary.Remove(type);
                    }

                    if (!_applyingPreset && _previewToggle.value)
                    {
                        if (_newModel != null) DestroyImmediate(_newModel);
                        _newModel = CreateCharacter(false, true);
                    }

                    previousButton.SetEnabled(partSelection.index > 0);
                    nextButton.SetEnabled(partSelection.index < popupValues.Count - 1);
                    removeButton.SetEnabled(!partSelection.value.Equals("None"));
                    randomButton.SetEnabled(partSelection.choices.Count > 2);
                }
            );

            _partSelectionDictionary.Add(type, partSelection);

            // If a part has previously been selected when the library is updated, it will try to reselect that part.
            if (_partDictionary.TryGetValue(type, out SkinnedMeshRenderer skinnedMesh))
            {
                if (partSelection.choices.Contains(skinnedMesh.name))
                {
                    partSelection.index = partSelection.choices.IndexOf(skinnedMesh.name);
                }
                else
                {
                    partSelection.value = "None";
                    _partDictionary.Remove(type);
                }
            }

            // Disable buttons based on what the current selected index is, or both if no parts to select.
            previousButton.SetEnabled(partSelection.index > 0);
            nextButton.SetEnabled(partSelection.index < partsList.Count);
            removeButton.SetEnabled(!partSelection.value.Equals("None"));
            randomButton.SetEnabled(partSelection.choices.Count > 2);

            // If there are no items (excluding "None"), disable the dropdown and randomize button.
            if (partsList.Count == 0)
            {
                partSelection.SetEnabled(false);
                randomButton.SetEnabled(false);
            }

            removeButton.clickable.clicked += delegate
            {
                partSelection.value = "None";
            };

            previousButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index - 1;
                if (newIndex <= 0)
                {
                    newIndex = 0;
                }

                partSelection.index = newIndex;
            };

            nextButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index + 1;
                if (newIndex >= partSelection.choices.Count - 1)
                {
                    newIndex = partSelection.choices.Count - 1;
                }

                partSelection.index = newIndex;
            };

            randomButton.clickable.clicked += delegate
            {
                int currentIndex = partSelection.index;
                if (partSelection.choices.Count - 1 > 1)
                {
                    while (partSelection.index == currentIndex)
                    {
                        partSelection.index = Random.Range(1, partSelection.choices.Count);
                    }
                }
            };

            partContainer.Add(partTypeTitle);
            partContainer.Add(removeButton);
            partContainer.Add(previousButton);
            partContainer.Add(nextButton);
            partContainer.Add(randomButton);
            partContainer.Add(partSelection);
        }

        /// <summary>
        ///     Determines the part type from the part name. This will work as long as the naming format is correct.
        /// </summary>
        /// <param name="partName">The name of the part.</param>
        /// <returns>The part type.</returns>
        private CharacterPartType ExtractPartType(string partName)
        {
            string partIndexString = partName.Split('_').Reverse().ElementAt(1).Substring(0, 2);
            bool valueParsed = int.TryParse(partIndexString, out int index);
            return valueParsed ? (CharacterPartType) index : 0;
        }

        /// <summary>
        ///     Takes all the parts selected in the window, and combines them into a single model in the scene.
        /// </summary>
        /// <param name="combineMesh">When true the character mesh will be combined into a single mesh.</param>
        /// <param name="processBoneMovement">When true the bones will be moved to match the blendshape settings.</param>
        /// <returns>A new character object.</returns>
        private GameObject CreateCharacter(bool combineMesh, bool processBoneMovement)
        {
            List<SkinnedMeshRenderer> toCombine = _partDictionary.Values.ToList();
            PopulateUVDictionary(toCombine);
            GameObject baseModel = (GameObject) _baseModelField.value;

            GameObject newSpawn;

            if (combineMesh)
            {
                newSpawn = Combiner.CreateCombinedSkinnedMesh(toCombine, baseModel, (Material) _materialField.value);
            }
            else
            {
                newSpawn = CreateModelFromParts(baseModel, toCombine);
            }

            newSpawn.name = _OUTPUT_MODEL_NAME;
            if (_currentMaterial == null)
            {
                _currentMaterial = (Material) _materialField.value;
            }

            Renderer renderer = newSpawn.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _currentMaterial;
            }

            Animator newModelAnimator = newSpawn.AddComponent<Animator>();
            Animator baseModelAnimator = baseModel.GetComponentInChildren<Animator>();
            newModelAnimator.avatar = baseModelAnimator.avatar;
            newModelAnimator.Rebind();
            UpdateBlendShapes(newSpawn);

            if (processBoneMovement)
            {
                ProcessRigMovementOnBlendShapeChange();
                ProcessBoneMovement(newSpawn);
            }

            return newSpawn;
        }

        /// <summary>
        ///     Creates the model but with all parts as separate meshes.
        /// </summary>
        /// <param name="baseModel">The base model to get the rig from.</param>
        /// <param name="parts">The parts to build into the character</param>
        /// <returns>A new game object with all the part meshes and a single rig.</returns>
        private GameObject CreateModelFromParts(GameObject baseModel, List<SkinnedMeshRenderer> parts)
        {
            GameObject partsModel = new GameObject(_OUTPUT_MODEL_NAME);
            Material partMaterial = (Material) _materialField.value;
            Transform modelRootBone = baseModel.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
            Transform[] bones = Array.Empty<Transform>();
            GameObject rootBone = Instantiate(modelRootBone.gameObject, partsModel.transform, true);
            rootBone.name = modelRootBone.name;
            Hashtable boneNameMap = Combiner.CreateBoneNameMap(rootBone);
            Transform[] additionalBones = Combiner.FindAdditionalBones(boneNameMap, new List<SkinnedMeshRenderer>(parts));
            if (additionalBones.Length > 0)
            {
                Combiner.JoinAdditionalBonesToBoneArray(bones, additionalBones, boneNameMap);
                // Need to redo the name map now that we have updated the bone array.
                boneNameMap = Combiner.CreateBoneNameMap(rootBone);
            }

            for (int i = 0; i < parts.Count; i++)
            {
                SkinnedMeshRenderer part = parts[i];
                GameObject newPart = new GameObject(part.name);
                newPart.transform.parent = partsModel.transform;
                SkinnedMeshRenderer renderer = newPart.AddComponent<SkinnedMeshRenderer>();
                Transform[] oldBones = part.bones;
                Transform[] newBones = new Transform[part.bones.Length];
                for (int j = 0; j < oldBones.Length; j++)
                {
                    newBones[j] = (Transform) boneNameMap[oldBones[j].name];
                }

                renderer.sharedMesh = MeshUtils.CopyMesh(part.sharedMesh);
                renderer.rootBone = (Transform) boneNameMap[part.rootBone.name];

                Combiner.MergeAndGetAllBlendShapeDataOfSkinnedMeshRenderers(
                    new SkinnedMeshRenderer[]
                    {
                        part
                    },
                    renderer.sharedMesh,
                    renderer
                );

                renderer.bones = newBones;
                renderer.sharedMaterial = partMaterial;
            }

            return partsModel;
        }

        /// <summary>
        ///     Saves a character (Parts and Colors) out to a file which can be imported by the tool into any project.
        /// </summary>
        private void SaveCharacter()
        {
            SaveCharacter(null);
        }

        /// <summary>
        ///     Saves a character (Parts and Colors) out to a file which can be imported by the tool into any project to a given path.
        /// </summary>
        /// <param name="savePath">The path to save the character to.</param>
        private void SaveCharacter(string savePath)
        {
            bool showSuccessMessage = false;

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = ShowCharacterSaveDialog();
                showSuccessMessage = true;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                // EditorUtility.DisplayDialog("Save Cancelled", "No save file selected. Saving cancelled.", "OK");
                return;
            }

            SerializedCharacter savedCharacter = new SerializedCharacter
            {
                Species = _currentSpecies.ID
            };

            List<SerializedPart> usedParts = new List<SerializedPart>();
            foreach (KeyValuePair<CharacterPartType, SkinnedMeshRenderer> entry in _partDictionary)
            {
                // TODO: Update the part version to use actual version once the information is available.
                usedParts.Add(new SerializedPart(entry.Value.name, entry.Key, "1"));
            }

            savedCharacter.Parts = usedParts;
            SerializedColorSet savedSet = new SerializedColorSet();
            savedSet.PopulateFromSidekickColorSet(_currentColorSet, _currentSpecies);
            savedCharacter.ColorSet = savedSet;

            savedCharacter.BlendShapes = new SerializedBlendShapeValues()
            {
                BodyTypeValue = _bodyTypeSlider.value,
                BodySizeValue = _bodySizeSlider.value,
                MuscleValue = _musclesSlider.value
            };

            List<SerializedColorRow> savedColorRows = new List<SerializedColorRow>();
            foreach (SidekickColorRow row in _allColorRows)
            {
                savedColorRows.Add(new SerializedColorRow(row));
            }

            savedCharacter.ColorRows = savedColorRows;

            Serializer serializer = new Serializer();

            File.WriteAllBytes(savePath, Encoding.ASCII.GetBytes(serializer.Serialize(savedCharacter)));
            if (showSuccessMessage)
            {
                EditorUtility.DisplayDialog("Save Successful", "Character successfully saved to " + Path.GetFileName(savePath), "OK");
            }
        }

        /// <summary>
        ///     Loads a character (Parts and Colors) into the tool.
        /// </summary>
        private void LoadCharacter()
        {
            bool showAllColors = _showAllColourProperties;
            _showAllColourProperties = true;

            string filePath = EditorUtility.OpenFilePanel("Load Character", "", "sk");
            if (string.IsNullOrEmpty(filePath))
            {
                EditorUtility.DisplayDialog("No File Chosen", "No file was chosen to load.", "OK");
                return;
            }

            _bodyPartsTab.value = true;
            SwitchToTab(TabView.Parts);

            byte[] bytes = File.ReadAllBytes(filePath);
            string data = Encoding.ASCII.GetString(bytes);

            Deserializer deserializer = new Deserializer();
            SerializedCharacter savedCharacter = deserializer.Deserialize<SerializedCharacter>(data);

            SidekickSpecies species = SidekickSpecies.GetByID(_dbManager, savedCharacter.Species);
            _speciesField.value = species.Name;
            ProcessSpeciesChange(species.Name);

            bool hasErrors = false;
            string errorMessage = "The following parts could not be found in your project:\n";
            foreach (CharacterPartType currentType in Enum.GetValues(typeof(CharacterPartType)))
            {
                PopupField<string> currentField = _partSelectionDictionary[currentType];
                SerializedPart part = savedCharacter.Parts.FirstOrDefault(p => p.PartType == currentType);
                if (part != null)
                {
                    UpdateResult result = UpdatePartDropdown(currentField, part.Name, errorMessage, hasErrors);
                    hasErrors = result.HasErrors;
                    errorMessage = result.ErrorMessage;
                }
                else
                {
                    currentField.value = "None";
                }
            }

            if (hasErrors)
            {
                EditorUtility.DisplayDialog(
                    "Assets Missing",
                    errorMessage,
                    "OK"
                );
            }

            LoadColorSet(savedCharacter);

            if (savedCharacter.BlendShapes != null)
            {
                LoadBlendShapes(savedCharacter);
            }

            _showAllColourProperties = showAllColors;
            UpdateColorTabContent();
        }

        /// <summary>
        ///     Updates a part dropdown to select a new part, if the part is not in the dropdown values, `None` is selected instead.
        /// </summary>
        /// <param name="currentField">The dropdown field to update.</param>
        /// <param name="partName">The new part to select.</param>
        /// <param name="errorMessage">The error message to update if the part is not available.</param>
        /// <param name="hasErrors">The error flag to update if an error is encountered.</param>
        /// <returns>A PartUpdateResult with the results of the update.</returns>
        private UpdateResult UpdatePartDropdown(PopupField<string> currentField, string partName, string errorMessage, bool hasErrors)
        {
            if (currentField.choices.Contains(partName))
            {
                currentField.value = partName;
            }
            else if (PartUtils.IsBaseSpeciesPart(partName))
            {
                currentField.value = currentField.choices.Find(n => n.Contains("BASE")) ?? "None";
                if (currentField.value == "None")
                {
                    hasErrors = true;
                    errorMessage += partName + "\n";
                }
            }
            else
            {
                currentField.value = "None";
                hasErrors = true;
                errorMessage += partName + "\n";
            }

            return new UpdateResult(errorMessage, hasErrors);
        }

        /// <summary>
        ///     Loads the color set for a saved character into memory.
        /// </summary>
        /// <param name="savedCharacter">The character to load the color set for.</param>
        private void LoadColorSet(SerializedCharacter savedCharacter)
        {
            _currentColorSet = savedCharacter.ColorSet.CreateSidekickColorSet(_dbManager);
            _colorSetsDropdown.value = "Custom";

            List<SidekickColorRow> newRows = new List<SidekickColorRow>();
            foreach (SerializedColorRow row in savedCharacter.ColorRows)
            {
                newRows.Add(row.CreateSidekickColorRow(_dbManager, _currentColorSet));
            }

            _allColorRows = newRows;
            UpdateColorTabContent();
        }

        /// <summary>
        ///     Updates the content of the Color Tab.
        /// </summary>
        private void UpdateColorTabContent()
        {
            PopulatePartColorRows();
            UpdateAllVisibleColors();
            RefreshVisibleColorRows();
        }

        /// <summary>
        ///     Loads the blend shapes from a saved character into the tool.
        /// </summary>
        /// <param name="savedCharacter">The character to load the blend shapes for.</param>
        private void LoadBlendShapes(SerializedCharacter savedCharacter)
        {
            _bodyTypeSlider.value = savedCharacter.BlendShapes.BodyTypeValue;
            _bodySizeSlider.value = savedCharacter.BlendShapes.BodySizeValue;
            _musclesSlider.value = savedCharacter.BlendShapes.MuscleValue;
        }

        /// <summary>
        ///     Shows the dialog box for where to save the character to, and also validates the save location and filename.
        /// </summary>
        /// <param name="path">The default path to save to.</param>
        /// <returns>The full file path and filename to save the character to.</returns>
        private string ShowCharacterSaveDialog(string path = "")
        {
            string defaultName = _currentSpecies.Name + "-" + _currentColorSet.Name + ".sk";
            string defaultDirectory = "";
            if (!string.IsNullOrEmpty(path))
            {
                defaultName = Path.GetFileName(path);
                defaultDirectory = Path.GetDirectoryName(path);
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Save New Character",
                defaultDirectory,
                defaultName,
                "sk"
            );

            // if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            // {
            //     int option = EditorUtility.DisplayDialogComplex(
            //         "File Already Exists",
            //         "A file already exists with the same name, are you sure you wish to overwrite it?\nThis cannot be undone.",
            //         "Overwrite",
            //         "Rename",
            //         "Cancel"
            //     );
            //
            //     switch (option)
            //     {
            //         // Overwrite.
            //         case 0:
            //             EditorUtility.DisplayDialog("Overwrite Accepted", "Existing file will be overwritten.", "OK");
            //             break;
            //
            //         // Rename.
            //         case 1:
            //             savePath = ShowCharacterSaveDialog(savePath);
            //             break;
            //
            //         // Cancel.
            //         case 2:
            //         default:
            //             savePath = null;
            //             break;
            //     }
            // }

            return savePath;
        }

        /// <summary>
        ///     Saves a created character as a prefab.
        /// </summary>
        private void CreateCharacterPrefab()
        {
            string savePath = SelectPrefabSaveLocation();

            if (string.IsNullOrEmpty(savePath))
            {
                // EditorUtility.DisplayDialog("Save Cancelled", "No save file selected. Saving cancelled.", "OK");
                return;
            }

            string baseFilename = Path.GetFileNameWithoutExtension(savePath);
            string directoryBase = Path.GetDirectoryName(savePath)?? string.Empty;
            string directory = Path.Combine(directoryBase, baseFilename);
            savePath = Path.Combine(directory, Path.GetFileName(savePath));
            string textureDirectory = Path.Combine(directory, "Textures");
            string meshDirectory = Path.Combine(directory, "Meshes");
            string materialDirectory = Path.Combine(directory, "Materials");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!Directory.Exists(meshDirectory))
            {
                Directory.CreateDirectory(meshDirectory);
            }

            if (!Directory.Exists(materialDirectory))
            {
                Directory.CreateDirectory(materialDirectory);
            }

            string savedCharacterPath = Path.Combine(directory, baseFilename + ".sk");
            SaveCharacter(savedCharacterPath);

            //TODO textures are shared between exports!
            SaveTexturesToDisk(textureDirectory, baseFilename);
            // Ensure textures are written to disk before proceeding. As it seems this happens outside the main Unity loop, so can't be easily checked
            // for.
            int cutoff = 0;
            while (cutoff < 1000000000 && Directory.GetFiles(textureDirectory).Length <= 0)
            {
                cutoff++;
            }
            AssetDatabase.Refresh();

            GameObject clonedModel = CreateCharacter(true, false); // don't process bone movements here. we need to bake the mesh first

            SkinnedMeshRenderer clonedRenderer = clonedModel.GetComponentInChildren<SkinnedMeshRenderer>();

            // Copy mesh, bone weights and bindposes before baking so the mesh can be re-skinned after baking.
            Mesh clonedSkinnedMesh = MeshUtils.CopyMesh(clonedRenderer.sharedMesh);
            BoneWeight[] boneWeights = clonedSkinnedMesh.boneWeights;
            Matrix4x4[] bindposes = clonedSkinnedMesh.bindposes;
            List<BlendShapeData> blendData = BlendShapeUtils.GetBlendShapeData(
                clonedSkinnedMesh,
                clonedRenderer,
                new string[]
                {
                    "defaultHeavy",
                    "defaultBuff",
                    "defaultSkinny",
                    "masculineFeminine"
                },
                0
            );
            clonedRenderer.BakeMesh(clonedSkinnedMesh);
            // Re-skin the new baked mesh.
            clonedSkinnedMesh.boneWeights = boneWeights;
            clonedSkinnedMesh.bindposes = bindposes;
            // assign the new mesh to the renderer
            clonedRenderer.sharedMesh = clonedSkinnedMesh;

            BlendShapeUtils.RestoreBlendShapeData(blendData, clonedSkinnedMesh, clonedRenderer);

            // now do the bone movements!
            ProcessRigMovementOnBlendShapeChange();
            ProcessBoneMovement(clonedModel);

            clonedRenderer.sharedMaterial = CreateNewMaterialAssetFromSource(clonedRenderer.sharedMaterial, textureDirectory, materialDirectory, baseFilename, baseFilename);
            CreatePrefab(clonedModel, meshDirectory, savePath, baseFilename);
            DestroyImmediate(clonedModel);
        }

        /// <summary>
        ///     Prompts the user to select a path and prefab name within the project.
        /// </summary>
        /// <returns>The path and filename to use to save the prefab to.</returns>
        private string SelectPrefabSaveLocation()
        {
            string defaultName = _currentSpecies.Name + "-" + _currentColorSet.Name + ".prefab";

            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save Character Prefab",
                defaultName,
                "prefab",
                "Select where to save the prefab"
            );

            return savePath;
        }

        /// <summary>
        ///     Sets the material to use the textures at the given location.
        /// </summary>
        /// <param name="material">The material to set the textures on.</param>
        /// <param name="texturePath">The path to set the textures from.</param>
        /// <param name="textureName">Additional naming for the textures, if applicable.</param>
        /// <returns>The material with the paths set on it.</returns>
        private Material SetTextureLinkOnMaterial(Material material, string texturePath, string textureName = "")
        {
            string filename = _TEXTURE_PREFIX;

            if (!string.IsNullOrEmpty(textureName))
            {
                filename += textureName;
            }

            material.SetTexture(_COLOR_MAP, null);
            LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_COLOR_NAME, _COLOR_MAP);
            // TODO: Uncomment when the shader has the these properties are enabled again
            // material.SetTexture(_METALLIC_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_METALLIC_NAME, _METALLIC_MAP);
            // material.SetTexture(_SMOOTHNESS_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_SMOOTHNESS_NAME, _SMOOTHNESS_MAP);
            // material.SetTexture(_REFLECTION_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_REFLECTION_NAME, _REFLECTION_MAP);
            // material.SetTexture(_EMISSION_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_EMISSION_NAME, _EMISSION_MAP);
            // material.SetTexture(_OPACITY_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_OPACITY_NAME, _OPACITY_MAP);

            return material;
        }

        /// <summary>
        ///     Loads a texture from disk and assigns it to the material in the given texture ID.
        /// </summary>
        /// <param name="material">The material to assign the texture to.</param>
        /// <param name="texturePath">The path to load the texture from.</param>
        /// <param name="textureName">The name of the texture to load.</param>
        /// <param name="textureID">The texture ID to load the texture into on the material.</param>
        private void LoadAndAssignTexture(Material material, string texturePath, string textureName, int textureID)
        {
            string filePath = Path.Combine(texturePath, textureName);
            while (material.GetTexture(textureID) == null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                    textureImporter.filterMode = FilterMode.Point;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        maxTextureSize = 32,
                        resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
                        format = TextureImporterFormat.RGB24
                    });
                    EditorUtility.SetDirty(textureImporter);
                    textureImporter.SaveAndReimport();
                }

                material.SetTexture(textureID, (Texture2D) AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)));
            }
        }

        /// <summary>
        ///     Creates a new material to assign to the prefab.
        /// </summary>
        /// <param name="sourceMaterial">The existing material from the base model.</param>
        /// <param name="textureDirectory">The directory to save the textures into.</param>
        /// <param name="materialDirectory">The directory to save the material into.</param>
        /// <param name="baseFilename">The base filename to use for all assets.</param>
        /// <param name="textureName">Additional naming for the textures, if applicable.</param>
        /// <returns>The new material cloned from sourceMaterial saved to the asset database.</returns>
        private Material CreateNewMaterialAssetFromSource(
            Material sourceMaterial,
            string textureDirectory,
            string materialDirectory,
            string baseFilename,
            string textureName = ""
        )
        {
            Material clonedMaterial = new Material(sourceMaterial.shader);
            // NOTE: this is copying the texture slots from oldMaterial, so they need to be null'd afterward in SetTextureLinkOnMaterial()
            clonedMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            clonedMaterial = SetTextureLinkOnMaterial(clonedMaterial, textureDirectory, textureName);
            string materialPath = Path.Combine(materialDirectory, baseFilename + ".mat");
            // If the user has chosen to overwrite the prefab, delete the existing assets to replace them.
            if (File.Exists(materialPath))
            {
                File.Delete(materialPath);
            }

            AssetDatabase.CreateAsset(clonedMaterial, materialPath);
            return clonedMaterial;
        }

        /// <summary>
        ///     Creates a prefab and the required assets for the model to work as an independent asset.
        /// </summary>
        /// <param name="rootGameObject">Root game object for the prefab.</param>
        /// <param name="meshDirectory">The directory to save the mesh and avatar assets to.</param>
        /// <param name="savePath">The path to save the prefab to.</param>
        /// <param name="baseFilename">The base filename to use for all assets.</param>
        private void CreatePrefab(
            GameObject rootGameObject,
            string meshDirectory,
            string savePath,
            string baseFilename
        )
        {
            SkinnedMeshRenderer renderer = rootGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            Mesh sharedMesh = renderer.sharedMesh;
            string meshPath = Path.Combine(meshDirectory, baseFilename + ".asset");
            // If the user has chosen to overwrite the prefab, delete the existing assets to replace them.
            if (File.Exists(meshPath))
            {
                File.Delete(meshPath);
            }

            AssetDatabase.CreateAsset(sharedMesh, meshPath);
            Animator animator = rootGameObject.GetComponentInChildren<Animator>();
            Avatar existingAvatar = animator.avatar;
            Avatar newAvatar = Instantiate(existingAvatar);
            animator.avatar = newAvatar;
            string avatarPath = Path.Combine(meshDirectory, baseFilename + "-avatar.asset");
            // If the user has chosen to overwrite the prefab, delete the existing assets to replace them.
            if (File.Exists(avatarPath))
            {
                File.Delete(avatarPath);
            }

            AssetDatabase.CreateAsset(newAvatar, avatarPath);
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAsset(rootGameObject, savePath);
        }

        /// <summary>
        ///     Populates the list of current UVs and UV part dictionary.
        /// </summary>
        private void PopulateUVDictionary(List<SkinnedMeshRenderer> usedParts)
        {
            _currentUVList = new List<Vector2>();
            _currentUVDictionary = new Dictionary<ColorPartType, List<Vector2>>();

            foreach (ColorPartType type in Enum.GetValues(typeof(ColorPartType)))
            {
                _currentUVDictionary.Add(type, new List<Vector2>());
            }

            foreach (SkinnedMeshRenderer skinnedMesh in usedParts)
            {
                ColorPartType type = Enum.Parse<ColorPartType>(ExtractPartType(skinnedMesh.name).ToString());
                List<Vector2> partUVs = _currentUVDictionary[type];
                foreach (Vector2 uv in skinnedMesh.sharedMesh.uv)
                {
                    int scaledU = (int) Math.Floor(uv.x * 16);
                    int scaledV = (int) Math.Floor(uv.y * 16);

                    if (scaledU == 16)
                    {
                        scaledU = 15;
                    }

                    if (scaledV == 16)
                    {
                        scaledV = 15;
                    }

                    Vector2 scaledUV = new Vector2(scaledU, scaledV);
                    // For the global UV list, we don't want any duplicates on a global level
                    if (!_currentUVList.Contains(scaledUV))
                    {
                        _currentUVList.Add(scaledUV);
                    }

                    // For the part specific UV list we may have UVs that are in the global list already, we don't want to exclude these, so check
                    // them separately to the global list
                    if (!partUVs.Contains(scaledUV))
                    {
                        partUVs.Add(scaledUV);
                    }
                }

                _currentUVDictionary[type] = partUVs;
            }
        }

        /// <summary>
        ///     Updates the blend shape values of the combined model.
        /// </summary>
        private void UpdateBlendShapes(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            List<SkinnedMeshRenderer> allMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            foreach (SkinnedMeshRenderer skinnedMesh in allMeshes)
            {
                Mesh sharedMesh = skinnedMesh.sharedMesh;
                for (int i = 0; i < sharedMesh.blendShapeCount; i++)
                {
                    string blendName = sharedMesh.GetBlendShapeName(i);
                    if (blendName.Contains(_BLEND_GENDER_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, (_bodyTypeBlendValue + 100) / 2);
                    }
                    else if (blendName.Contains(_BLEND_SHAPE_SKINNY_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, _bodySizeSkinnyBlendValue);
                    }
                    else if (blendName.Contains(_BLEND_SHAPE_HEAVY_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, _bodySizeHeavyBlendValue);
                    }
                    else if (blendName.Contains(_BLEND_MUSCLE_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, (_musclesBlendValue + 100) / 2);
                    }
                }
            }
        }

        /// <summary>
        ///     Populates the internal library of parts based on the files in the project.
        /// </summary>
        private void PopulatePartLibrary()
        {
            int totalPartCount = 0;

            _partLibrary = new Dictionary<CharacterPartType, Dictionary<string, string>>();
            _partOutfitMap = new Dictionary<string, List<string>>();
            _partOutfitToggleMap = new Dictionary<string, bool>();

            List<string> files = Directory.GetFiles("Assets", "SK_*.fbx", SearchOption.AllDirectories).ToList();

            foreach (CharacterPartType partType in Enum.GetValues(typeof(CharacterPartType)))
            {
                Dictionary<string, string> partLocationDictionary = new Dictionary<string, string>();

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string partName = fileInfo.Name;
                    partName = partName.Substring(0, partName.IndexOf(".fbx", StringComparison.Ordinal));
                    CharacterPartType characterPartType = ExtractPartType(partName);
                    if (characterPartType > 0 && characterPartType == partType && !partLocationDictionary.ContainsKey(partName))
                    {
                        partLocationDictionary.Add(partName, file);
                        totalPartCount++;

                        // TODO: populate with actual outfit data when we have proper information about part outfits
                        string tempPartOutfit = GetOutfitNameFromPartName(partName);
                        List<string> partNameList;
                        if (_partOutfitMap.TryGetValue(tempPartOutfit, out List<string> value))
                        {
                            partNameList = value;
                            partNameList.Add(partName);
                            _partOutfitMap[tempPartOutfit] = partNameList;
                        }
                        else
                        {
                            partNameList = new List<string>();
                            partNameList.Add(partName);
                            _partOutfitMap.Add(tempPartOutfit, partNameList);
                            _partOutfitToggleMap.Add(tempPartOutfit, true);
                        }

                    }
                }

                _partLibrary.Add(partType, partLocationDictionary);
            }

            _partCountLabel.text = totalPartCount + _PART_COUNT_BODY;
            PopulatePartUI();
        }

        /// <summary>
        ///     Gets the "outfit" name from the part name.
        ///     TODO: This will be replaced once parts and outfits have a proper relationship.
        /// </summary>
        /// <param name="partName">The part name to parse the "outfit" name from.</param>
        /// <returns>The "outfit" name.</returns>
        private string GetOutfitNameFromPartName(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return "None";
            }

            return string.Join('_', partName.Substring(3).Split('_').Take(2));
        }

        /// <summary>
        ///     The available tab views
        /// </summary>
        private enum TabView
        {
            Preset,
            Parts,
            Body,
            Colors,
            Decals,
            Options
        }

        /// <summary>
        ///     The different types of preset dropdown
        /// </summary>
        private enum PresetDropdownType
        {
            Part,
            Body,
            Color,
            Texture
        }

        /// <summary>
        ///     Encapsulates the result from a dropdown update attempt.
        /// </summary>
        private class UpdateResult
        {
            public string ErrorMessage { get; private set; }
            public bool HasErrors { get; private set; }

            public UpdateResult(string errorMessage, bool hasErrors)
            {
                ErrorMessage = errorMessage;
                HasErrors = hasErrors;
            }
        }
    }
}
