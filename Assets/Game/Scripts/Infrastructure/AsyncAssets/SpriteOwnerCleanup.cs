using UnityEngine;

namespace Game.Scripts.Infrastructure.AsyncAssets
{
    public class SpriteOwnerCleanup : MonoBehaviour
    {
        private ISpriteService _spriteService;

        public void Initialize(ISpriteService spriteService)
        {
            _spriteService = spriteService;
        }

        private void OnDestroy()
        {
            _spriteService?.UnloadSpritesForObject(gameObject);
        }
    }
}