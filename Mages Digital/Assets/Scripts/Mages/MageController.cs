using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class MageController : MonoBehaviour
{


    [Header("Маг")]
    [SerializeField] Mage _mage;         // данные мага
    
    [Header("Статус")]
    [SerializeField] int _health    = 20;   // здоровье мага
    [SerializeField] int _medals    = 0;    // количество медалей недобитого колдуна
    [SerializeField] int _bonusDice = 0;    // бонусные кубики к броскам
    
    [Header("Рука")]
    [SerializeField] List<CardController> _treasures  = new List<CardController>(); // рука мага
    [SerializeField] List<CardController> _deads      = new List<CardController>();
    [SerializeField] List<CardController> _sources    = new List<CardController>();
    [SerializeField] List<CardController> _qualities  = new List<CardController>();
    [SerializeField] List<CardController> _deliveries = new List<CardController>();
    [SerializeField] List<CardController> _wildMagics = new List<CardController>();

    [Header("Заклинание")]
    [SerializeField] List<CardController> _spell = new List<CardController>(3) {null, null, null};
    
    [Header("Соседние маги")]
    [SerializeField] MageController _leftMage  = null; // левый соседний маг
    [SerializeField] MageController _rightMage = null; // правый соседний маг


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    int _startHealth        = 20;       // начальное здоровье мага
    int _healthMax          = 25;       // здоровье мага
    bool _spellsAreExecuted = false;    // походил ли маг
    bool _readyToExecute    = false;    // подготовил ли маг заклинание


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    AbstractPlayerController   _owner;     // игрок, управляющий магом
    MageIconController         _mageIcon;  // иконка мага


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public Mage mage              => _mage;
    public int  health            => _health;
    public int  medals            => _medals;
    public int  bonusDice         => _bonusDice;
    public bool spellsAreExecuted => _spellsAreExecuted;
    public bool readyToExecute    => _readyToExecute;

    public AbstractPlayerController owner => _owner;  
    public MageIconController    mageIcon => _mageIcon;
    public MageController        leftMage => _leftMage;
    public MageController       rightMage => _rightMage;

    public List<CardController> treasures  => _treasures;
    public List<CardController> deads      => _deads;
    public List<CardController> sources    => _sources;
    public List<CardController> qualities  => _qualities;
    public List<CardController> deliveries => _deliveries;
    public List<CardController> wildMagics => _wildMagics;

    public List<CardController> spell      => _spell;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public List<CardController> nonNullSpell => _spell.Where(card => card != null).ToList(); // карты заклинаний в спеле не равные null
    
    public bool isDead          => _health <= 0;  // мертв ли маг
    public bool spellIsReady    => nCardsInSpell > 0;   // готово ли заклинание
    public bool isGameWinner    => _medals == 3;
    public bool ownerIsBot      => _owner.isBot;
    
    public int  nCardsInSpell   => nonNullSpell.Count;  // количество карт в заклинании
    public int  spellInitiative => nonNullSpell.Sum(x => ((SpellCard) x.card).initiative); // сумарная инициатива спела
    public int  nCardsToDraw    => 8 - GetSpellsInHand().Count; // количество карт для добора из колоды
    public int  nSpellsInHand   => _sources.Count + _qualities.Count + _deliveries.Count + _wildMagics.Count; // количество карт заклинаний в руке


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        _owner    = gameObject.GetComponent<AbstractPlayerController>();
        _mageIcon = gameObject.GetComponentInChildren<MageIconController>();
    }

    // TEST
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!ownerIsBot)
                TakeDamage(_healthMax);
        }
    }
    // TEST


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // ДОБАВИТЬ КАРТУ В РУКУ МАГА
    // устанавливает владельца карты
    // добавляет карту в нужный список руки
    // вызывает у владельца мага OnCardAddedToHand
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

    // ДОБАВИТЬ КАРТУ В ЗАКЛИНАНИЕ
    // добавляет карту в заклинание на определенное место
    // если в заклинании уже лежит карта на этом месте, то она автоматически будет заменена на новую
    public IEnumerator AddToSpell(CardController cardToAdd, Order order, bool backToHand = true, bool ownerReaction = true)
    {
        cardToAdd.SetUndiscoverable();

        // индекс расположения карты в заклинании
        int spellCardIndex = GetSpellIndexOfOrder(order);

        // текущая карта в заклинании
        CardController backToHandCard = _spell[spellCardIndex];
    
        // вернуть старую карту в заклинании обратно в руку
        if (backToHandCard != null && backToHand)
        {
            backToHandCard.SetUndiscoverable();
            yield return AddCard(backToHandCard);
            backToHandCard.SetDiscoverable();
        }

        // удалить карту для заклинания из руки и добавить в заклинание
        List<CardController> hand = GetHandOfCardType(cardToAdd.card);
        hand.Remove(cardToAdd);
        _spell[spellCardIndex] = cardToAdd;
        cardToAdd.SetStateToInSpell();

        if (ownerReaction)
            yield return owner.OnCardAddedToSpell(cardToAdd, order);

        cardToAdd.spellOrder   = order;

        cardToAdd.SetDiscoverable();
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
        backToHandCard.SetStateToInHand();
    }

    public IEnumerator AppendToSpell(CardController card)
    {
        card.SetStateToInSpell();
        _spell.Add(card);
        yield return owner.ShowSpellToAll();
    }

    public IEnumerator PassSpellOfOrderTo(MageController mage, Order order)
    {
        List<CardController> cards = nonNullSpell.FindAll(card => card.GetSpellCard().order == order || card.spellOrder == order);
        print(cards.Count);
        foreach (CardController card in cards)
        {
            RemoveCard(card);
            card.SetOwner(mage);
            card.SetVisible(true);
            yield return mage.AppendToSpell(card);
            yield return card.PositionFrontUp();
        }
        yield break;
    }

    // выполнить заклинание
    public IEnumerator ExecuteSpells()
    {
        yield return owner.ShowSpellToAll();

        // замена шальной магии
        List<CardController> spellWildMagics = nonNullSpell.Where(card => card.GetSpellCard().order == Order.WILDMAGIC).ToList();
        
        // выполнение спелов карт
        List<CardController> currentNonNullSpell = new List<CardController>(nonNullSpell);
        int nExecutedSpells = 0;
        while (nCardsInSpell > nExecutedSpells && !GameManager.instance.isAliveOnlyOne)
        {
            yield return OneSpellCardExecution(nonNullSpell[nExecutedSpells]);
            nExecutedSpells++;
            if (isDead) break;
        }
        

        if (spellWildMagics.Count > 0)
            spellWildMagics.ForEach(wildMagic => Destroy(wildMagic.gameObject));
        
        UnreadyToExecute();
        
        if (!isDead)
            yield return owner.HideSpellFromAll();
    }

    public IEnumerator OneSpellCardExecution(CardController card)
    {
        yield return card.Highlight(true);

        if (card.GetSpellCard().order == Order.WILDMAGIC)
        yield return GameManager.instance.spellsDeck.ReplaceWildMagic(card);

        yield return card.ExecuteSpell();

        yield return card.Highlight(false);
    }


    public IEnumerator ExecuteDeads()
    {
        List<CardController> deadsCopy = new List<CardController>(_deads);
        foreach (CardController deadCard in deadsCopy.FindAll(card => card != null))
        {
            yield return owner.ShowCardToAll(deadCard, highlight: true);
            yield return deadCard.ExecuteSpell();
            deadCard.ToFold();
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // сбросить подготовленное заклинание (отправить в сброс)
    public void DropSpell()
    {
        List<CardController> spellCopy = new List<CardController>(_spell);
        _spell = new List<CardController>(3) {null, null, null};
        spellCopy.FindAll(card => card != null).ForEach(card => card.ToFold());
        if (!ownerIsBot)
            StartCoroutine(owner.OnSpellDrop());
    }

    // подготовить мага к выполнению заклинания
    public void ReadyToExecute()
    {
        _readyToExecute    = true;
        _spellsAreExecuted = false;
    }

    // отменить готовность мага к выполнению заклинания
    public void UnreadyToExecute()
    {
        _readyToExecute    = false;
        _spellsAreExecuted = true;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // вернуть список всех карт, пренадлежащих магу
    public List<CardController> GetAllCards()
    {
        List<CardController> allCards = new List<CardController>();
        allCards.AddRange(GetSpellsInHand());
        allCards.AddRange(GetBonusCards());
        allCards.AddRange(nonNullSpell);
        return allCards;
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

    // вернуть список всех бонусных карт мага
    public List<CardController> GetBonusCards()
    {
        List<CardController> bonusCards = new List<CardController>();
        bonusCards.AddRange(_treasures);
        bonusCards.AddRange(_deads);
        return bonusCards;
    }

    // вернуть список трех бонусных карт для вывода информации о них на экране
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // BUG Не верная индексация
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public List<CardController> GetBonusInfo(int indexOffset = 0)
    {
        List<CardController> bonusInfo = new List<CardController>(3) {null, null, null};
        List<CardController> bonusCards = GetBonusCards();
        for (int i = 0; i < 3; i++)
        {
            if (i < bonusCards.Count)
            {
                int cardsIndex = i - indexOffset;
                cardsIndex = (-indexOffset < 0) ? bonusCards.Count -  cardsIndex : cardsIndex;

                int infoIndex = i - indexOffset;
                infoIndex = (-indexOffset < 0) ? bonusInfo.Count - infoIndex : infoIndex;

                bonusInfo[infoIndex % bonusInfo.Count] = bonusCards[cardsIndex % bonusCards.Count];
            }
        }
        return bonusInfo;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // установить все карты мага (не)доступными для взаимодействия с мышью
    public void SetAllDiscoverable(bool discoverable)
    {
        if (discoverable)
            GetAllCards().ForEach(card => card.SetDiscoverable());
        else
            GetAllCards().ForEach(card => card.SetUndiscoverable());
    }

    // добавить карту в определенную часть руки
    // если добавляется заклинание, то сортировать обновленный список
    public void AddCardToHand(List<CardController> hand, CardController cardController)
    {
        hand?.Add(cardController);
        cardController.SetStateToInHand();
        if (cardController.card != null && cardController.isSpell)
        {
            hand.Sort((c1, c2) => ((SpellCard)c1.card).sign.CompareTo(((SpellCard)c2.card).sign));
        }
    }

    // удалить карту
    public void RemoveCard(CardController card)
    {
        card.RemoveOwner();
        List<CardController> hand = GetHandOfCardType(card.card);
        if (hand.Contains(card))
        {
            hand.Remove(card);
        }
        else if (_spell.Contains(card))
        {
            int index = _spell.IndexOf(card);
            if (index < 3)
                _spell[index] = null;
            else
                _spell.RemoveAt(index);
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // получить урон
    public void TakeDamage(int damage, CardController damageSource = null)
    {
        if (!isDead)
        {
            _health = Mathf.Clamp(_health - damage, 0, _healthMax);
            StartCoroutine(_mageIcon.OnTakeDamage(damageSource));
            if (isDead)
                OnDeath();
        }
    }

    // захилиться
    public void Heal(int heal)
    {
        _health = Mathf.Clamp(_health + heal, 0, _healthMax);
    }

    // реакция на победу в турнире
    public void OnTournamentWon()
    {
        _medals += 1;
    }

    // реакция на смерть
    public void OnDeath()
    {
        GetAllCards().ForEach(card => card.ToFold(destroy: true));
        StartCoroutine(GameManager.instance.deadsDeck.PassCardsTo(this, 1));
        _mageIcon.OnTakeDamage();
    }

    // выставить начальные для турнира параметры
    public void ResetMage()
    {
        _health = _startHealth;
        _mageIcon.OnReset();
        if (!ownerIsBot)
            _owner.OnMageReset();
    }


    public bool HasCardOfOrderInSpell(Order order)
    {
        return nonNullSpell.Count(card => card.GetSpellCard().order == order) > 0;
    }



////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
        return GetSpellHandOfOrder(spellCard.order);
    }

    // вернуть определенный список по порядку карты заклинания
    public List<CardController> GetSpellHandOfOrder(Order order)
    {
        List<CardController> hand = null;
        switch (order)
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


}
