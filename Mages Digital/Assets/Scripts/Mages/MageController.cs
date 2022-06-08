using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class MageController : MonoBehaviour
{

    [SerializeField] private int _health = 20;        // здоровье мага
    [SerializeField] private Mage _mage;              // данные мага
    [SerializeField] private PlayerController _owner; // игрок, управляющий магом

    [Header("Рука")]
    [SerializeField] private List<CardController> _treasures  = new List<CardController>(); // рука мага
    [SerializeField] private List<CardController> _deads      = new List<CardController>();
    [SerializeField] private List<CardController> _sources    = new List<CardController>();
    [SerializeField] private List<CardController> _qualities  = new List<CardController>();
    [SerializeField] private List<CardController> _deliveries = new List<CardController>();
    [SerializeField] private List<CardController> _wildMagics = new List<CardController>();

    [Header("Заклинание")]
    [SerializeField] private List<CardController> _spell = new List<CardController>(3)
    {
        null, null, null
    };

    [Header("Медали недобитого колдуна")]
    [SerializeField] private int _deadMedals = 0; // количество медалей недобитого колдуна

    [Header("Соседние маги")]
    [SerializeField] private MageController _leftMage  = null; // левый соседний маг
    [SerializeField] private MageController _rightMage = null; // правый соседний маг



    public Mage mage   => _mage;
    public int  health => _health;

    public PlayerController owner     => _owner;  
    public MageController   leftMage  => _leftMage;
    public MageController   rightMage => _rightMage;  

    public List<CardController> treasures  => _treasures;
    public List<CardController> deads      => _deads;
    public List<CardController> sources    => _sources;
    public List<CardController> qualities  => _qualities;
    public List<CardController> deliveries => _deliveries;
    public List<CardController> wildMagics => _wildMagics;
    public List<CardController> spell      => _spell;

    public int deadMedals => _deadMedals;


    public bool isDead        => _health <= 0;  // мертв ли маг

    public List<CardController> nonNullSpell => _spell.Where(card => card != null).ToList(); // карты заклинаний в спеле не равные null
    public int  nCardsInSpell   => nonNullSpell.Count;  // количество карт в заклинании
    public bool spellIsReady    => nCardsInSpell > 0;   // готово ли заклинание
    public int  spellInitiative => nonNullSpell.Sum(x => ((SpellCard) x.card).initiative); // сумарная инициатива спела
    public int  nCardsToDraw    => 8 - GetSpellsInHand().Count; // количество карт для добора из колоды

    void Awake()
    {
        _owner = gameObject.GetComponent<PlayerController>();
    }

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
    public IEnumerator AddToSpell(CardController cardToAdd, Order order, bool backToHand = true, bool ownerReaction = true)
    {
        cardToAdd.discoverable = false;

        // индекс расположения карты в заклинании
        int spellCardIndex = GetSpellIndexOfOrder(order);

        // текущая карта в заклинании
        CardController backToHandCard = _spell[spellCardIndex];
    
        // вернуть старую карту в заклинании обратно в руку
        if (backToHandCard != null && backToHand)
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

        if (ownerReaction)
            yield return owner.OnCardAddedToSpell(cardToAdd, order);

        cardToAdd.spellOrder   = order;

        cardToAdd.discoverable = true;
    }

    // добавить шальную магию к заклинанию
    public IEnumerator AddWildMagicToSpell(CardController cardToAdd)
    {
        GameManager.instance.SetChoosingState();
        yield return owner.ChooseOrder();
        if (owner.chosenOrder != Order.WILDMAGIC)
            yield return AddToSpell(cardToAdd, owner.chosenOrder);
        if (GameManager.instance.isChoosingState)
            GameManager.instance.ReturnToPrevState();
    }

    // вернуть карту обратно в руку
    public IEnumerator BackToHand(CardController backToHandCard, Order order)
    {

        int spellCardIndex = GetSpellIndexOfOrder(order);
        _spell[spellCardIndex] = null;
        yield return AddCard(backToHandCard);
        backToHandCard.StateToInHand();
    }

    // выполнить заклинание
    public IEnumerator ExecuteSpells()
    {
        // замена шальной магии
        List<CardController> spellWildMagics = nonNullSpell.Where(card => card.GetSpellCard().order == Order.WILDMAGIC).ToList();
        foreach (CardController wildMagic in spellWildMagics)
        {
            yield return wildMagic.Highlight(true);
            yield return GameManager.instance.spellsDeck.ReplaceWildMagic(wildMagic);
        }
        if (spellWildMagics.Count > 0)
            spellWildMagics.ForEach(wildMagic => Destroy(wildMagic.gameObject));
        
        // выполнение спелов карт
        foreach (CardController card in nonNullSpell)
        {
            yield return card.Highlight(true);
            yield return card.ExecuteSpell();
            yield return card.Highlight(false);
        }

        yield break;
    }

    // вернуть список, состоящий из всех карт заклинаний на руке мага
    // последовательно: заводилы, навороты, приходы, шальные магии
    public List<CardController> GetSpellsInHand()
    {
        List<CardController> spellCards = new List<CardController>();
        spellCards.AddRange(_sources);
        spellCards.AddRange(_qualities);
        spellCards.AddRange(_deliveries);
        spellCards.AddRange(_wildMagics);
        return spellCards;
    }

    public List<CardController> GetAllCards()
    {
        List<CardController> allCards = new List<CardController>();
        allCards.AddRange(GetSpellsInHand());
        allCards.AddRange(_treasures);
        allCards.AddRange(_deads);
        allCards.AddRange(nonNullSpell);
        return allCards;
    }

    public void SetAllDiscoverable(bool discoverable)
    {
        GetAllCards().ForEach(card => card.discoverable = discoverable);
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

    public void TakeDamage(int damage)
    {
        _health = _health - damage;
        if (isDead)
        {
            
        }
    } 

    public void Heal(int heal)
    {
        _health += heal;

        if(_health > maxHpMage)
        {
            _health = maxHpMage;
        }
    }

}
