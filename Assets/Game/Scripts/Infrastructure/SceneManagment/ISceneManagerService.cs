using System.Threading.Tasks;

namespace Game.Scripts.Infrastructure.SceneManagment
{
    public enum SceneLayer
    {
        GameStage
    }

    public interface ISceneManagerService
    {
        Task LoadScene(string sceneName, SceneLayer layer, bool isActivateAfterLoad = false);
    }
}