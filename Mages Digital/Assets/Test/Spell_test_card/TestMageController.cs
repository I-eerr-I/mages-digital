using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;


public class TestMageController : MonoBehaviour
{

    [SerializeField] private int _health = 20;      // здоровье мага

    [Header("Hand")]
    [SerializeField] private List<TestCardController> _treasures  = new List<TestCardController>(); // рука мага
    [SerializeField] private List<TestCardController> _deads      = new List<TestCardController>();
    [SerializeField] private List<TestCardController> _sources    = new List<TestCardController>();
    [SerializeField] private List<TestCardController> _qualities  = new List<TestCardController>();
    [SerializeField] private List<TestCardController> _deliveries = new List<TestCardController>();
    [SerializeField] private List<TestCardController> _wildMagics = new List<TestCardController>();

    [Header("Spell")]
    [SerializeField] private List<TestCardController> _spell = new List<TestCardController>(3)
    {
        null, null, null
    };
    //CHANGE
    [SerializeField] private TestMageController _leftMage  = null; // левый соседний маг
    [SerializeField] private TestMageController _rightMage = null; // правый соседний маг
    [SerializeField] private int _deadMedals = 0; // Медали недобитого мага 

    //CHANGE
    public Mage mage;               // данные мага
    public PlayerController owner;  // игрок, управляющий магом

    public int maxHpMage = 25;
    public int health                   => _health; 
    public TestMageController leftMage  => _leftMage;
    public TestMageController rightMage => _rightMage;  
    public List<TestCardController> treasures  => _treasures;
    public List<TestCardController> deads      => _deads;
    public List<TestCardController> sources    => _sources;
    public List<TestCardController> qualities  => _qualities;
    public List<TestCardController> deliveries => _deliveries;
    public List<TestCardController> wildMagics => _wildMagics;
    public List<TestCardController> spell      => _spell;
    public int deadMedals                      => _deadMedals;// CHANGE


    public bool isDead       => health <= 0;  // мертв ли маг
    public int  cardsInSpell => _spell.FindAll(x => x != null).Count;
    public bool spellIsReady => cardsInSpell > 0;

    //добавить карту в руку мага
    public IEnumerator AddCard(TestCardController TestCardController)
    {   
        TestCardController.SetOwner(this);
        Card card = TestCardController.card;
        if (card != null)
        {
            List<TestCardController> hand = GetHandOfCardType(card);
            AddCardToHand(hand, TestCardController);
            if (owner !=  null)// CHANGE
            {

                //yield return owner.OnCardAddedToHand(TestCardController);
            }
            
        }
        yield break;
    }

    // добавить карту в заклинание
    // public IEnumerator AddToSpell(TestCardController cardToAdd, Order order)
    // {

    //     cardToAdd.discoverable = false;

    //     // индекс расположения карты в заклинании
    //     int spellCardIndex = GetSpellIndexOfOrder(order);

    //     // текущая карта в заклинании
    //     TestCardController backToHandCard = _spell[spellCardIndex];
    
    //     // вернуть старую карту в заклинании обратно в руку
    //     if (backToHandCard != null)
    //     {
    //         backToHandCard.discoverable = false;
    //         yield return AddCard(backToHandCard);
    //         backToHandCard.discoverable = true;
    //     }

    //     // удалить карту для заклинания из руки и добавить в заклинание
    //     List<TestCardController> hand = GetHandOfCardType(cardToAdd.card);
    //     hand.Remove(cardToAdd);
    //     _spell[spellCardIndex] = cardToAdd;
    //     cardToAdd.StateToInSpell();

    //     yield return owner.OnCardAddedToSpell(cardToAdd, order);

    //     cardToAdd.spellOrder   = order;
    //     cardToAdd.discoverable = true;
    // }

    // добавить шальную магию к заклинанию
    // public IEnumerator AddWildMagicToSpell(TestCardController cardToAdd)
    // {
    //     yield return owner.ChooseOrder();
    //     if (owner.chosenOrder != Order.WILDMAGIC)
    //         yield return AddToSpell(cardToAdd, owner.chosenOrder);
    // }

    // вернуть карту обратно в руку
    // public IEnumerator BackToHand(TestCardController backToHandCard, Order order)
    // {

    //     int spellCardIndex = GetSpellIndexOfOrder(order);
    //     _spell[spellCardIndex] = null;
    //     yield return AddCard(backToHandCard);
    //     backToHandCard.StateToInHand();
    // }

    // вернуть список, состоящий из всех карт заклинаний на руке мага
    // последовательно: заводилы, навороты, приходы, шальные магии
    public List<TestCardController> GetSpells()
    {
        List<TestCardController> spellCards = new List<TestCardController>();
        spellCards.AddRange(_sources);
        spellCards.AddRange(_qualities);
        spellCards.AddRange(_deliveries);
        spellCards.AddRange(_wildMagics);
        return spellCards;
    }

    // добавить карту в определенную часть руки
    // если добавляется заклинание, то сортировать обновленный список
    public void AddCardToHand(List<TestCardController> hand, TestCardController TestCardController)
    {
        hand?.Add(TestCardController);
        TestCardController.StateToInHand();
        if (TestCardController.card != null && TestCardController.isSpell)
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
    public List<TestCardController> GetHandOfCardType(Card card)
    {
        List<TestCardController> hand = null;
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
    public List<TestCardController> GetSpellHandOfCardOrder(Card card)
    {
        SpellCard spellCard = (SpellCard) card;
        List<TestCardController> hand = null;
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
///////////////////////////////////////////////////////////////////////////////////////////////////
    //CHANGE
///////////////////////////////////////////////////////////////////////////////////////////////////   

    // Метод нанесение урона цели
    public void TakeDamage(int damage)
    {
        _health = _health - damage;
    } 

    // Метод лечение цели
    public void Heal(int heal)
    {
        _health += heal;

        if(_health > maxHpMage)// В случае когда здоровье будет больше максимального
        {
            _health = maxHpMage;// Здоровье становится максимальным
        }
    }

    // Метод удаляет сокровище у выбранного мага и возвращает его
    // public List<TestCardController> DropTreasure ()
    // {
    //     if(_treasures.Count == 0)
    //     {
    //         return _treasures;
    //     }

    // }

    // Игрок выбирает сокровище, возвращает данное сокровище
    // Переписать полносттью функцию
    // public bool ChooseTreasure()
    // {
    //     //Если сокровища нет возвращаем false
    //     if(_treasures.Count == 0)
    //     {
    //         return false;
    //     }

    //     return true;

        
    // }

    // Выбор врага
    // Игрок выбирает врага
    // Метод возвращает выбранного мага
    // public TestMageController ChooseEnemyMage()
    // {

    //     return Mage;
    // }

    // Метод принимает игрока и его цель
    // Удаляет у цели выбранное сокровище
    // Добавляет выбранное сокровище игроку(ownerу карты заклинания)
    // public List<TestCardController> TakeEnemyTreasures (Mage owner, Mage chooseMage)
    // {
          

    // }

    // Метод для карты дискотечный
    // Во время выполнения карты, вызывается данная функция
    // Игрок выбирает карты заводилы или прихода
    // В карте происходит выполнение карты, которую вернул метод
    // public TestCardController ChooseCardInSpell()
    // {
    //     return spellCard;
    // }

}
