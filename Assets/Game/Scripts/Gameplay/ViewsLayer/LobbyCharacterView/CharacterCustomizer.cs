using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class CharacterObjectGroups
{
    public List<GameObject> headAllElements;
    public List<GameObject> headNoElements;
    public List<GameObject> eyebrow;
    public List<GameObject> facialHair;
    public List<GameObject> torso;
    public List<GameObject> arm_Upper_Right;
    public List<GameObject> arm_Upper_Left;
    public List<GameObject> arm_Lower_Right;
    public List<GameObject> arm_Lower_Left;
    public List<GameObject> hand_Right;
    public List<GameObject> hand_Left;
    public List<GameObject> hips;
    public List<GameObject> leg_Right;
    public List<GameObject> leg_Left;
}

[System.Serializable]
public class CharacterObjectListsAllGender
{
    public List<GameObject> headCoverings_Base_Hair;
    public List<GameObject> headCoverings_No_FacialHair;
    public List<GameObject> headCoverings_No_Hair;
    public List<GameObject> all_Hair;
    public List<GameObject> all_Head_Attachment;
    public List<GameObject> chest_Attachment;
    public List<GameObject> back_Attachment;
    public List<GameObject> shoulder_Attachment_Right;
    public List<GameObject> shoulder_Attachment_Left;
    public List<GameObject> elbow_Attachment_Right;
    public List<GameObject> elbow_Attachment_Left;
    public List<GameObject> hips_Attachment;
    public List<GameObject> knee_Attachement_Right;
    public List<GameObject> knee_Attachement_Left;
    public List<GameObject> all_12_Extra;
    public List<GameObject> elf_Ear;
}

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Слоты по частям тела")] public CharacterObjectGroups male = new CharacterObjectGroups();
    public CharacterObjectGroups female = new CharacterObjectGroups();
    public CharacterObjectListsAllGender allGender = new CharacterObjectListsAllGender();

    [Header("Активные объекты")] [HideInInspector]
    public List<GameObject> enabledObjects = new();

    public enum GenderType
    {
        Male,
        Female
    }

    void Start()
    {
        SetGender(GenderType.Male); // Можно указать Female
    }

    [Button]
    private void BuildLists()
    {
        // Мужские части
        BuildList(male.headAllElements, "Male_Head_All_Elements");
        BuildList(male.headNoElements, "Male_Head_No_Elements");
        BuildList(male.eyebrow, "Male_01_Eyebrows");
        BuildList(male.facialHair, "Male_02_FacialHair");
        BuildList(male.torso, "Male_03_Torso");
        BuildList(male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
        BuildList(male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
        BuildList(male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
        BuildList(male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
        BuildList(male.hand_Right, "Male_08_Hand_Right");
        BuildList(male.hand_Left, "Male_09_Hand_Left");
        BuildList(male.hips, "Male_10_Hips");
        BuildList(male.leg_Right, "Male_11_Leg_Right");
        BuildList(male.leg_Left, "Male_12_Leg_Left");

        // Женские части
        BuildList(female.headAllElements, "Female_Head_All_Elements");
        BuildList(female.headNoElements, "Female_Head_No_Elements");
        BuildList(female.eyebrow, "Female_01_Eyebrows");
        BuildList(female.facialHair, "Female_02_FacialHair");
        BuildList(female.torso, "Female_03_Torso");
        BuildList(female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
        BuildList(female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
        BuildList(female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
        BuildList(female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
        BuildList(female.hand_Right, "Female_08_Hand_Right");
        BuildList(female.hand_Left, "Female_09_Hand_Left");
        BuildList(female.hips, "Female_10_Hips");
        BuildList(female.leg_Right, "Female_11_Leg_Right");
        BuildList(female.leg_Left, "Female_12_Leg_Left");

        // Унисекс-элементы
        BuildList(allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
        BuildList(allGender.headCoverings_No_FacialHair, "HeadCoverings_No_FacialHair");
        BuildList(allGender.headCoverings_No_Hair, "HeadCoverings_No_Hair");
        BuildList(allGender.all_Hair, "All_01_Hair");
        BuildList(allGender.all_Head_Attachment, "All_02_Head_Attachment");
        BuildList(allGender.chest_Attachment, "All_03_Chest_Attachment");
        BuildList(allGender.back_Attachment, "All_04_Back_Attachment");
        BuildList(allGender.shoulder_Attachment_Right, "All_05_Shoulder_Attachment_Right");
        BuildList(allGender.shoulder_Attachment_Left, "All_06_Shoulder_Attachment_Left");
        BuildList(allGender.elbow_Attachment_Right, "All_07_Elbow_Attachment_Right");
        BuildList(allGender.elbow_Attachment_Left, "All_08_Elbow_Attachment_Left");
        BuildList(allGender.hips_Attachment, "All_09_Hips_Attachment");
        BuildList(allGender.knee_Attachement_Right, "All_10_Knee_Attachement_Right");
        BuildList(allGender.knee_Attachement_Left, "All_11_Knee_Attachement_Left");
        BuildList(allGender.all_12_Extra, "All_12_Extra");
        BuildList(allGender.elf_Ear, "Elf_Ear");
    }

    private void BuildList(List<GameObject> list, string partName)
    {
        list.Clear();
        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t.name == partName)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    var go = t.GetChild(i).gameObject;
                    go.SetActive(false);
                    list.Add(go);
                }

                break;
            }
        }
    }

    public void SetGender(GenderType gender)
    {
        DisableAll();
        var group = gender == GenderType.Male ? male : female;

        var defaultItemIndex = 5;
        Enable(group.headAllElements, defaultItemIndex);
        Enable(group.eyebrow, defaultItemIndex);
        Enable(group.facialHair, defaultItemIndex);
        Enable(group.torso, defaultItemIndex);
        Enable(group.arm_Upper_Right, defaultItemIndex);
        Enable(group.arm_Upper_Left, defaultItemIndex);
        Enable(group.arm_Lower_Right, defaultItemIndex);
        Enable(group.arm_Lower_Left, defaultItemIndex);
        Enable(group.hand_Right, defaultItemIndex);
        Enable(group.hand_Left, defaultItemIndex);
        Enable(group.hips, defaultItemIndex);
        Enable(group.leg_Right, defaultItemIndex);
        Enable(group.leg_Left, defaultItemIndex);
    }

    private void Enable(List<GameObject> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return;

        var go = list[index];
        go.SetActive(true);
        enabledObjects.Add(go);
    }

    private void DisableAll()
    {
        foreach (var go in enabledObjects)
        {
            if (go) go.SetActive(false);
        }

        enabledObjects.Clear();
    }
    
    private List<GameObject> ResolveListByKey(string key)
    {
        var genderFields = typeof(CharacterObjectGroups).GetFields();
        var allGenderFields = typeof(CharacterObjectListsAllGender).GetFields();

        foreach (var field in genderFields)
        {
            if (field.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase))
            {
                // Пробуем сначала у male
                var value = field.GetValue(male) as List<GameObject>;
                if (value != null && value.Count > 0) return value;

                // Если в male ничего нет — fallback на female
                return field.GetValue(female) as List<GameObject>;
            }
        }

        foreach (var field in allGenderFields)
        {
            if (field.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase))
            {
                return field.GetValue(allGender) as List<GameObject>;
            }
        }

        Debug.LogWarning($"CharacterCustomizer: список с ключом '{key}' не найден.");
        return null;
    }
    
    public void EnableItem(string key, int index)
    {
        DisableItem(key);

        var list = ResolveListByKey(key);
        if (list != null && index >= 0 && index < list.Count)
        {
            GameObject go = list[index];
            go.SetActive(true);
            enabledObjects.Add(go);
        }
    }

    public void DisableItem(string key)
    {
        var list = ResolveListByKey(key);
        if (list == null) return;

        foreach (var go in list)
        {
            if (go) go.SetActive(false);
        }

        enabledObjects.RemoveAll(obj => list.Contains(obj));
    }
}