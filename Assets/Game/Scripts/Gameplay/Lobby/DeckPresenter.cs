using System.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Gameplay.Lobby
{
    public enum CardType
    {
        Door,
        Treasure
    }
    public interface IBaseCard
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
        CardType CardType { get; }
    }

    public class BaseCard : IBaseCard
    {
        public string ID { get; }
        public string Name { get; }
        public string Description { get; }
        public CardType CardType { get; }
    }
    
    public interface IDeckPresenter
    {
        Task<IBaseCard> ClaimRandomCardFromDeck(CardType cardType);
        
    }
    
    public class DeckPresenter : IDeckPresenter
    {
        public async Task<IBaseCard> ClaimRandomCardFromDeck(CardType cardType)
        {
            Debug.Log($"ClaimRandomCardFromDeck {cardType.ToString()}");
            return new BaseCard();
        }
    }
}