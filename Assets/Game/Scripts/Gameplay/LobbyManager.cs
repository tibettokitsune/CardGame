using Unity.VisualScripting;

namespace Game.Scripts.Gameplay
{
    public class LobbyManager : ILobbyManager, IInitializable
    {
        public void Initialize()
        {
            PrepareLobby();
        }

        private void PrepareLobby()
        {
            
        }
    }

    public interface ILobbyManager
    {
    }
}