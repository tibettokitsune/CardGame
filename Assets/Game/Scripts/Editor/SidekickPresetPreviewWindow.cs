#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Synty.SidekickCharacters.API;
using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Database.DTO;
using Synty.SidekickCharacters.Enums;
using Synty.SidekickCharacters.Utils;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor.Sidekick
{
    public class SidekickPresetPreviewWindow : EditorWindow
    {
        private const string WindowTitle = "Sidekick Presets";
        private static readonly Color BackgroundColor = new Color(0.18f, 0.18f, 0.18f);
        private static readonly Vector3 PreviewPivot = new Vector3(0f, 1.2f, 0f);

        private enum PreviewTab
        {
            Equipment,
            Body
        }

        private enum BodyCategory
        {
            BodyShapes,
            ColorPresets
        }

        private sealed class EquipmentCategoryDefinition
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public PartGroup Group { get; set; }
            public CharacterPartType[] RequiredTypes { get; set; } = Array.Empty<CharacterPartType>();
        }

        private sealed class EquipmentCategoryData
        {
            public EquipmentCategoryDefinition Definition { get; }
            public List<SidekickPartPreset> Presets { get; }

            public EquipmentCategoryData(EquipmentCategoryDefinition definition, List<SidekickPartPreset> presets)
            {
                Definition = definition;
                Presets = presets;
            }
        }

        private static readonly EquipmentCategoryDefinition[] EquipmentCategoryDefinitions =
        {
            new EquipmentCategoryDefinition
            {
                Id = "helmets",
                Label = "Helmets",
                Group = PartGroup.Head,
                RequiredTypes = new[]
                {
                    CharacterPartType.Head,
                    CharacterPartType.Hair,
                    CharacterPartType.AttachmentHead,
                    CharacterPartType.AttachmentFace
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "masks",
                Label = "Masks & Face",
                Group = PartGroup.Head,
                RequiredTypes = new[]
                {
                    CharacterPartType.AttachmentFace,
                    CharacterPartType.Nose,
                    CharacterPartType.EarLeft,
                    CharacterPartType.EarRight
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "torso",
                Label = "Torso",
                Group = PartGroup.UpperBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.Torso
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "shoulders",
                Label = "Shoulders",
                Group = PartGroup.UpperBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.AttachmentShoulderLeft,
                    CharacterPartType.AttachmentShoulderRight
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "arms",
                Label = "Arms & Hands",
                Group = PartGroup.UpperBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.ArmUpperLeft,
                    CharacterPartType.ArmUpperRight,
                    CharacterPartType.ArmLowerLeft,
                    CharacterPartType.ArmLowerRight,
                    CharacterPartType.HandLeft,
                    CharacterPartType.HandRight
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "back",
                Label = "Back Attachments",
                Group = PartGroup.UpperBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.AttachmentBack
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "belt",
                Label = "Belts & Hips",
                Group = PartGroup.LowerBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.AttachmentHipsFront,
                    CharacterPartType.AttachmentHipsBack,
                    CharacterPartType.AttachmentHipsLeft,
                    CharacterPartType.AttachmentHipsRight,
                    CharacterPartType.Hips
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "legs",
                Label = "Legs",
                Group = PartGroup.LowerBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.LegLeft,
                    CharacterPartType.LegRight
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "knees",
                Label = "Kneepads",
                Group = PartGroup.LowerBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.AttachmentKneeLeft,
                    CharacterPartType.AttachmentKneeRight
                }
            },
            new EquipmentCategoryDefinition
            {
                Id = "feet",
                Label = "Boots & Feet",
                Group = PartGroup.LowerBody,
                RequiredTypes = new[]
                {
                    CharacterPartType.FootLeft,
                    CharacterPartType.FootRight
                }
            }
        };

        private PreviewRenderUtility _previewUtility;
        private GameObject _previewInstance;
        private SidekickRuntime _runtime;
        private DatabaseManager _databaseManager;

        private Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> _partLibrary;
        private readonly Dictionary<PartGroup, List<SidekickPartPreset>> _presetsByGroup = new();
        private readonly Dictionary<int, List<SidekickPartPresetRow>> _presetRowsCache = new();
        private readonly Dictionary<PartGroup, SidekickPartPreset> _activePresets = new();

        private readonly List<EquipmentCategoryData> _equipmentCategories = new();
        private readonly Dictionary<string, SidekickPartPreset> _activeCategorySelections = new();
        private string[] _equipmentCategoryLabels = Array.Empty<string>();
        private int _selectedEquipmentCategoryIndex;
        private Vector2 _equipmentScroll;

        private readonly Dictionary<int, List<SidekickColorPresetRow>> _colorPresetRowsCache = new();
        private List<SidekickBodyShapePreset> _bodyShapes = new();
        private List<SidekickColorPreset> _colorPresets = new();
        private SidekickBodyShapePreset _activeBodyShape;
        private SidekickColorPreset _activeColorPreset;
        private readonly string[] _bodyCategoryLabels = { "Body Shapes", "Color Presets" };
        private int _selectedBodyCategoryIndex;
        private Vector2 _bodyScroll;

        private PreviewTab _activeTab = PreviewTab.Equipment;
        private Vector2 _previewAngles = new(135f, -10f);
        private float _previewZoom = 4.5f;

        private bool _isInitialized;
        private bool _initializationFailed;

        private GUIStyle _presetButtonStyle;
        private GUIStyle _presetButtonSelectedStyle;

        [MenuItem("Tools/Sidekick/Preset Preview")]
        public static void ShowWindow()
        {
            var window = GetWindow<SidekickPresetPreviewWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private async void OnEnable()
        {
            InitializeStyles();
            CreatePreviewUtility();
            await InitializeAsync();
            Repaint();
        }

        private void OnDisable()
        {
            DestroyPreviewInstance();
            if (_previewUtility != null)
            {
                _previewUtility.Cleanup();
                _previewUtility = null;
            }

            _runtime = null;

            _databaseManager?.CloseConnection();
            _databaseManager = null;

            _presetsByGroup.Clear();
            _presetRowsCache.Clear();
            _activePresets.Clear();
            _equipmentCategories.Clear();
            _activeCategorySelections.Clear();
            _equipmentCategoryLabels = Array.Empty<string>();
            _selectedEquipmentCategoryIndex = 0;
            _equipmentScroll = Vector2.zero;
            _bodyShapes.Clear();
            _colorPresets.Clear();
            _colorPresetRowsCache.Clear();
            _activeBodyShape = null;
            _activeColorPreset = null;
            _selectedBodyCategoryIndex = 0;
            _bodyScroll = Vector2.zero;
            _activeTab = PreviewTab.Equipment;
            _isInitialized = false;
            _initializationFailed = false;
        }

        private void CreatePreviewUtility()
        {
            _previewUtility = new PreviewRenderUtility(true);
            _previewUtility.camera.fieldOfView = 30f;
            _previewUtility.camera.nearClipPlane = 0.1f;
            _previewUtility.camera.farClipPlane = 100f;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = BackgroundColor;

            var primaryLight = _previewUtility.lights[0];
            primaryLight.intensity = 1.4f;
            primaryLight.transform.rotation = Quaternion.Euler(40f, 40f, 0f);

            var secondaryLight = _previewUtility.lights[1];
            secondaryLight.intensity = 0.8f;
            secondaryLight.transform.rotation = Quaternion.Euler(340f, 210f, 0f);
        }

        private void InitializeStyles()
        {
            _presetButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24f
            };

            _presetButtonSelectedStyle = new GUIStyle(_presetButtonStyle)
            {
                normal =
                {
                    background = _presetButtonStyle.onNormal.background,
                    textColor = EditorStyles.miniButton.onNormal.textColor
                },
                fontStyle = FontStyle.Bold
            };
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized || _initializationFailed)
                return;

            try
            {
                _databaseManager = new DatabaseManager();
                if (_databaseManager.GetCurrentDbConnection() == null)
                {
                    await Task.Run(() => _databaseManager.GetDbConnection(true));
                }

                var baseModel = Resources.Load<GameObject>("Meshes/SK_BaseModel");
                var baseMaterial = Resources.Load<Material>("Materials/M_BaseMaterial");

                if (baseModel == null || baseMaterial == null)
                {
                    Debug.LogError("SidekickPresetPreviewWindow: Failed to load required Sidekick base resources.");
                    _initializationFailed = true;
                    return;
                }

                _runtime = new SidekickRuntime(baseModel, baseMaterial, null, _databaseManager);
                await SidekickRuntime.PopulateToolData(_runtime);

                _partLibrary = _runtime.MappedPartDictionary;
                BuildPresetCatalog();
                LoadBodyCustomizationData();

                if (_equipmentCategories.Count == 0 && _bodyShapes.Count == 0 && _colorPresets.Count == 0)
                {
                    Debug.LogWarning("SidekickPresetPreviewWindow: No presets were found in the Sidekick database.");
                    _initializationFailed = true;
                    return;
                }

                _isInitialized = true;
                RebuildCharacter();
            }
            catch (Exception ex)
            {
                Debug.LogError($"SidekickPresetPreviewWindow: Initialization failed.\n{ex}");
                _initializationFailed = true;
            }
        }

        private void BuildPresetCatalog()
        {
            _presetsByGroup.Clear();
            _activePresets.Clear();

            foreach (PartGroup group in Enum.GetValues(typeof(PartGroup)))
            {
                var presets = SidekickPartPreset.GetAllByGroup(_databaseManager, group)
                    .OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (presets.Count == 0)
                    continue;

                _presetsByGroup[group] = presets;
                if (!_activePresets.TryGetValue(group, out var existing) ||
                    presets.All(preset => preset.ID != existing.ID))
                {
                    _activePresets[group] = presets.First();
                }
            }

            BuildEquipmentCategories();
        }

        private void BuildEquipmentCategories()
        {
            _equipmentCategories.Clear();
            _equipmentCategoryLabels = Array.Empty<string>();

            foreach (var category in EquipmentCategoryDefinitions)
            {
                if (!_presetsByGroup.TryGetValue(category.Group, out var presets))
                    continue;

                var filtered = FilterPresetsByCategory(presets, category);
                if (filtered.Count == 0)
                    continue;

                var data = new EquipmentCategoryData(category, filtered);
                _equipmentCategories.Add(data);

                if (_activeCategorySelections.TryGetValue(category.Id, out var existingSelection) &&
                    filtered.Any(preset => preset.ID == existingSelection.ID))
                {
                    // keep current selection
                }
                else if (_activePresets.TryGetValue(category.Group, out var groupSelection) &&
                         filtered.Any(preset => preset.ID == groupSelection.ID))
                {
                    _activeCategorySelections[category.Id] = filtered.First(preset => preset.ID == groupSelection.ID);
                }
                else
                {
                    var firstPreset = filtered.First();
                    _activeCategorySelections[category.Id] = firstPreset;
                    _activePresets[category.Group] = firstPreset;
                }
            }

            if (_equipmentCategories.Count > 0)
            {
                _equipmentCategoryLabels = _equipmentCategories
                    .Select(data => data.Definition.Label)
                    .ToArray();
                _selectedEquipmentCategoryIndex = Mathf.Clamp(_selectedEquipmentCategoryIndex, 0,
                    _equipmentCategoryLabels.Length - 1);

                var validIds = new HashSet<string>(_equipmentCategories.Select(data => data.Definition.Id));
                foreach (var key in _activeCategorySelections.Keys.Where(id => !validIds.Contains(id)).ToList())
                {
                    _activeCategorySelections.Remove(key);
                }
            }
            else
            {
                _selectedEquipmentCategoryIndex = 0;
            }
        }

        private void LoadBodyCustomizationData()
        {
            _bodyShapes = SidekickBodyShapePreset.GetAll(_databaseManager) ?? new List<SidekickBodyShapePreset>();
            _bodyShapes = _bodyShapes
                .OrderBy(shape => shape.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (_bodyShapes.Count > 0)
            {
                if (_activeBodyShape == null ||
                    _bodyShapes.All(shape => shape.ID != _activeBodyShape.ID))
                {
                    _activeBodyShape = _bodyShapes.First();
                }
            }
            else
            {
                _activeBodyShape = null;
            }

            _colorPresets = SidekickColorPreset.GetAllByColorGroup(_databaseManager, ColorGroup.Outfits) ??
                            new List<SidekickColorPreset>();
            _colorPresets = _colorPresets
                .OrderBy(preset => preset.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (_colorPresets.Count > 0)
            {
                if (_activeColorPreset == null ||
                    _colorPresets.All(preset => preset.ID != _activeColorPreset.ID))
                {
                    _activeColorPreset = _colorPresets.First();
                }
            }
            else
            {
                _activeColorPreset = null;
            }
        }

        private List<SidekickPartPreset> FilterPresetsByCategory(
            List<SidekickPartPreset> presets,
            EquipmentCategoryDefinition category)
        {
            if (category.RequiredTypes == null || category.RequiredTypes.Length == 0)
                return new List<SidekickPartPreset>(presets);

            var requiredTypes = new HashSet<CharacterPartType>(category.RequiredTypes);
            var result = new List<SidekickPartPreset>();

            foreach (var preset in presets)
            {
                var rows = GetRowsForPreset(preset);
                if (rows.Any(row =>
                        TryResolveCharacterPartType(row.PartType, out var partType) &&
                        requiredTypes.Contains(partType)))
                {
                    result.Add(preset);
                }
            }

            return result;
        }

        private static bool TryResolveCharacterPartType(string shortcode, out CharacterPartType partType)
        {
            try
            {
                string typeName = CharacterPartTypeUtils.GetTypeNameFromShortcode(shortcode);
                return Enum.TryParse(typeName, out partType);
            }
            catch
            {
                partType = default;
                return false;
            }
        }

        private void OnGUI()
        {
            if (_previewUtility == null)
            {
                EditorGUILayout.HelpBox("Preview utility not ready.", MessageType.Warning);
                return;
            }

            if (!_isInitialized)
            {
                DrawInitializationState();
                return;
            }

            DrawMainTabToolbar();
            GUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawActiveTabPanel();
                DrawPreview();
            }
        }

        private void DrawInitializationState()
        {
            if (_initializationFailed)
            {
                EditorGUILayout.HelpBox("Failed to initialize Sidekick preset preview. See the console for details.", MessageType.Error);
                if (GUILayout.Button("Retry"))
                {
                    _initializationFailed = false;
                    _ = InitializeAsync();
                }
                return;
            }

            EditorGUILayout.HelpBox("Loading Sidekick data...", MessageType.Info);
        }

        private void DrawMainTabToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                int selected = GUILayout.Toolbar((int)_activeTab, new[] { "Equipment", "Body" }, EditorStyles.toolbarButton);
                if (selected != (int)_activeTab)
                {
                    _activeTab = (PreviewTab)selected;
                }
            }
        }

        private void DrawActiveTabPanel()
        {
            switch (_activeTab)
            {
                case PreviewTab.Equipment:
                    DrawEquipmentPanel();
                    break;
                case PreviewTab.Body:
                    DrawBodyPanel();
                    break;
                default:
                    DrawEquipmentPanel();
                    break;
            }
        }

        private void DrawEquipmentPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
            {
                if (_equipmentCategories.Count == 0)
                {
                    EditorGUILayout.HelpBox("No equipment presets available.", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    _selectedEquipmentCategoryIndex = GUILayout.Toolbar(
                        _selectedEquipmentCategoryIndex,
                        _equipmentCategoryLabels,
                        EditorStyles.toolbarButton);
                }

                var categoryData = _equipmentCategories[Mathf.Clamp(_selectedEquipmentCategoryIndex, 0,
                    _equipmentCategories.Count - 1)];
                string heading = $"{categoryData.Definition.Label} Presets";
                EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);

                using (var scrollScope =
                       new EditorGUILayout.ScrollViewScope(_equipmentScroll, GUILayout.ExpandHeight(true)))
                {
                    _equipmentScroll = scrollScope.scrollPosition;

                    foreach (var preset in categoryData.Presets)
                    {
                        bool isActive = _activeCategorySelections.TryGetValue(categoryData.Definition.Id,
                                            out var activePreset) &&
                                        activePreset.ID == preset.ID;

                        string label = $"{preset.ID}: {preset.Name}";
                        var style = isActive ? _presetButtonSelectedStyle : _presetButtonStyle;

                        if (GUILayout.Button(label, style))
                        {
                            if (!isActive)
                            {
                                ApplyEquipmentSelection(categoryData, preset);
                            }
                        }
                    }
                }
            }
        }

        private void DrawBodyPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    _selectedBodyCategoryIndex = GUILayout.Toolbar(
                        _selectedBodyCategoryIndex,
                        _bodyCategoryLabels,
                        EditorStyles.toolbarButton);
                }

                using (var scrollScope =
                       new EditorGUILayout.ScrollViewScope(_bodyScroll, GUILayout.ExpandHeight(true)))
                {
                    _bodyScroll = scrollScope.scrollPosition;
                    switch ((BodyCategory)_selectedBodyCategoryIndex)
                    {
                        case BodyCategory.BodyShapes:
                            DrawBodyShapeList();
                            break;
                        case BodyCategory.ColorPresets:
                            DrawColorPresetList();
                            break;
                        default:
                            DrawBodyShapeList();
                            break;
                    }
                }
            }
        }

        private void DrawBodyShapeList()
        {
            if (_bodyShapes == null || _bodyShapes.Count == 0)
            {
                EditorGUILayout.HelpBox("No body shape presets found.", MessageType.Info);
                return;
            }

            foreach (var shape in _bodyShapes)
            {
                bool isActive = _activeBodyShape != null && _activeBodyShape.ID == shape.ID;
                string displayName = string.IsNullOrWhiteSpace(shape.Name)
                    ? $"Body Shape {shape.ID}"
                    : $"{shape.ID}: {shape.Name}";
                var style = isActive ? _presetButtonSelectedStyle : _presetButtonStyle;

                if (GUILayout.Button(displayName, style))
                {
                    if (!isActive)
                    {
                        _activeBodyShape = shape;
                        RebuildCharacter();
                    }
                }
            }
        }

        private void DrawColorPresetList()
        {
            if (_colorPresets == null || _colorPresets.Count == 0)
            {
                EditorGUILayout.HelpBox("No color presets found.", MessageType.Info);
                return;
            }

            foreach (var preset in _colorPresets)
            {
                bool isActive = _activeColorPreset != null && _activeColorPreset.ID == preset.ID;
                string displayName = string.IsNullOrWhiteSpace(preset.Name)
                    ? $"Color Preset {preset.ID}"
                    : $"{preset.ID}: {preset.Name}";
                var style = isActive ? _presetButtonSelectedStyle : _presetButtonStyle;

                if (GUILayout.Button(displayName, style))
                {
                    if (!isActive)
                    {
                        _activeColorPreset = preset;
                        RebuildCharacter();
                    }
                }
            }
        }

        private void ApplyEquipmentSelection(EquipmentCategoryData categoryData, SidekickPartPreset preset)
        {
            _activeCategorySelections[categoryData.Definition.Id] = preset;
            _activePresets[categoryData.Definition.Group] = preset;

            foreach (var data in _equipmentCategories.Where(data => data.Definition.Group == categoryData.Definition.Group))
            {
                _activeCategorySelections[data.Definition.Id] = preset;
            }

            RebuildCharacter();
        }

        private void DrawPreview()
        {
            GUILayout.Space(8f);
            Rect previewRect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                if (_previewInstance != null)
                {
                    RenderPreview(previewRect);
                }
                else
                {
                    EditorGUI.DrawRect(previewRect, BackgroundColor);
                    GUI.Label(previewRect, "No preview available", EditorStyles.centeredGreyMiniLabel);
                }
            }
            HandlePreviewInput(previewRect);
        }

        private void RenderPreview(Rect rect)
        {
            UpdateCameraTransform();
            _previewUtility.BeginPreview(rect, GUIStyle.none);
            _previewUtility.Render();
            Texture previewTexture = _previewUtility.EndPreview();
            GUI.DrawTexture(rect, previewTexture, ScaleMode.StretchToFill, false);
        }

        private void HandlePreviewInput(Rect rect)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return;

            switch (evt.type)
            {
                case EventType.MouseDrag when evt.button == 0:
                    _previewAngles += new Vector2(evt.delta.x, -evt.delta.y);
                    _previewAngles.y = Mathf.Clamp(_previewAngles.y, -80f, 80f);
                    evt.Use();
                    Repaint();
                    break;
                case EventType.ScrollWheel:
                    _previewZoom = Mathf.Clamp(_previewZoom + evt.delta.y * 0.1f, 2.5f, 7.5f);
                    evt.Use();
                    Repaint();
                    break;
            }
        }

        private void UpdateCameraTransform()
        {
            Quaternion rotation = Quaternion.Euler(_previewAngles.y, _previewAngles.x, 0f);
            Vector3 direction = rotation * Vector3.forward;
            Vector3 position = PreviewPivot - direction * _previewZoom;

            _previewUtility.camera.transform.position = position;
            _previewUtility.camera.transform.rotation = Quaternion.LookRotation(PreviewPivot - position, Vector3.up);
        }

        private void RebuildCharacter()
        {
            if (_runtime == null || _partLibrary == null)
                return;

            var renderers = new List<SkinnedMeshRenderer>();

            foreach (var preset in _activePresets.Values.Where(p => p != null))
            {
                foreach (var row in GetRowsForPreset(preset))
                {
                    if (string.IsNullOrEmpty(row.PartName))
                        continue;

                    if (!TryResolveCharacterPartType(row.PartType, out var partType))
                        continue;

                    if (!_partLibrary.TryGetValue(partType, out var partsByName))
                        continue;
                    if (!partsByName.TryGetValue(row.PartName, out var part))
                        continue;

                    var model = part.GetPartModel();
                    var mesh = model != null ? model.GetComponentInChildren<SkinnedMeshRenderer>() : null;
                    if (mesh != null)
                    {
                        renderers.Add(mesh);
                    }
                }
            }

            if (renderers.Count == 0)
            {
                DestroyPreviewInstance();
                return;
            }

            DestroyPreviewInstance();
            ApplyBodyShape();
            ApplyColorPreset();

            var character = _runtime.CreateCharacter("Sidekick Preview", renderers, false, true);
            if (character == null)
                return;

            PreparePreviewInstance(character);
            _previewUtility.AddSingleGO(_previewInstance);
            Repaint();
        }

        private void ApplyBodyShape()
        {
            if (_activeBodyShape == null)
                return;

            _runtime.BodyTypeBlendValue = _activeBodyShape.BodyType;
            _runtime.MusclesBlendValue = _activeBodyShape.Musculature;
            _runtime.BodySizeHeavyBlendValue = _activeBodyShape.BodySize > 0 ? _activeBodyShape.BodySize : 0f;
            _runtime.BodySizeSkinnyBlendValue = _activeBodyShape.BodySize < 0 ? -_activeBodyShape.BodySize : 0f;
        }

        private void ApplyColorPreset()
        {
            if (_activeColorPreset == null)
                return;

            if (!_colorPresetRowsCache.TryGetValue(_activeColorPreset.ID, out var rows))
            {
                rows = SidekickColorPresetRow.GetAllByPreset(_databaseManager, _activeColorPreset);
                _colorPresetRowsCache[_activeColorPreset.ID] = rows;
            }

            foreach (var row in rows)
            {
                var colorRow = SidekickColorRow.CreateFromPresetColorRow(row);
                foreach (ColorType property in Enum.GetValues(typeof(ColorType)))
                {
                    _runtime.UpdateColor(property, colorRow);
                }
            }
        }

        private List<SidekickPartPresetRow> GetRowsForPreset(SidekickPartPreset preset)
        {
            if (!_presetRowsCache.TryGetValue(preset.ID, out var rows))
            {
                rows = SidekickPartPresetRow.GetAllByPreset(_databaseManager, preset);
                _presetRowsCache[preset.ID] = rows;
            }

            return rows;
        }

        private void PreparePreviewInstance(GameObject character)
        {
            _previewInstance = character;
            _previewInstance.hideFlags = HideFlags.HideAndDontSave;
            _previewInstance.transform.position = Vector3.zero;
            _previewInstance.transform.rotation = Quaternion.identity;

            foreach (var component in _previewInstance.GetComponentsInChildren<Component>())
            {
                if (component is Transform)
                    continue;

                component.hideFlags = HideFlags.HideAndDontSave;
            }

            var animator = _previewInstance.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.enabled = false;
            }
        }

        private void DestroyPreviewInstance()
        {
            if (_previewInstance == null)
                return;

            DestroyImmediate(_previewInstance);
            _previewInstance = null;
        }
    }
}
#endif
