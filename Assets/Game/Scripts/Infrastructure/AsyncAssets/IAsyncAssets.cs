using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Infrastructure.AsyncAssets
{
    public interface ISpriteService
    {
        Task<Sprite> LoadSpriteForObject(string spritePath, GameObject owner);

        /// <summary> Принудительно выгружает все спрайты, связанные с объектом </summary>
        void UnloadSpritesForObject(GameObject owner);

        /// <summary> Очищает все загруженные спрайты </summary>
        void ClearAll();
    }

    public class ResourceSpriteService : ISpriteService
    {
        private class SpriteReference
        {
            public Sprite Sprite;
            public int ReferenceCount;
        }

        private readonly Dictionary<string, SpriteReference> _loadedSprites = new();
        private readonly Dictionary<GameObject, List<string>> _objectSpriteMap = new();

        public async Task<Sprite> LoadSpriteForObject(string spritePath, GameObject owner)
        {
            if (_loadedSprites.TryGetValue(spritePath, out var sprite))
            {
                TrackSpriteForObject(spritePath, owner);
                return sprite.Sprite;
            }

            var request = Resources.LoadAsync<Sprite>(spritePath);
            while (!request.isDone)
                await Task.Yield();

            if (request.asset is Sprite loadedSprite)
            {
                if (_loadedSprites.TryGetValue(spritePath, out var spriteReference))
                {
                    _loadedSprites[spritePath].Sprite = loadedSprite;
                }
                else
                {
                    _loadedSprites.Add(spritePath, new SpriteReference {Sprite = loadedSprite, ReferenceCount = 1});
                }

                TrackSpriteForObject(spritePath, owner);
                return loadedSprite;
            }

            Debug.LogError($"Failed to load sprite at path: {spritePath}");
            return null;
        }

        public void UnloadSpritesForObject(GameObject owner)
        {
            if (owner == null || !_objectSpriteMap.TryGetValue(owner, out var spritePaths))
                return;

            foreach (var path in spritePaths)
            {
                if (_loadedSprites.TryGetValue(path, out var spriteRef))
                {
                    spriteRef.ReferenceCount--;

                    if (spriteRef.ReferenceCount <= 0)
                    {
                        Resources.UnloadAsset(spriteRef.Sprite);
                        _loadedSprites.Remove(path);
                    }
                }
            }

            _objectSpriteMap.Remove(owner);
        }

        private void TrackSpriteForObject(string spritePath, GameObject owner)
        {
            if (!_objectSpriteMap.TryGetValue(owner, out var spriteList))
            {
                spriteList = new List<string>();
                _objectSpriteMap[owner] = spriteList;

                // Добавляем компонент для автоматической очистки при уничтожении объекта
                if (!owner.TryGetComponent<SpriteOwnerCleanup>(out _))
                {
                    var cleanup = owner.AddComponent<SpriteOwnerCleanup>();
                    cleanup.Initialize(this);
                }
            }

            if (!spriteList.Contains(spritePath))
            {
                spriteList.Add(spritePath);
            }
        }

        public void ClearAll()
        {
            foreach (var kvp in _loadedSprites)
            {
                Resources.UnloadAsset(kvp.Value.Sprite);
            }

            _loadedSprites.Clear();
            _objectSpriteMap.Clear();
        }

        public void Dispose()
        {
            ClearAll();
        }
    }
}