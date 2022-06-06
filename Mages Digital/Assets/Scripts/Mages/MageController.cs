using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;


public class MageController : MonoBehaviour
{

    [SerializeField] private int _health = 20;      // здоровье мага

    [Header("Hand")]
    [SerializeField] private List<CardController> _treasures  = new List<CardController>(); // рука мага
    [SerializeField] private List<CardController> _deads      = new List<CardController>();
    [SerializeField] private List<CardController> _sources    = new List<CardController>();
    [SerializeField] private List<CardController> _qualities  = new List<CardController>();
    [SerializeField] private List<CardController> _deliveries = new List<CardController>();
    [SerializeField] private List<CardController> _wildMagics = new List<CardController>();

    [Header("Spell")]
    [SerializeField] private List<CardController> _spell = new List<CardController>(3)
    {
        null, null, null
    };

    private MageController _leftMage  = null; // левый соседний маг
    private MageController _rightMage = null; // правый соседний маг


    public Mage mage;               // данные мага
    public PlayerController owner;  // игрок, управляющий магом
    
    public int health   => _health;
    public MageController leftMage  => _leftMage;
    public MageController rightMage => _rightMage;  
    public List<CardController> treasures  => _treasures;
    public List<CardController> deads      => _deads;
    public List<CardController> sources    => _sources;
    public List<CardController> qualities  => _qualities;
    public List<CardController> deliveries => _deliveries;
    public List<CardController> wildMagics => _wildMagics;
    public List<CardController> spell      => _spell;

    public bool isDead       => _health <= 0;  // мертв ли маг
    public int  cardsInSpell => _spell.FindAll(x => x != null).Count;
    public bool spellIsReady => cardsInSpell > 0;


    // добавить карту в руку мага
    public IEnumerator AddCard(CardController cardController)
    {
        cardController.SetOwner(this);
        Card card = cardController.card;
        if (card != null)
        {
            List<CardController> hand = GetHandOfCardType(card);
            AddCardToHand(hand, cardController);
            yield return owner.OnCardAddedToHand(cardController);
        }
        yield break;
    }

    // добавить карту в заклинание
    public IEnumerator AddToSpell(CardController cardToAdd, Order order)
    {

        cardToAdd.discoverable = false;

        // индекс расположения карты в заклинании
        int spellCardIndex = GetSpellIndexOfOrder(order);

        // текущая карта в заклинании
        CardController backToHandCard = _spell[spellCardIndex];
    
        // вернуть старую карту в заклинании обратно в руку
        if (backToHandCard != null)
        {
            backToHandCard.discoverable = false;
            yield return AddCard(backToHandCard);
            backToHandCard.discoverable = true;
        }

        // удалить карту для заклинания из руки и добавить в заклинание
        List<CardController> hand = GetHandOfCardType(cardToAdd.card);
        hand.Remove(cardToAdd);
        _spell[spellCardIndex] = cardToAdd;
        cardToAdd.StateToInSpell();

        yield return owner.OnCardAddedToSpell(cardToAdd, order);

        cardToAdd.spellOrder   = order;
        cardToAdd.discoverable = true;
    }

    // добавить шальную магию к заклинанию
    public IEnumerator AddWildMagicToSpell(CardController cardToAdd)
    {
        yield return owner.ChooseOrder();
        if (owner.chosenOrder != Order.WILDMAGIC)
            yield return AddToSpell(cardToAdd, owner.chosenOrder);
    }

    // вернуть карту обратно в руку
    public IEnumerator BackToHand(CardController backToHandCard, Order order)
    {

        int spellCardIndex = GetSpellIndexOfOrder(order);
        _spell[spellCardIndex] = null;
        yield return AddCard(backToHandCard);
        backToHandCard.StateToInHand();
    }

    // вернуть список, состоящий из всех карт заклинаний на руке мага
    // последовательно: заводилы, навороты, приходы, шальные магии
    public List<CardController> GetSpells()
    {
        List<CardController> spellCards = new List<CardController>();
        spellCards.AddRange(_sources);
        spellCards.AddRange(_qualities);
        spellCards.AddRange(_deliveries);
        spellCards.AddRange(_wildMagics);
        return spellCards;
    }

    // добавить карту в определенную часть руки
    // если добавляется заклинание, то сортировать обновленный список
    public void AddCardToHand(List<CardController> hand, CardController cardController)
    {
        hand?.Add(cardController);
        cardController.StateToInHand();
        if (cardController.card != null && cardController.isSpell)
        {
            hand.Sort((c1, c2) => ((SpellCard)c1.card).sign.CompareTo(((SpellCard)c2.card).sign));
        }
    }

    // вернуть индекс заклинания
    public static int GetSpellIndexOfOrder(Order order)
    {
        switch (order)
        {
            case Order.SOURCE:
                return 0;
            case Order.QUALITY:
                return 1;
            case Order.DELIVERY:
                return 2;
        }
        return -1;
    }

    public static Order GetOrderOfSpellIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Order.SOURCE;
            case 1:
                return Order.QUALITY;
            case 2:
                return Order.DELIVERY;
        }
        return Order.WILDMAGIC;
    }

    // вернуть определенную руку по типу карты
    public List<CardController> GetHandOfCardType(Card card)
    {
        List<CardController> hand = null;
        switch (card.cardType)
        {
            case CardType.SPELL:
                hand = GetSpellHandOfCardOrder(card);
                break;
            case CardType.TREASURE:
                hand = _treasures;
                break;
            case CardType.DEAD:
                hand = _deads;
                break;
        }
        return hand;
    }

    // вернуть определенный список по порядку карты заклинания
    public List<CardController> GetSpellHandOfCardOrder(Card card)
    {
        SpellCard spellCard = (SpellCard) card;
        List<CardController> hand = null;
        switch (spellCard.order)
        {
            case Order.SOURCE:
                hand = _sources;
                break;
            case Order.QUALITY:
                hand = _qualities;
                break;
            case Order.DELIVERY:
                hand = _deliveries;
                break;
            case Order.WILDMAGIC:
                hand = _wildMagics;
                break;
        }
        return hand;
    }

}
