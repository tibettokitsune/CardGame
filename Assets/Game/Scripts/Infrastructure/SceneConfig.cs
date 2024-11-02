using UnityEngine;

[CreateAssetMenu(fileName = "SceneConfig", menuName = "Configs/SceneConfig")]
public class SceneConfig : ScriptableObject
{
    public string[] gameplayScenes;
    public string[] mainMenuScenes;
}
