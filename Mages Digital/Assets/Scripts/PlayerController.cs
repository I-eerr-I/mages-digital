using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private MageController _mage;   // маг под управлением

    private Order _chosenOrder = Order.WILDMAGIC;

    private bool _readyToExecute = false;

    [Header("Параметры расположения карт в руке по-умполчанию")]
    [SerializeField] private float _handCardsZ =  5.5f;    // локальная координата Z в руке по-умолчанию
    [SerializeField] private float _handCardsY = -2.75f;   // локальная координата Y в руке по-умолчанию
    
    [Header("Параметры перемещения карт")]
    [SerializeField] private Vector3 _cardMovingDestination       = new Vector3(0.0f, -5.5f, 7.5f); // глобальная позиция, куда карта перемещается после спауна
    [SerializeField] private float   _bonusCardMovingToHandTime   = 1.0f;  // время перемещения карты к руке
    [SerializeField] private float   _cardMovingToSpellTime       = 0.25f; // время перемещения карты к заклинанию
    [SerializeField] private float   _cardAvgFittingTime          = 0.15f; // среднее время выравнивания карты заклинания в руке

    [Header("Параметры расположения и анимации карт заклинаний в руке")]
    // [SerializeField] private float _spellsStartX        = 0.0f;     // локальная начальная координата X для заклинаний
    [SerializeField] private float _spellsRightMaxX     = 3.0f;     // локальное крайнее правое зачение X для заклинаний
    [SerializeField] private float _spellsSelectedZ     = 5.25f;    // локальная координата Z при наведении на карту
    [SerializeField] private float _spellsSelectedY     = -2.5f;    // локальная координата Y при наведении на карту
    
    [Header("Наклон карт заклинаний в руке")]
    [SerializeField] private float _spellsRotLeftMaxZ   = 30.0f;    // максимальный угол наклона влево
    [SerializeField] private float _spellsRotEllipseR   = 0.25f;    // радиус (высота) эллипса при наклоне
    [SerializeField] private bool  _rotateSpells        = true;     // применять ли наклон карт в руке

    [Header("Параметры расположения бонусных карт по-умолчанию")]
    [SerializeField] private float _bonusesStartX   = 4.5f;         // локальная координата X в руке по-умолчанию

    [Header("Параметры при выборе игрока")]
    [SerializeField] private float _spellLocationChoosingY    = 2.0f;  // глобальная координата Y при выборе месте выставления карты
    [SerializeField] private float _spellLocationMovingUpTime = 1.5f;  // время подъема места выбора карты

    [Header("Анимация готовности заклинания")]
    [SerializeField] private float _spellUpY             = 1.5f;  // насколько вверх поднять карты
    [SerializeField] private float _spellDownY           = 0.0f;  
    [SerializeField] private float _rightLeftSpellX      = 2.0f;  // насколько в сторону отоднинуть правую и левую карты
    [SerializeField] private float _spellUpTime          = 1.0f;  // время подъема карт
    [SerializeField] private float _rightLeftSpellEndX   = 1.25f; // конечная глобальная координата X
    [SerializeField] private float _spellDownTime        = 0.5f;  // время падения карт
    [SerializeField] private Vector3 _shakeAmount        = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private float _shakeTime            = 0.25f;

    public MageController mage => _mage;

    public Order chosenOrder    => _chosenOrder;
    public bool  isOrderChosen  => _chosenOrder != Order.WILDMAGIC;
    public bool  readyToExecute => _readyToExecute;

    void Awake()
    {
        _mage = gameObject.GetComponent<MageController>();
    }

    void Start()
    {
        _mage.owner = this;
    }

    void Update()
    {
        // TEST
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(FitSpellCardsInHand());
        }
        // TEST

        if (Input.GetButton("Execute"))
        {
            StartCoroutine(OnExecute());
        }
    }

    public IEnumerator OnExecute()
    {
        if (_mage.spellIsReady)
        {
            for (int i = 0; i < 3; i++)
            {
                CardController spellCard = mage.spell[i]; 
                if (spellCard == null) continue;
                Hashtable hashtable = new Hashtable();
                hashtable.Add("time", _spellUpTime);
                hashtable.Add("y", _spellUpY);
                if (spellCard.spellOrder != Order.QUALITY)
                {
                    float x = (spellCard.spellOrder == Order.SOURCE) ? -_rightLeftSpellX : _rightLeftSpellX;
                    hashtable.Add("x", x);
                }
                iTween.MoveTo(spellCard.gameObject, hashtable);
            }

            yield return new WaitForSeconds(_spellUpTime);

            for (int i = 0; i < 3; i++)
            {
                CardController spellCard = mage.spell[i]; 
                if (spellCard == null) continue;
                Hashtable hashtable = new Hashtable();
                hashtable.Add("time", _spellDownTime);
                hashtable.Add("y", _spellDownY);
                hashtable.Add("easetype", iTween.EaseType.easeOutExpo);
                if (spellCard.spellOrder != Order.QUALITY)
                {
                    float x = (spellCard.spellOrder == Order.SOURCE) ? -_rightLeftSpellEndX : _rightLeftSpellEndX;
                    hashtable.Add("x", x);
                }
                iTween.MoveTo(spellCard.gameObject, hashtable);
            }

            iTween.ShakePosition(transform.parent.gameObject, _shakeAmount, _shakeTime);
            iTween.ShakePosition(gameObject, _shakeAmount, _shakeTime);

            yield return new WaitForSeconds(_shakeTime);
            
            _readyToExecute = true; 
        }
        yield break;
    }

    public IEnumerator ChooseOrder()
    {
        GameManager.instance.SetChoosingState();
        _chosenOrder = Order.WILDMAGIC;

        List<Vector3> positions = new List<Vector3>();
        List<SpellLocationController> controllers = GameManager.instance.spellLocationControllers;
        foreach (SpellLocationController controller in controllers)
        {
            positions.Add(controller.transform.position);
            iTween.MoveTo(controller.gameObject, iTween.Hash("y", _spellLocationChoosingY, "time", _spellLocationMovingUpTime));
            controller.StartChoice();
        }

        yield return new WaitWhile(() => controllers.FindAll(x => x.isOrderChosen).Count == 0);

        _chosenOrder = controllers.FindAll(x => x.isOrderChosen)[0].chosenOrder;

        for(int i = 0; i < controllers.Count; i++)
        {
            iTween.MoveTo(controllers[i].gameObject, iTween.Hash("position", positions[i], "time", 0.05f));
            yield return new WaitForSeconds(0.05f);
            controllers[i].EndChoice();
        }

        GameManager.instance.ReturnToPrevState();
        yield break;
    }

    // реакция на добавление карты к руке
    public IEnumerator OnCardAddedToHand(CardController cardController)
    {

        Vector3 position = GetHandPositionVector(_bonusesStartX);

        cardController.transform.SetParent(transform);

        StartCoroutine(cardController.PositionFrontUp());
        if (!cardController.isSpell)
            iTween.MoveTo(cardController.gameObject, iTween.Hash("position", position, "time", _bonusCardMovingToHandTime, "islocal", true));
        else
            yield return FitSpellCardsInHand();
        yield break;
    }

    // реакция на добавление карты к заклинанию
    public IEnumerator OnCardAddedToSpell(CardController addedCard, Order order)
    {
        addedCard.transform.SetParent(GameManager.instance.fieldCenter);

        Transform orderLocation = GetLocationOfOrder(order);

        iTween.MoveTo(addedCard.gameObject, iTween.Hash("position", orderLocation.position, "time", _cardMovingToSpellTime));
        yield return new WaitForSeconds(_cardMovingToSpellTime);
        yield return addedCard.PositionFrontUp();
        yield return FitSpellCardsInHand();
    }

    // реакция на наведение на карту
    public void OnSpellCardSelected(CardController cardController, bool isSelected)
    {
        if (cardController.isSpell && cardController.inHand)
        {
            float y = (isSelected) ? _spellsSelectedY : _handCardsY;
            float z = (isSelected) ? _spellsSelectedZ : _handCardsZ;
            float x = cardController.transform.position.x;
            if (_rotateSpells) y += 0.25f * Mathf.Cos(x);
            iTween.MoveTo(cardController.gameObject, iTween.Hash("position", new Vector3(x, y, z), "time", _cardAvgFittingTime, "islocal", true));
            if (!isSelected && cardController.isSpell)
                StartCoroutine(FitSpellCardsInHand());
        }
    }

    // выровнять карты заклинаний в руке
    public IEnumerator FitSpellCardsInHand()
    {
        List<CardController> spellCards = _mage.GetSpells();
        float step    = (_spellsRightMaxX * 2.0f) / (spellCards.Count + 1);
        float rotStep = (_spellsRotLeftMaxZ * 2.0f) / (spellCards.Count + 1);
        float x      = -_spellsRightMaxX + step;
        // float z      = _handCardsZ;
        float rotZ   = _spellsRotLeftMaxZ - rotStep;
        for (int i = 0; i < spellCards.Count; i++)
        {
            CardController card = spellCards[i];

            card.transform.SetSiblingIndex(i);
            card.frontSpriteRenderer.sortingOrder = i;

            Vector3 position = new Vector3(x, _handCardsY, _handCardsZ);
        
            if (_rotateSpells)
            {
                position.y += _spellsRotEllipseR*Mathf.Cos(x);
                Vector3 rotation = new Vector3(0.0f, 0.0f, rotZ);
                iTween.RotateTo(card.gameObject, iTween.Hash("rotation", rotation, "time", _cardAvgFittingTime, "islocal", true));
                rotZ -= rotStep;
            }
            iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", _cardAvgFittingTime, "islocal", true));
            
            // z += 0.01f;
            x    += step;
        }
        yield break;
    }

    // вернуть локальный вектор расположения карты в руке
    Vector3 GetHandPositionVector(float x)
    {
        return new Vector3(x, _handCardsY, _handCardsZ);
    }

    // вернуть расположение места для карты заклинания определенного порядка
    public Transform GetLocationOfOrder(Order order)
    {
        switch (order)
        {
            case Order.SOURCE:
                return GameManager.instance.sourceLocation;
            case Order.QUALITY:
                return GameManager.instance.qualityLocation;
            case Order.DELIVERY:
                return GameManager.instance.deliveryLocation;
        }
        return null;
    }
}
