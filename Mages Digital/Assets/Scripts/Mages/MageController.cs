using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;


public class MageController : MonoBehaviour
{

    [SerializeField] private int _health = 20;      // здоровье мага

    [Header("Hand")]
    [SerializeField] private List<CardController> _treasures  = new List<CardController>();
    [SerializeField] private List<CardController> _deads      = new List<CardController>();
    [SerializeField] private List<CardController> _sources    = new List<CardController>();
    [SerializeField] private List<CardController> _qualities  = new List<CardController>();
    [SerializeField] private List<CardController> _deliveries = new List<CardController>();
    [SerializeField] private List<CardController> _wildMagics = new List<CardController>();

    [Header("Spell")]
    [SerializeField] private CardController _spellSource;
    [SerializeField] private CardController _spellQuality;
    [SerializeField] private CardController _spellDelivery;

    private MageController _leftMage  = null;
    private MageController _rightMage = null;


    public Mage mage;  // данные мага
    public PlayerController owner;
    
    public bool isDead  => _health <= 0; // мертв ли маг
    public int health   => _health;
    public MageController leftMage  => _leftMage;
    public MageController rightMage => _rightMage;  
    public List<CardController> treasures  => _treasures;
    public List<CardController> deads      => _deads;
    public List<CardController> sources    => _sources;
    public List<CardController> qualities  => _qualities;
    public List<CardController> deliveries => _deliveries;
    public List<CardController> wildMagics => _wildMagics;
    public CardController spellSource => _spellSource;
    public CardController spellQuality => _spellQuality;
    public CardController spellDelivery => _spellDelivery;

    public IEnumerator AddCard(CardController cardController, bool moveToOwner = false)
    {
        cardController.SetOwner(this);
        Card card = cardController.card;
        if (card != null)
        {
            List<CardController> hand = GetHandOfCardType(card);
            AddCardToHand(hand, cardController);

            if (moveToOwner)
                yield return owner.AddToHand(cardController);
        }
        yield break;
    }

    public List<CardController> GetSpells()
    {
        List<CardController> spellCards = new List<CardController>();
        spellCards.AddRange(_sources);
        spellCards.AddRange(_qualities);
        spellCards.AddRange(_deliveries);
        spellCards.AddRange(_wildMagics);
        return spellCards;
    }

    public void AddCardToHand(List<CardController> hand, CardController cardController)
    {
        hand?.Add(cardController);
        cardController.inHand = true;
        if (cardController.card != null && cardController.card.cardType == CardType.SPELL)
        {
            hand.Sort((c1, c2) => ((SpellCard)c1.card).sign.CompareTo(((SpellCard)c2.card).sign));
        }
    }

    public IEnumerator AddToSpell(CardController cardToAdd, Order order)
    {
        CardController spellCard = GetSpellCardOfOrder(order);
        CardController backToHandCard = spellCard;
        if (backToHandCard != null)
        {
            StartCoroutine(AddCard(backToHandCard, true));
        }
        List<CardController> hand = GetHandOfCardType(cardToAdd.card);
        hand.Remove(cardToAdd);
        spellCard = cardToAdd;
        yield return owner.OnSpellCardAddedToSpell(spellCard, order);
    }

    public IEnumerator BackToHand(CardController backToHandCard, Order order)
    {
        CardController spellCard = GetSpellCardOfOrder(order);
        spellCard = null;
        yield return AddCard(backToHandCard, true);
    }

    public CardController GetSpellCardOfOrder(Order order)
    {
        switch (order)
        {
            case Order.SOURCE:
                return _spellSource;
            case Order.QUALITY:
                return _spellQuality;
            case Order.DELIVERY:
                return _spellDelivery;
        }
        return null;
    }

    public List<CardController> GetHandOfCardType(Card card)
    {
        List<CardController> hand = null;
        switch (card.cardType)
        {
            case CardType.SPELL:
                hand = GetSpellHandOfCardType(card);
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

    public List<CardController> GetSpellHandOfCardType(Card card)
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
