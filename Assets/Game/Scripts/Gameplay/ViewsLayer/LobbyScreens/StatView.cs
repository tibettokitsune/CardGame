using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.AsyncAssets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class StatView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI cardBonus;
        [SerializeField] private Image mainIcon;
        private CancellationTokenSource _cancellationTokenSource;

        public async Task<IDisposable> Setup(StatEntity cardEntity, ISpriteService spriteService)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            
            cardName.text = cardEntity.Name;
            cardBonus.text = $"{cardEntity.Value.ToString(cardEntity.Format)}";
            SetupAsync(cardEntity, spriteService, token).Forget();
            return Disposable.Create(() =>
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            });
        }
        
        private async UniTaskVoid SetupAsync(StatEntity cardEntity, ISpriteService spriteService, CancellationToken token)
        {
            try
            {
                var mainLayerIcon = await spriteService
                    .LoadSpriteForObject(cardEntity.Icon, mainIcon.gameObject, token);
                mainIcon.sprite = mainLayerIcon;
            }
            catch (OperationCanceledException)
            {
                // Загрузка отменена — ничего не делаем
            }
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}