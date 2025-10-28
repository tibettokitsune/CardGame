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
        private const string WindowTitle = "Sidekick Preset Preview";
        private static readonly Color PreviewBackgroundColor = new(0.18f, 0.18f, 0.18f);
        private static readonly Vector3 PreviewPivot = new(0f, 1.2f, 0f);
        private static readonly PartGroup[] OrderedPartGroups = { PartGroup.Head, PartGroup.UpperBody, PartGroup.LowerBody };
        private static readonly CharacterPartType[] BodyConfigurationTypes = PartGroup.Head
            .GetPartTypes()
            .Concat(PartGroup.UpperBody.GetPartTypes())
            .Concat(PartGroup.LowerBody.GetPartTypes())
            .Where(type =>
                type != CharacterPartType.AttachmentHead &&
                type != CharacterPartType.AttachmentFace &&
                type != CharacterPartType.AttachmentBack &&
                !type.ToString().StartsWith("Attachment", StringComparison.Ordinal))
            .Distinct()
            .ToArray();

        private enum PreviewTab
        {
            Equipment,
            Body
        }

        private const float DefaultPrimaryYaw = 40f;
        private const float DefaultPrimaryPitch = 40f;
        private const float DefaultPrimaryIntensity = 1.4f;
        private const float DefaultSecondaryYaw = 210f;
        private const float DefaultSecondaryPitch = -20f;
        private const float DefaultSecondaryIntensity = 0.8f;

        private sealed class PartRecord
        {
            public string Key;
            public SidekickPart Part;
        }

        private PreviewRenderUtility _previewUtility;
        private GameObject _previewInstance;
        private SidekickRuntime _runtime;
        private DatabaseManager _databaseManager;

        private Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> _runtimeLibrary;
        private readonly Dictionary<CharacterPartType, List<PartRecord>> _partsByType = new();
        private readonly Dictionary<CharacterPartType, PartRecord> _baseSelections = new();
        private readonly Dictionary<CharacterPartType, PartRecord> _equipmentSelections = new();

        private Vector2 _equipmentScroll;
        private Vector2 _bodyScroll;
        private Vector2 _previewAngles = new(135f, -10f);
        private float _previewZoom = 4.5f;

        private float _primaryLightYaw = DefaultPrimaryYaw;
        private float _primaryLightPitch = DefaultPrimaryPitch;
        private float _primaryLightIntensity = DefaultPrimaryIntensity;
        private float _secondaryLightYaw = DefaultSecondaryYaw;
        private float _secondaryLightPitch = DefaultSecondaryPitch;
        private float _secondaryLightIntensity = DefaultSecondaryIntensity;

        private PreviewTab _activeTab = PreviewTab.Equipment;
        private PartGroup _selectedEquipmentGroup = PartGroup.Head;
        private CharacterPartType _selectedEquipmentPartType = CharacterPartType.Head;
        private string _equipmentSearch = string.Empty;
        private bool _includeBasePartsInEquipment;

        private List<SidekickSpecies> _species;
        private SidekickSpecies _activeSpecies;
        private SidekickSpecies _unrestrictedSpecies;

        private readonly Dictionary<int, List<SidekickColorPresetRow>> _colorPresetRowsCache = new();
        private List<SidekickBodyShapePreset> _bodyShapes = new();
        private List<SidekickColorPreset> _colorPresets = new();
        private SidekickBodyShapePreset _activeBodyShape;
        private SidekickColorPreset _activeColorPreset;

        private bool _isInitialized;
        private bool _initializationFailed;

        private GUIStyle _listButtonStyle;
        private GUIStyle _listButtonSelectedStyle;

        private string[] _groupLabels;

        [MenuItem("Tools/Sidekick/Preset Preview")]
        public static void ShowWindow()
        {
            var window = GetWindow<SidekickPresetPreviewWindow>(true, WindowTitle);
            window.minSize = new Vector2(720f, 420f);
            window.Show();
        }

        private async void OnEnable()
        {
            SetupPreviewUtility();
            _groupLabels ??= OrderedPartGroups
                .Select(group => ObjectNames.NicifyVariableName(group.ToString()))
                .ToArray();
            await InitializeAsync();
            Repaint();
        }

        private void OnDisable()
        {
            DestroyPreviewInstance();
            _previewUtility?.Cleanup();
            _previewUtility = null;

            _runtime = null;
            _runtimeLibrary = null;

            _databaseManager?.CloseConnection();
            _databaseManager = null;

            _partsByType.Clear();
            _baseSelections.Clear();
            _equipmentSelections.Clear();
            _species = null;
            _activeSpecies = null;
            _unrestrictedSpecies = null;
            _bodyShapes.Clear();
            _colorPresets.Clear();
            _colorPresetRowsCache.Clear();

            _isInitialized = false;
            _initializationFailed = false;
        }

        private void InitializeStyles()
        {
            _listButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 22f,
                richText = false
            };

            _listButtonSelectedStyle = new GUIStyle(_listButtonStyle)
            {
                normal =
                {
                    background = _listButtonStyle.onNormal.background,
                    textColor = EditorStyles.miniButton.onNormal.textColor
                },
                fontStyle = FontStyle.Bold
            };
        }

        private void SetupPreviewUtility()
        {
            _previewUtility = new PreviewRenderUtility(true);
            _previewUtility.camera.fieldOfView = 30f;
            _previewUtility.camera.nearClipPlane = 0.1f;
            _previewUtility.camera.farClipPlane = 100f;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = PreviewBackgroundColor;
            ApplyLightSettings();
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
                    Debug.LogError("SidekickPresetPreviewWindow: Failed to load base Sidekick resources.");
                    _initializationFailed = true;
                    return;
                }

                _runtime = new SidekickRuntime(baseModel, baseMaterial, null, _databaseManager);
                await SidekickRuntime.PopulateToolData(_runtime);
                _runtimeLibrary = _runtime.MappedPartDictionary;

                _species = SidekickSpecies.GetAll(_databaseManager);
                _unrestrictedSpecies = _species.FirstOrDefault(s =>
                    string.Equals(s.Name, "Unrestricted", StringComparison.OrdinalIgnoreCase));
                _activeSpecies = _species.FirstOrDefault(s =>
                    string.Equals(s.Name, "Humans", StringComparison.OrdinalIgnoreCase)) ?? _species.FirstOrDefault();

                BuildPartCollections();
                LoadBodyCustomizationData();
                EnsureDefaultSelections();

                _isInitialized = true;
                RebuildCharacter();
            }
            catch (Exception ex)
            {
                Debug.LogError($"SidekickPresetPreviewWindow: Initialization failed.\n{ex}");
                _initializationFailed = true;
            }
        }

        private void OnGUI()
        {
            if (_listButtonStyle == null || _listButtonSelectedStyle == null)
            {
                InitializeStyles();
            }
            if (_groupLabels == null || _groupLabels.Length != OrderedPartGroups.Length)
            {
                _groupLabels = OrderedPartGroups
                    .Select(group => ObjectNames.NicifyVariableName(group.ToString()))
                    .ToArray();
            }

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

            DrawSpeciesSelector();
            DrawMainTabToolbar();

            GUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawActivePanel();
                DrawPreview();
            }
        }

        private void DrawInitializationState()
        {
            if (_initializationFailed)
            {
                EditorGUILayout.HelpBox("Failed to initialize Sidekick preview. See the console for details.", MessageType.Error);
                if (GUILayout.Button("Retry"))
                {
                    _initializationFailed = false;
                    _ = InitializeAsync();
                }
                return;
            }

            EditorGUILayout.HelpBox("Loading Sidekick data...", MessageType.Info);
        }

        private void DrawSpeciesSelector()
        {
            if (_species == null || _species.Count == 0)
                return;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Species", GUILayout.Width(60f));
            var speciesNames = _species.Select(s => s.Name).ToArray();
            var currentIndex = Mathf.Max(0, _species.IndexOf(_activeSpecies));
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup(currentIndex, speciesNames);
            if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < _species.Count)
            {
                SetActiveSpecies(_species[newIndex]);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        private void DrawMainTabToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                int selected = GUILayout.Toolbar((int)_activeTab, new[] { "Equipment", "Body" }, EditorStyles.toolbarButton);
                if (selected != (int)_activeTab)
                {
                    _activeTab = (PreviewTab)selected;
                    RebuildCharacter();
                }
            }
        }

        private void DrawActivePanel()
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
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(320f)))
            {
                int newGroupIndex = GUILayout.Toolbar(Array.IndexOf(OrderedPartGroups, _selectedEquipmentGroup), _groupLabels);
                if (newGroupIndex >= 0 && OrderedPartGroups[newGroupIndex] != _selectedEquipmentGroup)
                {
                    _selectedEquipmentGroup = OrderedPartGroups[newGroupIndex];
                    EnsureValidPartTypeSelection();
                    RebuildCharacter();
                }

                GUILayout.Space(4f);

                var partTypes = _selectedEquipmentGroup.GetPartTypes();
                var partTypeNames = partTypes
                    .Select(type => ObjectNames.NicifyVariableName(type.ToString()))
                    .ToArray();
                int currentTypeIndex = Mathf.Max(0, partTypes.IndexOf(_selectedEquipmentPartType));
                EditorGUI.BeginChangeCheck();
                int newTypeIndex = EditorGUILayout.Popup("Part Type", currentTypeIndex, partTypeNames);
                if (EditorGUI.EndChangeCheck())
                {
                    _selectedEquipmentPartType = partTypes[Mathf.Clamp(newTypeIndex, 0, partTypes.Count - 1)];
                    RebuildCharacter();
                }

                _equipmentSearch = EditorGUILayout.TextField("Search", _equipmentSearch);
                _includeBasePartsInEquipment = EditorGUILayout.Toggle("Include Base Parts", _includeBasePartsInEquipment);

                DrawSelectedEquipmentInfo();

                using (var scroll = new EditorGUILayout.ScrollViewScope(_equipmentScroll, GUILayout.ExpandHeight(true)))
                {
                    _equipmentScroll = scroll.scrollPosition;

                    foreach (var record in GetEquipmentCandidates(_selectedEquipmentPartType))
                    {
                        DrawEquipmentRecord(record);
                    }
                }

                if (_equipmentSelections.TryGetValue(_selectedEquipmentPartType, out var selected) && selected != null)
                {
                    if (GUILayout.Button("Clear Selection"))
                    {
                        _equipmentSelections.Remove(_selectedEquipmentPartType);
                        RebuildCharacter();
                    }
                }
            }
        }

        private void DrawEquipmentRecord(PartRecord record)
        {
            if (record?.Part == null)
                return;

            bool isSelected = _equipmentSelections.TryGetValue(record.Part.Type, out var current) &&
                              current != null &&
                              current.Part.ID == record.Part.ID;

            var style = isSelected ? _listButtonSelectedStyle : _listButtonStyle;
            string label = FormatPartLabel(record);
            var tooltip = record.Part.FileName ?? record.Key ?? label;

            if (GUILayout.Button(new GUIContent(label, tooltip), style))
            {
                _equipmentSelections[record.Part.Type] = record;
                RebuildCharacter();
            }
        }

        private void DrawSelectedEquipmentInfo()
        {
            if (_equipmentSelections.TryGetValue(_selectedEquipmentPartType, out var record) && record != null)
            {
                string info = $"Selected: [{record.Part.ID}] {GetDisplayName(record)}";
                EditorGUILayout.HelpBox(info, MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("No equipment selected for this part type.", MessageType.Info);
            }
        }

        private void DrawBodyPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(340f)))
            {
                DrawBodyShapeControls();
                DrawColorPresetControls();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Base Body Configuration", EditorStyles.boldLabel);

                using (var scroll = new EditorGUILayout.ScrollViewScope(_bodyScroll, GUILayout.ExpandHeight(true)))
                {
                    _bodyScroll = scroll.scrollPosition;

                    foreach (var group in OrderedPartGroups)
                    {
                        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(group.ToString()), EditorStyles.miniBoldLabel);
                        EditorGUI.indentLevel++;
                        foreach (var partType in BodyConfigurationTypes.Where(t => group.GetPartTypes().Contains(t)))
                        {
                            DrawBasePartSelector(partType);
                        }
                        EditorGUI.indentLevel--;
                        GUILayout.Space(4f);
                    }
                }
            }
        }

        private void DrawBodyShapeControls()
        {
            if (_bodyShapes == null || _bodyShapes.Count == 0)
                return;

            var labels = _bodyShapes
                .Select(shape => string.IsNullOrWhiteSpace(shape.Name)
                    ? $"Body Shape #{shape.ID}"
                    : $"{shape.Name} (#{shape.ID})")
                .ToArray();
            int currentIndex = Mathf.Max(0, _bodyShapes.FindIndex(shape => _activeBodyShape != null && shape.ID == _activeBodyShape.ID));
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Body Shape", currentIndex, labels);
            if (EditorGUI.EndChangeCheck())
            {
                _activeBodyShape = _bodyShapes[Mathf.Clamp(newIndex, 0, _bodyShapes.Count - 1)];
                RebuildCharacter();
            }
        }

        private void DrawColorPresetControls()
        {
            if (_colorPresets == null || _colorPresets.Count == 0)
                return;

            var labels = _colorPresets
                .Select(preset => string.IsNullOrWhiteSpace(preset.Name)
                    ? $"Color Preset #{preset.ID}"
                    : $"{preset.Name} (#{preset.ID})")
                .ToArray();
            int currentIndex = Mathf.Max(0, _colorPresets.FindIndex(preset => _activeColorPreset != null && preset.ID == _activeColorPreset.ID));
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Color Preset", currentIndex, labels);
            if (EditorGUI.EndChangeCheck())
            {
                _activeColorPreset = _colorPresets[Mathf.Clamp(newIndex, 0, _colorPresets.Count - 1)];
                RebuildCharacter();
            }
        }

        private void DrawBasePartSelector(CharacterPartType partType)
        {
            var candidates = GetBaseCandidates(partType).ToList();

            var displayOptions = new List<string> { "None" };
            displayOptions.AddRange(candidates.Select(FormatPartLabel));

            _baseSelections.TryGetValue(partType, out var currentRecord);
            int currentIndex = 0;
            if (currentRecord != null)
            {
                int foundIndex = candidates.FindIndex(record => record.Part.ID == currentRecord.Part.ID);
                if (foundIndex >= 0)
                {
                    currentIndex = foundIndex + 1;
                }
            }

            string label = ObjectNames.NicifyVariableName(partType.ToString());
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(label, currentIndex, displayOptions.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex <= 0)
                {
                    _baseSelections.Remove(partType);
                }
                else
                {
                    _baseSelections[partType] = candidates[newIndex - 1];
                }
                RebuildCharacter();
            }
        }

        private void DrawPreview()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
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
                        EditorGUI.DrawRect(previewRect, PreviewBackgroundColor);
                        GUI.Label(previewRect, "No preview available", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                HandlePreviewInput(previewRect);
                GUILayout.Space(6f);
                DrawLightingControls();
            }
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
            if (_runtime == null || _runtimeLibrary == null)
                return;

            var selectedPartNames = new Dictionary<CharacterPartType, string>();

            foreach (var kvp in _baseSelections)
            {
                if (kvp.Value != null)
                {
                    selectedPartNames[kvp.Key] = kvp.Value.Key;
                }
            }

            foreach (var kvp in _equipmentSelections.Where(pair => pair.Value != null))
            {
                selectedPartNames[kvp.Key] = kvp.Value.Key;
            }

            var renderers = new List<SkinnedMeshRenderer>();

            foreach (var entry in selectedPartNames.OrderBy(entry => (int)entry.Key))
            {
                if (!_runtimeLibrary.TryGetValue(entry.Key, out var partsByName))
                    continue;
                if (!partsByName.TryGetValue(entry.Value, out var part))
                    continue;

                var model = part.GetPartModel();
                var mesh = model != null ? model.GetComponentInChildren<SkinnedMeshRenderer>() : null;
                if (mesh != null)
                {
                    renderers.Add(mesh);
                }
            }

            DestroyPreviewInstance();

            if (renderers.Count == 0)
                return;

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

        private void DrawLightingControls()
        {
            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            _primaryLightIntensity = EditorGUILayout.Slider("Key Intensity", _primaryLightIntensity, 0f, 3f);
            _primaryLightYaw = EditorGUILayout.Slider("Key Yaw", _primaryLightYaw, -180f, 180f);
            _primaryLightPitch = EditorGUILayout.Slider("Key Pitch", _primaryLightPitch, -90f, 90f);
            _secondaryLightIntensity = EditorGUILayout.Slider("Fill Intensity", _secondaryLightIntensity, 0f, 3f);
            _secondaryLightYaw = EditorGUILayout.Slider("Fill Yaw", _secondaryLightYaw, -180f, 180f);
            _secondaryLightPitch = EditorGUILayout.Slider("Fill Pitch", _secondaryLightPitch, -90f, 90f);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyLightSettings();
                Repaint();
            }

            if (GUILayout.Button("Reset Lighting"))
            {
                ResetLighting();
            }

            EditorGUI.indentLevel--;
        }

        private void ApplyLightSettings()
        {
            if (_previewUtility == null || _previewUtility.lights == null || _previewUtility.lights.Length < 2)
                return;

            var primaryLight = _previewUtility.lights[0];
            primaryLight.intensity = Mathf.Max(0f, _primaryLightIntensity);
            primaryLight.transform.rotation = Quaternion.Euler(_primaryLightPitch, _primaryLightYaw, 0f);

            var secondaryLight = _previewUtility.lights[1];
            secondaryLight.intensity = Mathf.Max(0f, _secondaryLightIntensity);
            secondaryLight.transform.rotation = Quaternion.Euler(_secondaryLightPitch, _secondaryLightYaw, 0f);
        }

        private void ResetLighting()
        {
            _primaryLightYaw = DefaultPrimaryYaw;
            _primaryLightPitch = DefaultPrimaryPitch;
            _primaryLightIntensity = DefaultPrimaryIntensity;
            _secondaryLightYaw = DefaultSecondaryYaw;
            _secondaryLightPitch = DefaultSecondaryPitch;
            _secondaryLightIntensity = DefaultSecondaryIntensity;
            ApplyLightSettings();
            Repaint();
        }

        private IEnumerable<PartRecord> GetEquipmentCandidates(CharacterPartType partType)
        {
            if (!_partsByType.TryGetValue(partType, out var list))
                yield break;

            string term = _equipmentSearch?.Trim() ?? string.Empty;
            bool hasSearch = !string.IsNullOrEmpty(term);
            term = term.ToLowerInvariant();

            foreach (var record in list)
            {
                if (record?.Part == null)
                    continue;

                bool isBase = IsBasePart(record);
                if (!_includeBasePartsInEquipment && isBase)
                    continue;

                if (hasSearch)
                {
                    if (!MatchesSearch(record, term))
                        continue;
                }

                yield return record;
            }
        }

        private IEnumerable<PartRecord> GetBaseCandidates(CharacterPartType partType)
        {
            if (!_partsByType.TryGetValue(partType, out var list))
                return Enumerable.Empty<PartRecord>();

            var baseRecords = list
                .Where(IsBasePart)
                .ToList();

            if (baseRecords.Count > 0)
                return baseRecords;

            return list.Take(1); // fallback
        }

        private bool MatchesSearch(PartRecord record, string term)
        {
            if (record?.Part == null)
                return false;

            string name = record.Part.Name ?? string.Empty;
            string fileName = record.Part.FileName ?? string.Empty;
            string key = record.Key ?? string.Empty;

            if (name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (fileName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (key.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (record.Part.ID.ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private static string FormatPartLabel(PartRecord record)
        {
            if (record?.Part == null)
                return "Unknown";

            string displayName = GetDisplayName(record);
            return $"[{record.Part.ID}] {displayName}";
        }

        private static string GetDisplayName(PartRecord record)
        {
            if (record?.Part == null)
                return "Unknown";

            if (!string.IsNullOrWhiteSpace(record.Part.Name))
                return record.Part.Name;
            if (!string.IsNullOrWhiteSpace(record.Part.FileName))
                return record.Part.FileName;
            if (!string.IsNullOrWhiteSpace(record.Key))
                return record.Key;

            return $"Part #{record.Part.ID}";
        }

        private void SetActiveSpecies(SidekickSpecies species)
        {
            if (species == null || _activeSpecies == species)
                return;

            _activeSpecies = species;
            BuildPartCollections();
            EnsureDefaultSelections();
            RebuildCharacter();
        }

        private void BuildPartCollections()
        {
            _partsByType.Clear();

            if (_runtimeLibrary == null)
                return;

            foreach (var kvp in _runtimeLibrary)
            {
                var partType = kvp.Key;
                var perTypeRecords = new Dictionary<int, PartRecord>();

                foreach (var entry in kvp.Value)
                {
                    var part = entry.Value;
                    if (part == null)
                        continue;

                    if (!PartMatchesActiveSpecies(part))
                        continue;

                    if (perTypeRecords.ContainsKey(part.ID))
                        continue;

                    perTypeRecords[part.ID] = new PartRecord
                    {
                        Key = entry.Key,
                        Part = part
                    };
                }

                if (perTypeRecords.Count == 0)
                    continue;

                var ordered = perTypeRecords.Values
                    .OrderBy(record => record.Part.Name ?? record.Part.FileName ?? record.Key)
                    .ThenBy(record => record.Part.ID)
                    .ToList();

                _partsByType[partType] = ordered;
            }
        }

        private void EnsureDefaultSelections()
        {
            foreach (var type in BodyConfigurationTypes)
            {
                if (_baseSelections.TryGetValue(type, out var record) && record != null &&
                    _partsByType.TryGetValue(type, out var list) &&
                    list.Any(item => item.Part.ID == record.Part.ID))
                {
                    continue;
                }

                var baseCandidate = GetBaseCandidates(type).FirstOrDefault();
                if (baseCandidate != null)
                {
                    _baseSelections[type] = baseCandidate;
                }
                else
                {
                    _baseSelections.Remove(type);
                }
            }

            var keysToRemove = _equipmentSelections
                .Where(kvp => kvp.Value == null || !_partsByType.TryGetValue(kvp.Key, out var list) ||
                              !list.Any(record => record.Part.ID == kvp.Value.Part.ID))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _equipmentSelections.Remove(key);
            }

            EnsureValidPartTypeSelection();
        }

        private void EnsureValidPartTypeSelection()
        {
            var partTypes = _selectedEquipmentGroup.GetPartTypes();
            if (!partTypes.Contains(_selectedEquipmentPartType))
            {
                _selectedEquipmentPartType = partTypes.FirstOrDefault();
            }
        }

        private bool PartMatchesActiveSpecies(SidekickPart part)
        {
            if (part?.Species == null)
                return true;

            if (_activeSpecies == null)
                return true;

            if (part.Species.ID == _activeSpecies.ID)
                return true;

            if (_unrestrictedSpecies != null && part.Species.ID == _unrestrictedSpecies.ID)
                return true;

            return false;
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

        private static bool IsBasePart(PartRecord record)
        {
            if (record?.Part == null)
                return false;

            string fileName = record.Part.FileName ?? string.Empty;
            string name = record.Part.Name ?? string.Empty;

            return fileName.IndexOf("_BASE_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("_BASE_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   fileName.IndexOf("BASE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("BASE", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
#endif
