using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Synty.SidekickCharacters.API;
using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Database.DTO;
using Synty.SidekickCharacters.Enums;
using Synty.SidekickCharacters.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Sidekick resources")]
    [SerializeField] private string baseModelResourcePath = "Meshes/SK_BaseModel";
    [SerializeField] private string baseMaterialResourcePath = "Materials/M_BaseMaterial";
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private Transform characterRoot;

    private readonly List<(string key, int index)> _pendingOverrides = new();
    private readonly Dictionary<PartGroup, SidekickPartPreset> _activePresets = new();
    private readonly Dictionary<PartGroup, SidekickPartPreset> _defaultPresets = new();

    private DatabaseManager _databaseManager;
    private SidekickRuntime _runtime;
    private Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> _partLibrary;
    private List<SidekickBodyShapePreset> _bodyShapePresets;
    private List<SidekickColorPreset> _colorPresets;
    private SidekickBodyShapePreset _currentBodyShape;
    private SidekickColorPreset _currentColorPreset;
    private GameObject _currentCharacter;
    private Animator _characterAnimator;
    private bool _isInitialized;
    private Dictionary<PartGroup, List<SidekickPartPreset>> _availablePresets = new();
    private Dictionary<int, List<SidekickPartPresetRow>> _presetRowsCache = new();
    private Dictionary<int, List<SidekickColorPresetRow>> _colorPresetRowsCache = new();

    private static readonly string OutputModelName = "Sidekick Character";

    private static readonly Dictionary<string, PartGroup> OverrideGroups =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {"head", PartGroup.Head},
            {"headcoverings_base_hair", PartGroup.Head},
            {"headcoverings_no_facialhair", PartGroup.Head},
            {"headcoverings_no_hair", PartGroup.Head},
            {"upperbody", PartGroup.UpperBody},
            {"torso", PartGroup.UpperBody},
            {"lowerbody", PartGroup.LowerBody},
            {"legs", PartGroup.LowerBody}
        };

    public bool IsInitialized => _isInitialized;
    public Animator Animator => _characterAnimator;
    public event Action<Animator> CharacterRebuilt;

    private async void Start()
    {
        await InitializeAsync();
    }

    private void OnDestroy()
    {
        if (_currentCharacter != null)
        {
            Destroy(_currentCharacter);
            _currentCharacter = null;
        }

        _databaseManager?.CloseConnection();
    }

    public void EnableItem(string key, int index)
    {
        if (!_isInitialized)
        {
            _pendingOverrides.Add((key, index));
            return;
        }

        if (!TryApplyOverride(key, index))
        {
            Debug.LogWarning($"CharacterCustomizer: unable to apply override '{key}'.");
        }
    }

    public void DisableItem(string key)
    {
        if (!_isInitialized)
        {
            _pendingOverrides.RemoveAll(o => string.Equals(o.key, key, StringComparison.OrdinalIgnoreCase));
            return;
        }

        if (!TryResolveGroup(key, out var group))
            return;

        if (_defaultPresets.TryGetValue(group, out var preset))
        {
            _activePresets[group] = preset;
            RebuildCharacter();
        }
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _databaseManager = new DatabaseManager();
        if (_databaseManager.GetCurrentDbConnection() == null)
        {
            await Task.Run(() => _databaseManager.GetDbConnection(true));
        }

        var baseModel = Resources.Load<GameObject>(baseModelResourcePath);
        var baseMaterial = Resources.Load<Material>(baseMaterialResourcePath);

        if (baseModel == null || baseMaterial == null)
        {
            Debug.LogError("CharacterCustomizer: Failed to load Sidekick base resources.");
            return;
        }

        if (animatorController == null)
        {
            Debug.LogWarning("CharacterCustomizer: Animator controller is not assigned. Character animations may not play.");
        }

        _runtime = new SidekickRuntime(baseModel, baseMaterial, animatorController, _databaseManager);
        await SidekickRuntime.PopulateToolData(_runtime);

        _partLibrary = _runtime.MappedPartDictionary;
        Dictionary<PartGroup, List<SidekickPartPreset>> availablePresets = null;
        Dictionary<int, List<SidekickPartPresetRow>> presetRows = null;
        Dictionary<int, List<SidekickColorPresetRow>> colorRows = null;
        List<SidekickBodyShapePreset> bodyShapes = null;
        List<SidekickColorPreset> colorPresets = null;

        await Task.Run(() =>
        {
            bodyShapes = SidekickBodyShapePreset.GetAll(_databaseManager);
            colorPresets = SidekickColorPreset.GetAllByColorGroup(_databaseManager, ColorGroup.Outfits);
            availablePresets = BuildAvailablePresets();
            presetRows = BuildPresetRowsCache(availablePresets);
            colorRows = BuildColorPresetRowsCache(colorPresets);
        });

        _bodyShapePresets = bodyShapes;
        _colorPresets = colorPresets;
        _availablePresets = availablePresets;
        _presetRowsCache = presetRows;
        _colorPresetRowsCache = colorRows;

        InitializeDefaults();
        RebuildCharacter();

        _isInitialized = true;

        if (_pendingOverrides.Count > 0)
        {
            var pending = _pendingOverrides.ToArray();
            _pendingOverrides.Clear();
            foreach (var request in pending)
            {
                TryApplyOverride(request.key, request.index);
            }
        }
    }

    private Dictionary<PartGroup, List<SidekickPartPreset>> BuildAvailablePresets()
    {
        var result = new Dictionary<PartGroup, List<SidekickPartPreset>>();
        foreach (PartGroup group in Enum.GetValues(typeof(PartGroup)))
        {
            if (group != PartGroup.Head && group != PartGroup.UpperBody && group != PartGroup.LowerBody)
                continue;

            var presets = SidekickPartPreset
                .GetAllByGroup(_databaseManager, group)
                .Where(preset => preset.HasAllPartsAvailable(_databaseManager))
                .ToList();

            if (presets.Count > 0)
            {
                result[group] = presets;
            }
        }

        return result;
    }

    private Dictionary<int, List<SidekickPartPresetRow>> BuildPresetRowsCache(
        Dictionary<PartGroup, List<SidekickPartPreset>> availablePresets)
    {
        var cache = new Dictionary<int, List<SidekickPartPresetRow>>();
        foreach (var presets in availablePresets.Values)
        {
            foreach (var preset in presets)
            {
                if (cache.ContainsKey(preset.ID))
                    continue;

                cache[preset.ID] = SidekickPartPresetRow.GetAllByPreset(_databaseManager, preset);
            }
        }

        return cache;
    }

    private Dictionary<int, List<SidekickColorPresetRow>> BuildColorPresetRowsCache(
        IEnumerable<SidekickColorPreset> colorPresets)
    {
        var cache = new Dictionary<int, List<SidekickColorPresetRow>>();
        foreach (var preset in colorPresets)
        {
            cache[preset.ID] = SidekickColorPresetRow.GetAllByPreset(_databaseManager, preset);
        }

        return cache;
    }

    private void InitializeDefaults()
    {
        foreach (var kvp in _availablePresets)
        {
            var preset = kvp.Value[0];//Random.Range(0, kvp.Value.Count)];
            _defaultPresets[kvp.Key] = preset;
            _activePresets[kvp.Key] = preset;
        }

        if (_bodyShapePresets != null && _bodyShapePresets.Count > 0)
        {
            _currentBodyShape = _bodyShapePresets[0];//Random.Range(0, _bodyShapePresets.Count)];
        }

        if (_colorPresets != null && _colorPresets.Count > 0)
        {
            _currentColorPreset = _colorPresets[0];//Random.Range(0, _colorPresets.Count)];
        }
    }

    private bool TryApplyOverride(string key, int index)
    {
        if (!TryResolveGroup(key, out var group))
            return false;

        if (!_availablePresets.TryGetValue(group, out var presets) || presets.Count == 0)
            return false;

        int targetIndex = index >= 0 && index < presets.Count
            ? index
            : Random.Range(0, presets.Count);

        _activePresets[group] = presets[targetIndex];
        RebuildCharacter();
        return true;
    }

    private bool TryResolveGroup(string key, out PartGroup group)
    {
        if (OverrideGroups.TryGetValue(key, out group))
            return true;

        if (Enum.TryParse(key, true, out group))
            return true;

        return false;
    }

    private void RebuildCharacter()
    {
        if (_runtime == null)
            return;

        var presetsToApply = _activePresets.Values.Where(p => p != null).ToList();
        if (presetsToApply.Count == 0)
            return;

        var partsToUse = new List<SkinnedMeshRenderer>();

        foreach (var preset in presetsToApply)
        {
            if (!_presetRowsCache.TryGetValue(preset.ID, out var rows))
                continue;
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.PartName))
                    continue;

                var partType = Enum.Parse<CharacterPartType>(CharacterPartTypeUtils.GetTypeNameFromShortcode(row.PartType));
                if (!_partLibrary.TryGetValue(partType, out var locationDictionary))
                    continue;
                if (!locationDictionary.TryGetValue(row.PartName, out var part))
                    continue;

                var partModel = part.GetPartModel();
                var mesh = partModel.GetComponentInChildren<SkinnedMeshRenderer>();
                if (mesh != null)
                {
                    partsToUse.Add(mesh);
                }
            }
        }

        if (partsToUse.Count == 0)
            return;

        ApplyBodyShape();
        ApplyColorPreset();

        var targetRoot = characterRoot != null ? characterRoot : transform;
        Vector3 cachedLocalPosition = Vector3.zero;
        Quaternion cachedLocalRotation = Quaternion.identity;
        Vector3 cachedLocalScale = Vector3.one;

        if (_currentCharacter != null)
        {
            var currentTransform = _currentCharacter.transform;
            cachedLocalPosition = currentTransform.localPosition;
            cachedLocalRotation = currentTransform.localRotation;
            cachedLocalScale = currentTransform.localScale;
            Destroy(_currentCharacter);
            _currentCharacter = null;
            _characterAnimator = null;
        }

        var character = _runtime.CreateCharacter(OutputModelName, partsToUse, false, true);
        character.transform.SetParent(targetRoot, false);
        character.transform.localPosition = cachedLocalPosition;
        character.transform.localRotation = cachedLocalRotation;
        character.transform.localScale = cachedLocalScale;

        _currentCharacter = character;
        _characterAnimator = character.GetComponentInChildren<Animator>();
        CharacterRebuilt?.Invoke(_characterAnimator);
    }

    private void ApplyBodyShape()
    {
        if (_currentBodyShape == null)
            return;

        _runtime.BodyTypeBlendValue = _currentBodyShape.BodyType;
        _runtime.MusclesBlendValue = _currentBodyShape.Musculature;
        _runtime.BodySizeHeavyBlendValue = _currentBodyShape.BodySize > 0 ? _currentBodyShape.BodySize : 0f;
        _runtime.BodySizeSkinnyBlendValue = _currentBodyShape.BodySize < 0 ? -_currentBodyShape.BodySize : 0f;
    }

    private void ApplyColorPreset()
    {
        if (_currentColorPreset == null)
            return;

        if (!_colorPresetRowsCache.TryGetValue(_currentColorPreset.ID, out var colorRows))
            return;
        foreach (var row in colorRows)
        {
            var colorRow = SidekickColorRow.CreateFromPresetColorRow(row);
            foreach (ColorType property in Enum.GetValues(typeof(ColorType)))
            {
                _runtime.UpdateColor(property, colorRow);
            }
        }
    }
}
