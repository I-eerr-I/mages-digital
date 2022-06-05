using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine.UI;
using UnityEngine;

public class CardController : MonoBehaviour
{
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

    [SerializeField] private Card _card;            // данные карты
    [SerializeField] private MageController _owner; // маг владеющий картой

    [Header("Анимация карты")]
    [SerializeField] private float _cardFlippingTime     = 0.15f;
    [SerializeField] private float _cardShowInfoWaitTime = 1.5f;

    private float _mouseOverTime = 0.0f; // время, прошедшее с момента наведения на карту

    private SpriteRenderer _frontSpriteRenderer;    // рендер передней части карты
    private SpriteRenderer _backSpriteRenderer;     // рендер задней части (рубашки) карты

    private OutlineController _outlineController;   // управление подсветкой карты

    
    public bool  discoverable  = true; // можно ли взаимодействовать с картой

    public CardState cardState = CardState.NO_OWNER; // состояние карты

    public bool inHand    => cardState == CardState.IN_HAND;  // находится ли карта в руке
    public bool inSpell   => cardState == CardState.IN_SPELL; // находится ли карта в заклинании
    public bool withOwner => cardState != CardState.NO_OWNER; // есть ли у карты владелец
    public bool isSpell   => (_card != null) && (_card.cardType == CardType.SPELL); // является карта картой заклинания

    public Card card => _card;
    public MageController owner => _owner;
    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;
    public SpriteRenderer backSpriteRenderer  => _backSpriteRenderer;

    void Awake()
    {
        _frontSpriteRenderer     = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer      = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        _outlineController       = transform.GetChild(2).gameObject.GetComponent<OutlineController>();
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
    }
    // TEST

    // настроить карту
    public void SetupCard(Card card, Sprite back = null)
    {
        // данные карты
        _card = card;
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

    // триггер при наведении курсора на карту
    public void OnMouseOverTrigger()
    {
        if (discoverable)
        {
            if (withOwner && isSpell)
            {
                _mouseOverTime += Time.deltaTime;
                if (_mouseOverTime >= _cardShowInfoWaitTime)
                    UIManager.instance.ShowCardInfo(card.front, true);
                owner.owner.OnSpellCardSelected(this, true);
            }
        }
    }

    // триггер при выходе курсора из области карты
    public void OnMouseExitTrigger()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowCardInfo(card.front, false);
        if (discoverable)
        {
            if (isSpell && withOwner)
                owner.owner.OnSpellCardSelected(this, false);
        }
    }

    // триггер при клике на карту
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XXX баг при быстром клике на карту одного и того же порядка
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    public void OnMouseDownTrigger()
    {
        if (discoverable && GameManager.instance.isSpellCreationState)
        {
            if (isSpell && withOwner)
            {
                SpellCard spellCard = (SpellCard) card;
                Order order = spellCard.order;
                if (inHand)
                {
                    if (order == Order.WILDMAGIC) return; // XXX
                    StartCoroutine(owner.AddToSpell(this, order));
                }
                else if (inSpell)
                {
                    StartCoroutine(owner.BackToHand(this, order));
                }
            }
        }
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

}
