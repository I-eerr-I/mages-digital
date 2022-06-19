using UnityRandom = UnityEngine.Random;
using Random = System.Random;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class CardController : MonoBehaviour
{
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ STATIC AND OTHER VARIABLES ]

    public Random random = new Random();

    // словарь для получения цвета подсветки карты в зависимости от порядка в заклинании
    public static Dictionary<Order, Color> SPELL_CARD_MAIN_COLOR = new Dictionary<Order, Color>()
    {
        { Order.SOURCE,      new Color(1.0f, 0.8427867f, 0.0f) },
        { Order.QUALITY,     new Color(0.9960785f, 0.4745098f, 0.0f) },
        { Order.DELIVERY,    new Color(0.9960785f, 0.0509804f, 0.1529412f) },
        { Order.WILDMAGIC,   new Color(0.0f, 0.60784f, 0.5843f) }
    };

    // словарь для получения цвета подсветки карты в зависимости от ее типа
    public static Dictionary<CardType, Color> CARD_MAIN_COLOR = new Dictionary<CardType, Color>()
    {
        { CardType.SPELL,    new Color(1.0f, 0.0f, 0.0f) },
        { CardType.TREASURE, new Color(1.0f, 0.8427867f, 0.0f) },
        { CardType.DEAD,     new Color(0.0f, 0.6603774f, 0.4931389f) }
    };

    public const string POSITION_LEFT  = "left";  // Позиция слева
    public const string POSITION_RIGHT = "right"; // Позиция справа

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ PRIVATE VARIABLES ]

    [SerializeField] Card _card;                 // данные карты
    [SerializeField] MageController _owner;      // маг владеющий картой
    [SerializeField] DeckController _sourceDeck; // колода, откуда была создана карта
    [SerializeField] bool _visible = true;       // видимость карты


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    float _cardFlippingTime          = 0.15f; // время переворота карты
    float _cardShowInfoWaitTime      = 1.5f;  // сколько держать курсор на карте, чтобы показать ее информацию
    float _cardHighlightDeltaY       = 1.0f;  // насколько поднять карту вверх для выделения
    float _cardHighlightTime         = 1.0f;  // время поднятия карты для выделения
    float _cardHighlightLightDelta   = 2.0f;  // увеличение дальности света (яркости) при выделении
    float _cardFlyOutY               = 10.0f; // глобальная координата Y при вылете карты за экран
    float _cardFlyOutXMin            = -3.0f; // минимальное значение X при вылете карты за экран
    float _cardFlyOutXMax            = 3.0f;  // максимальное значение X при вылете карты за экран
    float _cardFlyOutTime            = 1.0f;  // время вылета карты за экран


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    bool _ownerGoesFirst      = false;
    bool _discoverable        = true;               // можно ли взаимодействовать с картой
    bool _isMouseOver         = false;              // находится ли курсор на карте
    float _mouseOverTime      = 0.0f;               // время, прошедшее с момента наведения на карту
    int _bonusInfoIndexOffset = 0;                  // нужна для перебора карт сокровищ при наведении на них
    Order _spellOrder         = Order.WILDMAGIC;    // порядок в заклинании (нужен для шальной магии)
    CardState cardState       = CardState.NO_OWNER; // состояние карты
    bool _choosingCardState   = false;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    GameObject        _middle;              // середина карты
    Light             _middleLight;         // источник света карты
    MeshRenderer      _middleMeshRenderer;  // рендер середины карты
    BoxCollider       _middleBoxCollider;   // коллайдер середины карты
    SpriteRenderer    _frontSpriteRenderer; // рендер передней части карты
    SpriteRenderer    _backSpriteRenderer;  // рендер задней части (рубашки) карты
    OutlineController _outlineController;   // управление подсветкой карты
    
    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ PUBLIC VARIABLES ]

    public Card           card       => _card;
    public MageController owner      => _owner;
    public bool ownerGoesFirst => _ownerGoesFirst;

    public Order spellOrder
    {
        get => _spellOrder;
        set => _spellOrder = value;
    }

    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public GameManager gm => GameManager.instance;
    public CardEffectsManager effects => CardEffectsManager.instance;

    public bool  isSpell         => (_card != null) && (_card.cardType == CardType.SPELL); // является карта картой заклинания
    public bool  inHand          => cardState == CardState.IN_HAND;     // находится ли карта в руке
    public bool  inSpell         => cardState == CardState.IN_SPELL;    // находится ли карта в заклинании
    public bool  withOwner       => cardState != CardState.NO_OWNER;    // есть ли у карты владелец
    public bool  withSourceDeck  => _sourceDeck != null;                // создана ли карта колодой
    public float cardSizeX       => _middle.transform.localScale.x;     // ширина карты

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ UNITY PIPELINE ]

    void Awake()
    {
        _frontSpriteRenderer     = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer      = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        
        _middle = transform.GetChild(2).gameObject;
        _middleLight        = _middle.GetComponent<Light>();
        _outlineController  = _middle.GetComponent<OutlineController>();
        _middleMeshRenderer = _middle.GetComponent<MeshRenderer>();
        _middleBoxCollider  = _middle.GetComponent<BoxCollider>();
    }


    void Start()
    {
        SetVisible(_visible);

        if (isSpell)
            _ownerGoesFirst = GetSpellCard().spell == "Neterpeliviu";
    }


    void Update()
    {
        if (_isMouseOver && !isSpell)
        {
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(mouseScroll) > 0.0f)
            {
                int addToOffset = (mouseScroll > 0.0f) ? 1 : -1;
                _bonusInfoIndexOffset += addToOffset;
            }
        }
    }

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ CARD LOGIC ]

    // добавить карту в заклинание
    void AddToSpell()
    {
        Order order = ((SpellCard) card).order;
        if (order == Order.WILDMAGIC)
            StartCoroutine(owner.AddWildMagicToSpell(this));
        else
            StartCoroutine(owner.AddToSpell(this, order));
    }

    // вернуть карту в руку из заклинания
    void BackToHand()
    {
        StartCoroutine(owner.SpellCardBackToHand(this, spellOrder));
    }

    // настроить карту
    public void SetupCard(Card card, DeckController deck = null, Sprite back = null)
    {
        // данные карты
        _card = card;
        // колода карты
        _sourceDeck = deck;
        // передняя часть карты
        _frontSpriteRenderer.sprite = card.front;
        // задняя часть карты
        if (back != null)
            _backSpriteRenderer.sprite  = back;
        // подсветка карты
        Color cardColor;
        if (card.cardType == CardType.SPELL)
            cardColor = SPELL_CARD_MAIN_COLOR[((SpellCard) card).order];
        else
            cardColor = CARD_MAIN_COLOR[card.cardType];
        _outlineController.SetColor(cardColor);
    }

    // отправить карту в сброс
    public void ToFold(bool destroy = true)
    {
        if (withOwner)
        {
            owner.RemoveCard(this);
            RemoveOwner();
        }
        if (_sourceDeck != null) 
            _sourceDeck.AddCardToFold(this);
        StartCoroutine(FlyOutAndDestroy(destroy: destroy));
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // вылет карты за экран и уничтожение
    IEnumerator FlyOutAndDestroy(bool destroy = true)
    {
        float x = (float) random.NextDouble() * (_cardFlyOutXMax - _cardFlyOutXMin) + _cardFlyOutXMin;
        
        Hashtable hashtable = new Hashtable();
        hashtable.Add("y", _cardFlyOutY);
        hashtable.Add("x", x);
        hashtable.Add("time", _cardFlyOutTime);
        if (destroy)
            hashtable.Add("oncomplete", "DestoyObject");
        
        iTween.MoveTo(gameObject, hashtable);
        
        yield break;
    }

    // перевернуть карту лицевой строной вверх
    public IEnumerator PositionFrontUp()
    {
        Vector3 rotation;
        if (inHand)
            rotation = new Vector3(0.0f, 0.0f, 0.0f);
        else
            rotation = new Vector3(90.0f, 0.0f, 0.0f);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", _cardFlippingTime, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }

    // перевернуть карту рубашкой вверх
    public IEnumerator PositionBackUp()
    {
        Vector3 rotation;
        if (inHand)
            rotation = new Vector3(0.0f, 180.0f, 0.0f);
        else
            rotation = new Vector3(-90.0f, 0.0f, 0.0f);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", _cardFlippingTime, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }

    // выделение карты (обычно в заклинании)
    public IEnumerator Highlight(bool highlight)
    {
        if (_outlineController != null)
            _outlineController.enabled = !highlight;

        _middleLight.enabled  = highlight;
        _middleLight.range   += (highlight ? 1.0f : -1.0f) * _cardHighlightLightDelta;
        
        float y = transform.position.y;
        y += (highlight ? 1.0f : -1.0f) * _cardHighlightDeltaY;
        
        iTween.MoveTo(gameObject, iTween.Hash("y", y, "time", _cardHighlightTime));
        
        yield return new WaitForSeconds(_cardHighlightTime);
    }

    // выполнение заклинания карты
    public IEnumerator ExecuteSpell()
    {
        if (card.spell != "")
            yield return StartCoroutine(card.spell);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // триггер при наведении курсора на карту
    public void OnMouseOverTrigger()
    {
        if (_discoverable)
        {
            _isMouseOver = true;
            if (withOwner)
            {
                if (isSpell)
                {
                    SelectCard(true);
                    ShowCardInfo();
                }
                else
                {
                    ShowBonusCardInfo();
                }
            }
        }
    }

    // триггер при выходе курсора из области карты
    public void OnMouseExitTrigger()
    {
        _isMouseOver = false;
        if (isSpell)
            HideCardInfo();
        else
            HideBonusCardInfo();
        if (_discoverable)
        {
            if (withOwner)
                if (isSpell)
                    SelectCard(false);
        }
    }

    // триггер при клике на карту
    public void OnMouseDownTrigger()
    {
        if (_discoverable)
        {
            if (gm.isSpellCreationState)
            {
                if (withOwner)
                {
                    if (isSpell)
                    {
                        if (inHand)
                        {
                            AddToSpell();   
                        }
                        else if (inSpell)
                        {
                            BackToHand();
                        }
                    }
                }
            }
            else if (gm.isChoosingState)
            {
                gm.StopChoosing();
            }
        }

        if (_choosingCardState)
        {
            GameManager.instance.player.chosenCard = this;
        }
    }

    public void OnChoosingCardState()
    {
        _choosingCardState = true;
    }

    public void OnChoosingCardStateEnd()
    {
        _choosingCardState = false;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // показать информацию о карте
    void ShowCardInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= _cardShowInfoWaitTime)
            UIManager.instance.ShowCardInfo(this, true);
    }

    // показать информацию о бонусных картах в руке
    void ShowBonusCardInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= _cardShowInfoWaitTime)
            UIManager.instance.ShowBonusInfo(owner.GetBonusInfo(indexOffset: _bonusInfoIndexOffset));
    }

    // спрятать информацию о карте
    void HideCardInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowCardInfo(this, false);
    }

    // спрятать информацию о бонусных картах в руке
    void HideBonusCardInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowBonusInfo(null, false);
    }

    // выделить карту
    public void SelectCard(bool select)
    {
        owner.owner.OnSpellCardSelected(this, select);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // установить видимость карты
    public void SetVisible(bool visible)
    {
        _visible = visible;
        _frontSpriteRenderer.enabled = visible;
        _backSpriteRenderer.enabled  = visible;
        _middleMeshRenderer.enabled  = visible;
        _middleBoxCollider.enabled   = visible;
        _outlineController.enabled   = visible;
    }

    // установить карту доступной для нажатия и наведения мышью
    public void SetDiscoverable()
    {
        _discoverable = true;
        _outlineController.enabled = true;
    }

    // установить карту недоступной для нажатия и наведения мышью
    public void SetUndiscoverable()
    {
        _discoverable = false;
        _outlineController.enabled = false;
    }

    // установить владельца карты
    public void SetOwner(MageController owner)
    {
        _owner = owner;
    }

    // удалить владельца карты
    public void RemoveOwner()
    {
        _owner = null;
        SetStateToNoOwner();
    }

    // получить SpellCard версию данных карты
    public SpellCard GetSpellCard()
    {
        if (card.cardType == CardType.SPELL)
            return (SpellCard) card;
        return null;
    }


    public void SetStateToNoOwner()
    {
        cardState = CardState.NO_OWNER;
        _outlineController.SetProperties(true, false);
    }


    public void SetStateToInSpell()
    {
        cardState = CardState.IN_SPELL;
        _outlineController.SetProperties(true, false);
    }


    public void SetStateToInHand()
    {
        cardState = CardState.IN_HAND;
        if (_owner is PlayerController)
            _outlineController.SetProperties(false, true);
    }


    public void DestoyObject()
    {
        Destroy(gameObject);
    }

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ SPELLS ]

    #region [ CARD SPELLS ADDITIONAL METHODS ]


    public enum TargetType
    {
        RANDOM,
        LOW_HP,
        HIGH_HP
    }


    public IEnumerator OnDiceRoll(List<int> rolls, bool showBonus = true)
    {
        if (owner.bonusDice > 0 && showBonus)
            owner.mageIcon.ShowInfoText($"+{owner.bonusDice}");

        yield return effects.RollDice(rolls);
        if (showBonus)
            owner.mageIcon.HideInfoText();
    }

    public IEnumerator OnRandomEnemy(MageController enemy)
    {
        yield return effects.HighlightEnemiesOfMage(owner, 5);

        yield return enemy.mageIcon.HighlightForSomeTime(0.5f);
    }

    public List<int> RollDice(int numberDice)
    {
        List<int> rolls = new List<int>();
        for (int i = 0; i < numberDice;  i++)
        {
            int result = random.Next(1, 7); // Бросок кубика
            rolls.Add(result);
        }

        return rolls;
    }

    public int NumberDice(Sign sign)
    {
        int numberDice = 0; // Кол-во кубиков

        // Цикл нахождения кол-во одинаковых знаков карты заклинания => кол-во кубиков 
        List<CardController> currentSpell = owner.nonNullSpell.ToList();
        numberDice = currentSpell.Count(spellCard => spellCard.GetSpellCard().sign == sign);
        numberDice += owner.additionalSigns.Count(addSign => addSign == sign);

        numberDice += owner.bonusDice;
        
        return numberDice;
    }

    // Урон магу (Урон, Цель)
    public IEnumerator DamageToTarget(int damage, MageController target)
    {
        target.TakeDamage(damage, this);
        yield return effects.Attack(transform.position, target.mageIcon.transform.position, this);
        yield return new WaitForSeconds(1.0f);
    } 

    // Лечение мага (Жизни, Цель)
    public IEnumerator HealToTarget(int healHp, MageController target)
    {
        target.Heal(healHp);
        yield break;
    }

    // Урон нескольким магам (Урон, Лист Целей)
    public IEnumerator DamageToTargets(int damage, List<MageController> listTargets)
    {
        foreach(MageController target in  listTargets)
            yield return DamageToTarget(damage, target);

        yield break;
    }

    // Метод возвращает цель, если цель мертва берет следующую цель или слева или справа
    public MageController IsDeadTargetLeftOrRight(MageController target, string position)
    {
        while (target.isDead)// Пока цель мертва
        {
            if(position == POSITION_LEFT)
                target = target.leftMage; // Берем следующего левого мага
            else if(position == POSITION_RIGHT)
                target = target.rightMage; // Берем следующего левого мага
        }
        return target;
    }

    // Урон магу и его соседям слева и справа
    public IEnumerator DamageToTargetsNeighbors(int damage, int damageNeighbors , List<MageController> listTargets)
    {
        foreach(MageController target in  listTargets)
        {
            yield return DamageToTarget(damage, target);
            
            yield return DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));
            yield return DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));
        }
    } 

    public List<MageController> FindTargets(TargetType targetType)
    {
        switch (targetType)
        {
            case TargetType.HIGH_HP:
                return HighHpTargets();
            case TargetType.LOW_HP:
                return LowHpTargets();
            case TargetType.RANDOM:
                return RandomEnemy();
        }
        return null;
    }

    // Метод возвращающий случайного врага(принимает владельца)
    public List<MageController> RandomEnemy()
    {
        // Создаем список без владельца карты
        List<MageController> magesWithoutOwner = AllEnemies();
        // Рандомим индекс
        int index = UnityRandom.Range(0, magesWithoutOwner.Count);
        // Возвращаем мага из списка
        List<MageController> targets = new List<MageController>() { magesWithoutOwner[index] };
        return targets;
    }

    // Метод возвращает лист целей, Нахождение хилого мага из списка живых
    public List<MageController> LowHpTargets()
    {
        List<MageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        List<MageController> listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    // Метод возвращает лист целей, Нахождение живучего мага из списка живых
    public List<MageController> HighHpTargets()
    {
        
        List<MageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        List<MageController> listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    //Метод возвращает лист целей, нахождение каждого врага
    public List<MageController> AllEnemies()
    {
        List<MageController> magesWithOutOwner = gm.aliveMages.FindAll(mage => owner !=mage);

        return magesWithOutOwner;
    }

    // Увеличение урона от кол-ва одинаковых знаков (некоторые карты)
    public int BuffDamageSign(Sign sign)
    {
        int buffDamage = 0; // Стартовое увеличение урона
        // Цикл нахождения кол-ва одинаковых знаков карты заклинания => Увеличение урона
        foreach (CardController spellCard in owner.nonNullSpell)
        {
            if(spellCard.GetSpellCard().sign == sign)
                buffDamage+= 1;
        }
        return buffDamage;
    }

    // Забрать карту определенного порядка  
    public IEnumerator StealCardFromSpell(Order order, List<MageController> listTargets)
    {
        List<MageController> targets = listTargets.FindAll(mage => mage.HasCardOfOrderInSpell(order));
        foreach (MageController target in targets)
            yield return target.PassSpellToSpellOfOrderTo(owner, order);
    }

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ SPELLS ]

    // Драконий сундук
    public IEnumerator  DrakoniuSundyk()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        owner.resultDice  = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 2;}
        else
        {
            damage = 3;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        // Нахождение магов без сокровища
        List<MageController> listTargets = gm.aliveMages.FindAll(mage => mage != owner && mage.treasures.Count == 0);

        yield return DamageToTargets(damage, listTargets);

        yield break;
    }

    // Договор с Дьяволом
    public IEnumerator DogovorSDuavolom()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
                
        int damage = 0;
        if   (owner.resultDice <= 4){damage = 1;}
        else                  {damage = 2;}

        List<MageController> listTargets = FindTargets(TargetType.HIGH_HP);
        yield return DamageToTargets(damage, listTargets);

        if (owner.resultDice > 9)
        {
            yield return Highlight(false);
            yield return StealCardFromSpell(Order.DELIVERY, listTargets);
        }

        yield break;
    }

    // Кислотный слив
    public IEnumerator KislotnuiSliv()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей
        
        int damage;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 2;}
        else
        {
            damage = 4;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));

        yield return DamageToTargets(damage, listTargets);
        
        yield break;
    }

    // Экзорцизм
    public IEnumerator Exorcism()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        List<MageController> listTargets      = new List<MageController>(); // Лист целей
        List<MageController> listTargetsDeads = new List<MageController>(); // Лист целей с жетоном недобитого колдуна
        
        int damageDeads = 0; // Урон недобитым магам

        foreach(MageController mage in AllEnemies())
        {
            if(mage.medals != 0)
                listTargetsDeads.Add(mage); // сохранение магов с жетоном недобитого колдуна
            else
                listTargets.Add(mage); // без жетона                 
        }

        int damage = 0;
        if      (owner.resultDice <= 4)
        {
            damage = 1;
            yield return DamageToTarget(damage, owner);
        }
        else if (owner.resultDice <= 9)
        {
            damage = 2;
            damageDeads = 4;
            yield return DamageToTargets(damage, listTargets);
            yield return DamageToTargets(damageDeads, listTargetsDeads);
        }
        else
        {
            damage = 4;
            damageDeads = 8;
            yield return DamageToTargets(damage, listTargets);
            yield return DamageToTargets(damageDeads, listTargetsDeads);
        }
        
        yield break;
    }

    // Петушок
    public IEnumerator Petyshok()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 1;}
        else                     {damage = 7;}

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        yield break;
    }

    // Шарах Молнии
    public IEnumerator SharahMolnuu()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));

        if (IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT).leftMage != owner)
            listTargets.Add(owner.leftMage.leftMage);

        yield return DamageToTargets(damage, listTargets);
        yield break;
    }

    // Химерический Хохот
    public IEnumerator HimericheskiuXoxot()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     {damage = 4;}
        
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT)); //Правый маг
        yield break;
    }

    // Ядерный синтез
    public IEnumerator YaderniuSintez()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        List<MageController> targets = FindTargets(TargetType.HIGH_HP);

        int damageNeighbors = 0; // Урон соседям
        
        int damage = 0;
        if      (owner.resultDice <= 4)
        {
            damage = 1;
            yield return DamageToTargets(damage, targets);
        }
        else if (owner.resultDice <= 9)
        {
            damage = 3;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, targets);// Урон магу и его соседям справа и слева
        }
        else
        {
            damage = 5;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, targets);// Урон магу и его соседям справа и слева
        }
        
        yield break;
    }

    // Раздвоитель личности
    // Недописанная карта
    // Нет сброса выбранного жертвой сокровища
    public IEnumerator RazdvoitelLichnosti()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        MageController target = IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT);

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 2;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                  
        {
            yield return target.ChooseAndDropTreasure(hasChoiceNotToDrop: true);
            if (target.chosenCard != null || target.treasures.Count == 0)
                damage = 3;
            else
                damage = 6;
            
        }

        yield return DamageToTarget(damage, target); //Левый маг

        yield break;
    }

    // Кулак природы
    public IEnumerator KylakPrirodi()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT)); //Левый маг
        yield break;
    }

    // Шалтай Разболтай
    public IEnumerator  ShaltaiRazboltaui()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 3;}
        else
        {
            damage = 5;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        yield return DamageToTargets(damage, FindTargets(TargetType.LOW_HP));

        yield break;
    }

    // Удар милосердия
    public IEnumerator  YdarMiloserdiya()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 2;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     {damage = 3;}

        yield return DamageToTargets(damage, FindTargets(TargetType.LOW_HP));

        yield break;
    }

    // Змеиный жор
    public IEnumerator  ZmeinuiJor()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей
        
        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 2;}
        else                     {damage = 2 * BuffDamageSign(GetSpellCard().sign);}// Увеличение урона на кол-во знаков травы(для этой карты)

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage,  POSITION_LEFT));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));

        yield return DamageToTargets(damage, listTargets);

        yield break;
    }

    // Отсос Мозга
    public IEnumerator  OtsosMozga()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage; // Враг по выбору


        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     
        {
            damage = 4;
            yield return owner.ChooseTreasureFromMage(target, "Забрать");
            if (owner.chosenCard != null)
                yield return target.PassTreasureTo(owner, owner.chosenCard); // Отжать сокровище у врага по своему выбору
            
        }

        yield return DamageToTarget(damage, target);

        yield break;
    }

    // Нетерпеливый
    // Начинать ход первым
    // Проверка во время определения очередности хода игроков
    public IEnumerator  Neterpeliviu()
    {
        yield return DamageToTargets(1, AllEnemies());

        yield break;
    }

    // Ритуальный
    public IEnumerator  Rityalnui()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        int damage = 0;
        if(owner.resultDice <= 4)
        {
            damage = 3;
            yield return DamageToTarget(damage, owner);
        }
        else
        {
            yield return owner.ChooseEnemy();
            MageController target = owner.chosenMage; // Враг по выбору
            
            if (owner.resultDice <= 9)
            {
                damage = 3;
            }
            else                     
            {
                damage = 5;
            }
            yield return DamageToTarget(damage, target);
        }

        yield break;
    }

    // Дьявольский
    public IEnumerator  Dyavolskiu()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage; // Враг по выбору
        

        int selfDamage = 0;
        int damage = 0;
        if(owner.resultDice <= 4)
        {
            damage = 2;
            yield return DamageToTarget(damage, target);
        }
        else if (owner.resultDice <= 9)
        {
            damage = 4;
            selfDamage = 1;
            yield return DamageToTarget(damage, target);
            yield return DamageToTarget(selfDamage, owner);
        }
        else                     
        {
            damage = 5;
            selfDamage = 2;
            yield return DamageToTarget(damage, target);
            yield return DamageToTarget(selfDamage, owner);
        }

        yield break;
    }

    // Дискотечный
    // Недописанная карта
    // Нужен метод выбора заводилы или прихода
    public IEnumerator  Diskotechnui()
    {
        yield return owner.ChooseCardFromSpellOfOrders(new List<Order>() {Order.SOURCE, Order.DELIVERY});
        yield return owner.OneSpellCardExecution(owner.chosenCard);
        
        yield break;
    }

    //Двуличный
    public IEnumerator  Dvylichniu()
    {
        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage; // Враг по выбору
        

        yield return gm.treasuresDeck.PassCardsTo(target, 1); 

        int damage = target.treasures.Count * 2; // Урон = Кол-во сокровищ * 2
        
        yield return DamageToTarget(damage, target);

        yield break;
    }



    // Котострофический
    public IEnumerator  Kotostroficheskiu()
    {
        // Словарь маг key бросок value
        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1; // кол-во кубиков
        
        foreach (MageController mage in gm.aliveMages)
        {
            
            mage.mageIcon.Highlight(true);

            List<int> rolls = RollDice(numberDice); // Бросок кубика 
            
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();

            string resultDiceText = resultDice.ToString();

            if( mage == owner )
            {
                resultDice += 2;// Если цель владелец, +2 к результату
                resultDiceText += " +2";
            }

            mage.mageIcon.ShowInfoText(resultDiceText);

            magesAndRolls.Add(mage, resultDice); 

            mage.mageIcon.Highlight(false);
        }

        var maxValue = magesAndRolls.Values.Max();// Нахождение максимального броска

        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            if( mageRoll.Value == maxValue )
            {
                mage.mageIcon.Highlight(true);
                
                // Если маг выкинул максимальное число, получает сокровище
                yield return gm.treasuresDeck.PassCardsTo(mage, 1);

                mage.mageIcon.Highlight(false);
            }
            else
            {
                // Остальные получают урон = броску
                yield return DamageToTarget(mageRoll.Value, mageRoll.Key);
            }

            mage.mageIcon.HideInfoText();
        }
        yield break;
    }

    //Мозголомный
    public IEnumerator  Mozgolomniu()
    {
        // Урон
        int damage = 3;
        
        // Выбираем случайного врага
        MageController randomEnemy = FindTargets(TargetType.RANDOM)[0];
        yield return OnRandomEnemy(randomEnemy);
        
        // Наносим урон
        yield return DamageToTarget(damage, randomEnemy);
        
        // Берем сокровище для себя и случайного врага
        yield return gm.treasuresDeck.PassCardsTo(owner, 1);
        yield return gm.treasuresDeck.PassCardsTo(randomEnemy, 1);
        
        yield break;
    }

    // Разрывной
    // Недописанная карта
    // Нет выбора врага
    // Нет выбора сокровища для сброса
    public IEnumerator  Razrivnoi()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        int selfDamage = 0;
        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage; // Враг по выбору
        
        
        int damage = 0;
        if(owner.resultDice <= 4)
        {
            damage = 1;
            yield return DamageToTarget(damage, target);
        }
        else if (owner.resultDice <= 9)
        {
            damage = 3;
            selfDamage = 1;
            yield return DamageToTarget(damage, target);
            yield return DamageToTarget(selfDamage, owner);
        }
        else                     
        {
            damage = 4;
            yield return DamageToTarget(damage, target);
            yield return owner.ChooseTreasureFromMage(target, "Выбрать сокровище врага");
            if (owner.chosenCard != null)
                target.DropCard(owner.chosenCard);
            
        }

        yield break;
    }



    //Шипастый
    public IEnumerator Shipastui()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        int damage = 0;
        int healHp = 0;
        if      (owner.resultDice <= 4){damage = 1; healHp = 0;}
        else if (owner.resultDice <= 9){damage = 1; healHp = 1;}
        else                     {damage = 3; healHp = 3;}

        yield return HealToTarget(healHp, owner);
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));
        
        yield break;
    }

    // Загнивающий
    // Убрать из списка сходивших магов
    // Нужна функция сбрасывания Quality у таргетов
    public IEnumerator Zagnivaushui()
    {
        // Убрать с листа сходивших магов 
        List<MageController> targets = FindTargets(TargetType.HIGH_HP).FindAll(mage => !mage.spellsAreExecuted);
        
        targets.ForEach(mage => mage.DropSpellOfOrder(Order.QUALITY));
        
        yield break;
    }

    //Каменючный
    public IEnumerator Kamenuchnui()
    {
        MageController target = owner.leftMage; // Берем левого врага у владельца

        int damage = 1;

        while (target != owner)// Пока цель не владелец
        {
            if (!target.isDead)// Если враг не умер
            {
                yield return DamageToTarget(damage, target);
                damage++;
            }
            target = target.leftMage; // Берем следующего левого мага
        }
        yield break;
    }

    //От старого Жгуна
    public IEnumerator  OtStarogoJgyna()
    {
        List<int> rolls = RollDice(1); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        if(owner.resultDice <= 3)
        {
            yield return DamageToTarget(owner.resultDice, owner); // Урон владельцу сколько выпало
        }
        else
        {
            yield return HealToTarget(owner.resultDice, owner); // Лечение владельцу, сколько выпало
        }

        yield break;
    }

    // От Короля Оберона
    public IEnumerator  OtKoroluaOberona()
    {
        yield return HealToTarget(2, owner);
        
        yield break;
    }




    // Кубический
    public IEnumerator  Kybicheskui()
    {
        // Словарь маг key бросок value
        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1; // кол-во кубиков

        owner.mageIcon.Highlight(true);

        List<int> rolls       = RollDice(numberDice); // Бросок кубика 
        yield return OnDiceRoll(rolls, showBonus: false);
        int firstResultOwner  = rolls.Sum();          // Результат первого кубика владельца

        owner.mageIcon.ShowInfoText($"{firstResultOwner}");

        rolls = RollDice(numberDice);
        yield return OnDiceRoll(rolls, showBonus: false);
        int secondResultOwner = rolls.Sum(); // Результат второго кубика владельца

        owner.mageIcon.ShowInfoText($"{firstResultOwner} + {secondResultOwner}");

        owner.mageIcon.Highlight(false);

        foreach (MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);
            
            rolls = RollDice(numberDice);
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();
            magesAndRolls.Add(mage, resultDice); // Каждый маг кидает кубик

            mage.mageIcon.ShowInfoText(resultDice.ToString());

            mage.mageIcon.Highlight(false);
        }

        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            int mageDice = mageRoll.Value;


            // Если совпало с 1 кубиком, получает урон = броску
            if( mageDice == firstResultOwner || mageDice == secondResultOwner)
            {
                yield return DamageToTarget(mageDice, mage); 
            }

            mage.mageIcon.HideInfoText();
        }

        owner.mageIcon.HideInfoText();

        yield break;
    }

    // Рубин в башке
    public IEnumerator  RubinVBashke()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();
        
        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage; // Враг по выбору
        

        int damage = 1;
        
        yield return DamageToTarget(damage, target);
        if (owner.resultDice <= 9)
        {
            // Случайная карта с руки к заклинанию   
            List<CardController> hand = owner.GetSpellsInHand();
            int index = random.Next(hand.Count);
            yield return owner.AppendToSpell(hand[index]);
        }
        else                     
        {
            yield return owner.ChooseCardFromHand();
            if (owner.chosenCard != null)
                yield return owner.AppendToSpell(owner.chosenCard);
            
        }

        yield break;
    }



    // Отборный
    public IEnumerator  Otbornui()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9)
        {
            damage = 2;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }
        else
        {
            damage = 5;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));

        yield break;
    }

    // Качковый
    // Недописанная карта
    // Нет добавления случайно карты к своему заклинанию
    public IEnumerator  Kachkovui()
    {
        int healHp = 2;
        yield return HealToTarget(healHp, owner);

        List<MageController> allMages = gm.aliveMages.ToList();
        allMages.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага

        int minHp = allMages[0].health; // Сохранение его здоровья

        if(owner.health == minHp)
        {
            // Добавить случайную карту к своему заклинанию
            List<CardController> spellsInHand = owner.GetSpellsInHand();
            int randomCardIndex = random.Next(spellsInHand.Count);
            CardController card = spellsInHand[randomCardIndex];
            yield return owner.AppendToSpell(card);
        }
        
        yield break;
    }

    // Мошоночный 
    public IEnumerator Moshonochnui()
    {
        int damage = 3;
        
        MageController leftMage  = IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT);
        MageController rightMage = IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT); 
        
        yield return leftMage.ChooseAndDropTreasure(hasChoiceNotToDrop: true);
        if (leftMage.chosenCard == null)
            yield return DamageToTarget(damage, leftMage);

        yield return rightMage.ChooseAndDropTreasure(hasChoiceNotToDrop: true);
        if (rightMage.chosenCard == null)
            yield return DamageToTarget(damage, rightMage);

        yield break;
    }

    // От Бена Вуду
    public IEnumerator  OtBenaVydy()
    {             
        int numberDice = 1; // кол-во кубиков

        foreach(MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);

            List<int> rolls = RollDice(numberDice);
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();

            mage.mageIcon.ShowInfoText(resultDice.ToString());

            yield return DamageToTarget(resultDice, mage);

            mage.mageIcon.HideInfoText();
            mage.mageIcon.Highlight(false);
        }
        
        yield return owner.ChooseAndDropTreasure();
        

        yield break;
    } 

    // От Сера Кладомота
    public IEnumerator  OtSeraKladomota()
    {
        int numberDice = 1; // кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить сокровище

        yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        
        foreach(MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);

            List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика 
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();


            if (resultDice == necessaryResult)
                yield return gm.treasuresDeck.PassCardsTo(mage, 1);

            mage.mageIcon.Highlight(false);
        }

        yield break;
    }

    // От Магмуда Поджигая
    public IEnumerator  OtMagmydaPodjigatelua()
    {
        int damageSolo = 3; // Урон одному врагу
        int damageAll  = 1; // Урон всем

        yield return owner.ChooseEnemy();
        MageController leftTarget = IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT);
        MageController target     = owner.chosenMage;

        if(target == leftTarget)
        {
            // Нанести урон левому врагу
            yield return DamageToTarget(damageSolo, leftTarget);
        }
        else
        {
            // Нанести урон всем врагам
            yield return DamageToTargets(damageAll, AllEnemies());
        }
 
        yield break;
    }

    // От Мордоеда
    // Скопировать решение у карты дохлый колдун
    public IEnumerator  OtMordoeda()
    {
        owner.bonusDice += 1;

        owner.mageIcon.ShowInfoText($"+1");

        yield return new WaitForSeconds(1.5f);

        owner.mageIcon.HideInfoText();

        yield break;
    }

    // От Горячей штучки
    public IEnumerator  OtGoruacheiShtychki()
    {
        int damage = 3;
        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));

        yield break;
    }

    // От профессора Ахалая
    // Дописать шальную магию и проверку на наличие ее в заклинании
    public IEnumerator  OtProfessoraAxalaya()
    {
        int damage = 3;
        yield return DamageToTargets(damage, FindTargets(TargetType.RANDOM));

        if(owner.isWildMagicInSpell)
            yield return gm.treasuresDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // Жмураган
    public IEnumerator Jmyragan()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        owner.resultDice  = rolls.Sum();

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 2;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     {damage = 6;}
        

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        
        yield break;
    }

    // От Сфинксенона
    public IEnumerator  OtSfinksenona()
    {
        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage;
 
        yield return owner.ChooseTreasureFromMage(target, "Отжать");
        CardController treasure = owner.chosenCard;
        yield return target.PassTreasureTo(owner, treasure);
     
        yield break;
    }



    // От Тай Тьфуна
    // Поменять список целей на список уже сходивших
    public IEnumerator OtTauiTfyna()
    {
        int damage = 3;

        yield return DamageToTargets(damage, gm.magesWhoExecutedSpell.FindAll(mage => mage != owner));
        
        yield break;
    }




    // От Поганого Мерлина
    public IEnumerator  OtPoganogoMerlina()
    {
        int damage = gm.aliveMages.Count;
        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        
        yield return gm.deadsDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // От Брадострела
    public IEnumerator  OtBradostrela()
    {
        List<CardController> deliveries = owner.GetCardsOfOrderInSpell(Order.DELIVERY);
        
        foreach (CardController card in deliveries)
        {
            yield return owner.OneSpellCardExecution(card);
        }

        yield break;
    }



    // От Феечки смерти
    // Недописанная карта
    public IEnumerator  OtFeechkiSmerti()
    {
        int damage = 2;
        yield return owner.ChooseEnemy();
        MageController target = owner.chosenMage;
        
        yield return DamageToTarget(damage, target);
        while(target.isDead)
        {
            yield return owner.ChooseEnemy();
            target = owner.chosenMage;
            yield return DamageToTarget(damage, target);
        }

        yield break;
    }

    // От д-ра Конея Дуболома
    public IEnumerator  OtDraKorneyaDyboloma()
    {
        int healHp = 3; // Сколько жизней нужно вылечить
        yield return HealToTarget(healHp, owner); // Лечение себя

        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1;// кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить лечение
        
        // Каждый враг бросает кубик 
        foreach(MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);
            List<int> rolls = RollDice(numberDice);
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice  = rolls.Sum();
            magesAndRolls.Add(mage, resultDice);
            mage.mageIcon.ShowInfoText(resultDice.ToString());
            mage.mageIcon.Highlight(false);
        }

        // Проходим по каждому врагу и смотрим на его бросок
        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            if( mageRoll.Value == necessaryResult )// Если враг выкинул 6
            {
                yield return HealToTarget(healHp, mage);// Лечение врага
            }
            mage.mageIcon.HideInfoText();
        }
        
        yield break;
    }

    // Фонтан Молодости
    public IEnumerator FontanMolodosti()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        owner.resultDice  = rolls.Sum();
        
        int healHp = 0;
        if      (owner.resultDice <= 4){healHp = 0;}
        else if (owner.resultDice <= 9){healHp = 2;}
        else                     {healHp = 4;}

        yield return HealToTarget(healHp, owner); // Лечение владельца
        yield break;
    }

    // Вихрь бодрости
    public IEnumerator VihrBodrosti()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign));
        yield return OnDiceRoll(rolls);
        owner.resultDice = rolls.Sum(); // Бросок кубика
        
        yield return owner.ChooseCardFromHand();
        if (owner.chosenCard != null)
            owner.DropCard(owner.chosenCard);

        int damage = 0; 
        if (owner.resultDice > 4)
        {
            yield return owner.ChooseCardFromHand();
            if (owner.chosenCard != null)
                owner.DropCard(owner.chosenCard);

            damage = 2;
            yield return DamageToTargets(damage, AllEnemies());
            if (owner.resultDice > 9)
            {
                yield return gm.treasuresDeck.PassCardsTo(owner, 1);
            }
        }
        
        yield break;
    }

    // Самовыпил
    public IEnumerator Samovipil()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        owner.resultDice  = rolls.Sum();

        int damageSelf = 1;

        int damage = 0;
        if      (owner.resultDice <= 4){damage = 2;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     {damage = 5;}

        yield return DamageToTarget(damageSelf, owner);
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT)); //Правый маг
        
        yield break;
    }

    // Мясной фарш
    public IEnumerator MyasnouiFarsh()
    {
        List<int> rolls = RollDice(NumberDice(GetSpellCard().sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        owner.resultDice  = rolls.Sum();

        int damage = 0;

        // Поиск врагов с большим здоровьем чем у владельца
        List<MageController> listTargets = gm.aliveMages.FindAll(mage => mage.health > owner.health);

        if      (owner.resultDice <= 4){damage = 1;}
        else if (owner.resultDice <= 9){damage = 3;}
        else                     {damage = 4;}

        yield return DamageToTargets(damage, listTargets);
        
        yield break;
    }

    #endregion

    #endregion


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Подвязки госпожи удачи
    public IEnumerator PodvyazkiGospozhiUdachi()
    {
        if (owner.nCardsInSpell <= 2)
        {
            int index = random.Next(owner.nSpellsInHand);
            CardController card = owner.GetSpellsInHand()[index];
            yield return owner.AppendToSpell(card);
        }
    }

    // Некрасные Мокасины
    public IEnumerator NekrasnieMokasini()
    {
        if (owner.isExecuting)
            owner.additionalSigns.Add(Sign.ARCANE);
        yield break;
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Башмаки Скомороха
    public IEnumerator BashmakiSkomoroha()
    {
        if (owner.isExecuting)
            owner.additionalSigns.Add(Sign.ILLUSION);
        yield break;
    }
// 
}
