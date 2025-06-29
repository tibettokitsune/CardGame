using System.Threading.Tasks;

namespace Game.Scripts.Infrastructure.SceneManagment
{
    public enum SceneLayer
    {
        GameStage,
        GameplayElement
    }

    public interface ISceneManagerService
    {
        Task LoadScene(string sceneName, SceneLayer layer, bool isActivateAfterLoad = false);
        Task UnloadScene(string sceneName,  SceneLayer layer);
    }
}