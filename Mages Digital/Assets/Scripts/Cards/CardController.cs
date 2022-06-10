using Random = System.Random;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine.UI;
using UnityEngine;

public class CardController : MonoBehaviour
{
    private Random random = new Random();

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

    [SerializeField] private Card _card;                 // данные карты
    [SerializeField] private MageController _owner;      // маг владеющий картой
    [SerializeField] private DeckController _sourceDeck; // колода, откуда была создана карта

    [Header("Анимация карты")]
    [SerializeField] private float _cardFlippingTime          = 0.15f; // время переворота карты
    [SerializeField] private float _cardShowInfoWaitTime      = 1.5f;  // сколько держать курсор на карте, чтобы показать ее информацию
    [SerializeField] private float _cardHighlightDeltaY       = 1.0f;  // насколько поднять карту вверх для выделения
    [SerializeField] private float _cardHighlightTime         = 1.0f;  // время поднятия карты для выделения
    [SerializeField] private float _cardHighlightLightDelta   = 2.0f;  // увеличение дальности света (яркости) при выделении
    [SerializeField] private float _cardFlyOutY               = 10.0f; // глобальная координата Y при вылете карты за экран
    [SerializeField] private float _cardFlyOutXMin            = -3.0f; // минимальное значение X при вылете карты за экран
    [SerializeField] private float _cardFlyOutXMax            = 3.0f;  // максимальное значение X при вылете карты за экран
    [SerializeField] private float _cardFlyOutTime            = 1.0f;  // время вылета карты за экран

    private float _mouseOverTime = 0.0f; // время, прошедшее с момента наведения на карту

    private GameObject        _middle;              // середина карты
    private Light             _middleLight;         // источник света карты
    private MeshRenderer      _middleMeshRenderer;  // рендер середины карты
    private BoxCollider       _middleBoxCollider;   // коллайдер середины карты
    private SpriteRenderer    _frontSpriteRenderer; // рендер передней части карты
    private SpriteRenderer    _backSpriteRenderer;  // рендер задней части (рубашки) карты
    private OutlineController _outlineController;   // управление подсветкой карты
    
    private bool _visible = true; // видимость карты

    private Order _spellOrder = Order.WILDMAGIC; // порядок в заклинании (нужен для шальной магии)



    public bool  discoverable  = true; // можно ли взаимодействовать с картой

    public CardState cardState = CardState.NO_OWNER; // состояние карты

    public Order spellOrder
    {
        get => _spellOrder;
        set => _spellOrder = value;
    }

    public Card           card  => _card;
    public MageController owner => _owner;
    public DeckController sourceDeck => _sourceDeck;
    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;
    public SpriteRenderer backSpriteRenderer  => _backSpriteRenderer;
    public bool visible => _visible;
    
    public bool withSourceDeck => _sourceDeck != null;
    public bool inHand    => cardState == CardState.IN_HAND;  // находится ли карта в руке
    public bool inSpell   => cardState == CardState.IN_SPELL; // находится ли карта в заклинании
    public bool withOwner => cardState != CardState.NO_OWNER; // есть ли у карты владелец
    public bool isSpell   => (_card != null) && (_card.cardType == CardType.SPELL); // является карта картой заклинания
    public float cardSizeX => _middle.transform.localScale.x;
    public float cardSizeY => _middle.transform.localScale.y;


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

    // TEST
    bool isup = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isup)
                StartCoroutine(PositionBackUp());
            else
                StartCoroutine(PositionFrontUp());
            isup = !isup;
        }
        else if (Input.GetKeyDown(KeyCode.V))
            SetVisible(!_visible);
    }
    // TEST

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

    // вылет карты за экран и уничтожение
    public IEnumerator FlyOutAndDestroy(bool destroy = true)
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

    // выделение карты (обычно в заклинании)
    public IEnumerator Highlight(bool highlight)
    {
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
        print($"Карта {card.cardName} выполяет спелл {card.spell}");
        // yield return StartCoroutine(card.spell);
        yield return new WaitForSeconds(1.0f);
        print("ЗАКОНЧИЛА");
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

    // установить владельца карты
    public void SetOwner(MageController owner)
    {
        _owner = owner;
    }

    // удалить владельца карты
    public void RemoveOwner()
    {
        _owner = null;
        StateToNoOwner();
    }

    // триггер при наведении курсора на карту
    public void OnMouseOverTrigger()
    {
        if (discoverable)
        {
            if (withOwner && isSpell)
            {
                SelectCard(true);
                ShowCardInfo();
            }
        }
    }

    // триггер при выходе курсора из области карты
    public void OnMouseExitTrigger()
    {
        HideCardInfo();
        if (discoverable)
        {
            if (isSpell && withOwner)
                SelectCard(false);
        }
    }

    // триггер при клике на карту
    public void OnMouseDownTrigger()
    {
        if (discoverable)
        {
            if (GameManager.instance.isSpellCreationState)
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
            else if (GameManager.instance.isChoosingState)
            {
                GameManager.instance.StopChoosing();
            }
        }
    }

    // показать информацию о карте
    public void ShowCardInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= _cardShowInfoWaitTime)
            UIManager.instance.ShowCardInfo(this, true);
    }

    // спрятать информацию о карте
    public void HideCardInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowCardInfo(this, false);
    }

    // выделить карту
    public void SelectCard(bool select)
    {
        owner.owner.OnSpellCardSelected(this, select);
    }

    // добавить карту в заклинание
    public void AddToSpell()
    {
        Order order = ((SpellCard) card).order;
        if (order == Order.WILDMAGIC)
            StartCoroutine(owner.AddWildMagicToSpell(this));
        else
            StartCoroutine(owner.AddToSpell(this, order));
    }

    // получить SpellCard версию данных карты
    public SpellCard GetSpellCard()
    {
        if (card.cardType == CardType.SPELL)
            return (SpellCard) card;
        return null;
    }

    // вернуть карту в руку из заклинания
    public void BackToHand()
    {
        StartCoroutine(owner.BackToHand(this, spellOrder));
    }

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

    public void StateToNoOwner()
    {
        cardState = CardState.NO_OWNER;
        _outlineController.SetProperties(true, false);
    }

    public void StateToInSpell()
    {
        cardState = CardState.IN_SPELL;
        _outlineController.SetProperties(true, false);
    }

    public void StateToInHand()
    {
        cardState = CardState.IN_HAND;
        _outlineController.SetProperties(false, true);
    }

    public void DestoyObject()
    {
        Destroy(gameObject);
    }

}
