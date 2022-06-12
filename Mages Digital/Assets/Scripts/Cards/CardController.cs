using Random = System.Random;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine.UI;
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


    bool _discoverable        = true;               // можно ли взаимодействовать с картой
    bool _isMouseOver         = false;              // находится ли курсор на карте
    float _mouseOverTime      = 0.0f;               // время, прошедшее с момента наведения на карту
    int _bonusInfoIndexOffset = 0;                  // нужна для перебора карт сокровищ при наведении на них
    Order _spellOrder         = Order.WILDMAGIC;    // порядок в заклинании (нужен для шальной магии)
    CardState cardState       = CardState.NO_OWNER; // состояние карты


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
    public DeckController sourceDeck => _sourceDeck;
    
    public bool  visible      => _visible;
    public bool  discoverable => _discoverable;
    public Order spellOrder
    {
        get => _spellOrder;
        set => _spellOrder = value;
    }

    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;
    public SpriteRenderer backSpriteRenderer  => _backSpriteRenderer;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public GameManager gm => GameManager.instance;

    public bool  isSpell         => (_card != null) && (_card.cardType == CardType.SPELL); // является карта картой заклинания
    public bool  inHand          => cardState == CardState.IN_HAND;     // находится ли карта в руке
    public bool  inSpell         => cardState == CardState.IN_SPELL;    // находится ли карта в заклинании
    public bool  withOwner       => cardState != CardState.NO_OWNER;    // есть ли у карты владелец
    public bool  withSourceDeck  => _sourceDeck != null;                // создана ли карта колодой
    public float cardSizeY       => _middle.transform.localScale.y;     // длина карты
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
        if (order == Order.WILDMAGIC && !owner.ownerIsBot)
            StartCoroutine(owner.AddWildMagicToSpell(this));
        else
            StartCoroutine(owner.AddToSpell(this, order));
    }

    // вернуть карту в руку из заклинания
    void BackToHand()
    {
        StartCoroutine(owner.BackToHand(this, spellOrder));
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
                if (withOwner && !owner.ownerIsBot)
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
    void SelectCard(bool select)
    {
        if (!owner.ownerIsBot)
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
        if (!owner.ownerIsBot)
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

    public int RollDice(int numberDice)
    {
        int totalRoll = 0; // Счет выпавших кубиков
        
        for (int i = 0; i < numberDice;  i++)
           totalRoll += random.Next(1, 7); // Бросок кубика

        return totalRoll;
    }

    public int NumberDice(Sign sign)
    {
        int numberDice = owner.bonusDice; // Кол-во кубиков

        // Цикл нахождения кол-во одинаковых знаков карты заклинания => кол-во кубиков 
        List<CardController> currentSpell = owner.nonNullSpell.ToList();
        foreach (CardController spellCard in currentSpell)
        {
            if(spellCard.GetSpellCard().sign == sign)
                numberDice+= 1;
        }
        return numberDice;
    }

    // Урон магу (Урон, Цель)
    public IEnumerator DamageToTarget(int damage, MageController target)
    {
        target.TakeDamage(damage, this);
        yield return gm.lightningManager.Generate(transform.position, target.mageIcon.transform.position, 1.0f);
    } 

    // Урон нескольким магам (Урон, Лист Целей)
    public IEnumerator DamageToTargets(int damage, List<MageController> listTargets)
    {
        foreach(MageController target in  listTargets)
            yield return DamageToTarget(damage, target);

        yield break;
    }

    #endregion

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region [ SPELLS ]

    // Драконий сундук
    public IEnumerator  DrakoniuSundyk()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
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

    #endregion

    #endregion


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
