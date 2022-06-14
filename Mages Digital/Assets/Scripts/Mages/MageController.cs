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

    [Header("Действия заклинаний")]
    [SerializeField] int  _bonusDice = 0;    // бонусные кубики к броскам
    [SerializeField] bool _isWildMagicInSpell = false;
    
    [Header("Рука")]
    [SerializeField] List<BonusCardController> _treasures  = new List<BonusCardController>(); // рука мага
    [SerializeField] List<BonusCardController> _deads      = new List<BonusCardController>();
    [SerializeField] List<SpellCardController> _sources    = new List<SpellCardController>();
    [SerializeField] List<SpellCardController> _qualities  = new List<SpellCardController>();
    [SerializeField] List<SpellCardController> _deliveries = new List<SpellCardController>();
    [SerializeField] List<SpellCardController> _wildMagics = new List<SpellCardController>();

    [Header("Заклинание")]
    [SerializeField] List<SpellCardController> _spell = new List<SpellCardController>(3) {null, null, null};
    
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
    public bool spellsAreExecuted => _spellsAreExecuted;
    public bool readyToExecute    => _readyToExecute;

    public AbstractPlayerController owner => _owner;  
    public MageIconController    mageIcon => _mageIcon;
    public MageController        leftMage => _leftMage;
    public MageController       rightMage => _rightMage;

    public List<BonusCardController> treasures  => _treasures;
    public List<BonusCardController> deads      => _deads;
    public List<SpellCardController> sources    => _sources;
    public List<SpellCardController> qualities  => _qualities;
    public List<SpellCardController> deliveries => _deliveries;
    public List<SpellCardController> wildMagics => _wildMagics;

    public List<SpellCardController> spell      => _spell;

    public int  bonusDice 
    {
        get => _bonusDice;
        set => _bonusDice = value;
    }
    public bool isWildMagicInSpell => _isWildMagicInSpell;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public List<SpellCardController> nonNullSpell => _spell.Where(card => card != null).ToList(); // карты заклинаний в спеле не равные null
    
    public bool isDead          => _health <= 0;  // мертв ли маг
    public bool spellIsReady    => nCardsInSpell > 0;   // готово ли заклинание
    public bool isGameWinner    => _medals == 3;
    
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
            if (_owner is PlayerController)
                TakeDamage(_healthMax);
        }
    }
    // TEST


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // ДОБАВИТЬ КАРТУ В РУКУ МАГА
    // устанавливает владельца карты
    // добавляет карту в нужный список руки
    // вызывает у владельца мага OnCardAddedToHand
    public IEnumerator AddCard(CardController card)
    {
        card.SetOwner(this);
        if (card.card != null)
        {
            var hand = GetHandOfCardType(card.card);
            AddCardToHand(hand, card);
            yield return owner.OnCardAddedToHand(card);
        }
        yield break;
    }

    // ДОБАВИТЬ КАРТУ В ЗАКЛИНАНИЕ
    // добавляет карту в заклинание на определенное место
    // если в заклинании уже лежит карта на этом месте, то она автоматически будет заменена на новую
    public IEnumerator AddToSpell(SpellCardController cardToAdd, Order order, bool backToHand = true, bool ownerReaction = true)
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
        var hand = GetHandOfCardType(cardToAdd.card);
        hand.Remove(cardToAdd);
        _spell[spellCardIndex] = cardToAdd;
        cardToAdd.SetStateToInSpell();

        if (ownerReaction)
            yield return owner.OnCardAddedToSpell(cardToAdd, order);

        cardToAdd.spellOrder   = order;

        cardToAdd.SetDiscoverable();
    }

    // добавить шальную магию к заклинанию
    public IEnumerator AddWildMagicToSpell(SpellCardController cardToAdd)
    {
        GameManager.instance.SetChoosingState();
        yield return owner.ChooseOrder();
        if (owner.chosenOrder != Order.WILDMAGIC)
            yield return AddToSpell(cardToAdd, owner.chosenOrder);
        if (GameManager.instance.isChoosingState)
            GameManager.instance.ReturnToPrevState();
    }

    // вернуть карту обратно в руку
    public IEnumerator SpellCardBackToHand(CardController backToHandCard, Order order)
    {

        int spellCardIndex = GetSpellIndexOfOrder(order);
        _spell[spellCardIndex] = null;
        yield return AddCard(backToHandCard);
        backToHandCard.SetStateToInHand();
    }

    public IEnumerator AppendToSpell(SpellCardController card)
    {
        card.SetStateToInSpell();
        _spell.Add(card);
        yield return owner.ShowSpellToAll();
    }

    public IEnumerator PassSpellOfOrderTo(MageController mage, Order order)
    {
        List<SpellCardController> cards = nonNullSpell.FindAll(card => card.GetSpellCard().order == order || card.spellOrder == order);
        print(cards.Count);
        foreach (SpellCardController card in cards)
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
        List<SpellCardController> spellWildMagics = nonNullSpell.Where(card => card.GetSpellCard().order == Order.WILDMAGIC).ToList();
        foreach (SpellCardController wildMagic in spellWildMagics)
        {
            yield return wildMagic.Highlight(true);
            yield return GameManager.instance.spellsDeck.ReplaceWildMagic(wildMagic);
            yield return wildMagic.Highlight(false);

            _isWildMagicInSpell = true;
        }

        if (spellWildMagics.Count > 0)
            spellWildMagics.ForEach(wildMagic => Destroy(wildMagic.gameObject));
        
        // выполнение спелов карт
        List<CardController> currentNonNullSpell = new List<CardController>(nonNullSpell);
        int nExecutedSpells = 0;
        while (nCardsInSpell > nExecutedSpells && !GameManager.instance.isAliveOnlyOne)
        {
            yield return OneSpellCardExecution(nonNullSpell[nExecutedSpells]);
            nExecutedSpells++;
            if (isDead) break;
        }
        
        UnreadyToExecute();
        
        if (!isDead)
            yield return owner.HideSpellFromAll();
        
        _isWildMagicInSpell = false;
    }

    public IEnumerator OneSpellCardExecution(CardController card)
    {
        yield return card.Highlight(true);
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
        List<SpellCardController> spellCopy = new List<SpellCardController>(_spell);
        _spell = new List<SpellCardController>(3) {null, null, null};
        spellCopy.FindAll(card => card != null).ForEach(card => card.ToFold());
        StartCoroutine(owner.OnSpellDrop());
    }

    public void DropSpellOfOrder(Order order)
    {
        List<SpellCardController> spellCopy = new List<SpellCardController>(_spell);
        for (int i = 0; i < spellCopy.Count; i++)
        {
            SpellCardController card = spellCopy[i];
            if (card != null)
            {
                if (card.GetSpellCard()?.order == order || card.spellOrder == order)
                {
                    spellCopy[i] = null;
                    card.SetVisible(true);
                    card.ToFold();
                    StartCoroutine(owner.HideSpellFromAll());
                }
            }
        }
        _spell = spellCopy;
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
    public List<SpellCardController> GetSpellsInHand()
    {
        List<SpellCardController> spellCards = new List<SpellCardController>();
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

    public List<SpellCardController> GetCardsOfOrderInSpell(Order order)
    {
        List<SpellCardController> cards = new List<SpellCardController>();
        foreach (SpellCardController card in nonNullSpell)
        {
            if (card.order == order || card.spellOrder == order)
                cards.Add(card);
        }
        return cards;
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
    public void AddCardToHand(IList hand, CardController cardController)
    {
        print(hand);
        hand?.Add(cardController);
        cardController.SetStateToInHand();
        if (cardController.card != null && cardController.isSpell)
        {
            
            ((List<SpellCardController>) hand).Sort((c1, c2) => ((SpellCard)c1.card).sign.CompareTo(((SpellCard)c2.card).sign));
        }
    }

    // удалить карту
    public void RemoveCard(CardController card)
    {
        card.RemoveOwner();
        var hand = GetHandOfCardType(card.card);
        if (hand.Contains(card))
        {
            hand.Remove(card);
        }
        else if (_spell.Contains(card))
        {
            int index = _spell.IndexOf((SpellCardController) card);
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
        StartCoroutine(_mageIcon.OnHeal());
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

    public void OnRoundStart()
    {
        _bonusDice = 0;
        _isWildMagicInSpell = false;
    }

    // выставить начальные для турнира параметры
    public void ResetMage()
    {
        _health    = _startHealth;
        _mageIcon.OnReset();
        _owner.OnMageReset();
    }

    public void OnChangeOrder()
    {
        StartCoroutine(_mageIcon.OnChangeOrder());
    }


    public bool HasCardOfOrderInSpell(Order order)
    {
        return nonNullSpell.Count(card => card.GetSpellCard().order == order) > 0;
    }



////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // вернуть определенную руку по типу карты
    public IList GetHandOfCardType(Card card)
    {
        switch (card.cardType)
        {
            case CardType.SPELL:
                return GetSpellHandOfCardOrder(card);
            case CardType.TREASURE:
                return _treasures;
            case CardType.DEAD:
                return _deads;
        }
        return null;
    }

    // вернуть определенный список по порядку карты заклинания
    public List<SpellCardController> GetSpellHandOfCardOrder(Card card)
    {
        SpellCard spellCard = (SpellCard) card;
        return GetSpellHandOfOrder(spellCard.order);
    }

    // вернуть определенный список по порядку карты заклинания
    public List<SpellCardController> GetSpellHandOfOrder(Order order)
    {
        List<SpellCardController> hand = null;
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
