using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class TestDeckController : MonoBehaviour
{

    [Header("Спрятанное состояние колоды")]
    [SerializeField] private float _hiddenY   = 20.0f;  // координата Y колоды в скрытом состоянии
    [SerializeField] private float _unhiddenY = 0.5f;   // координата Y колоды в видимом состоянии
    [SerializeField] private float _hideTime  = 0.25f;  // время для анимации скрытия колоды
    
    [Header("Размер и вид 3D модели колоды")]
    [SerializeField] private float  _cardThickness = 0.025f; // толщина одной карты

    [Header("Колоды")]
    [SerializeField] private CardType _cardsType = CardType.SPELL;    // тип карт в колоде
    [SerializeField] private List<Card>  _deck   = new List<Card>();  // список карт колоды
    [SerializeField] private List<Card>  _fold   = new List<Card>();  // сброс карт
    
    private Random _random = new Random();
    
    private bool  _hidden  = true;   // спрятана ли колода

    private MeshRenderer   _baseMeshRenderer;   // рендер основы колоды
    private Light          _baseLight;          // свет от основы колоды
    private SpriteRenderer _backSpriteRenderer; // рендер задней (верхней) части колоды (рубашка)

    private Sprite _back; // рубашка карт колоды

    public float    hiddenY     => _hiddenY;   
    public float    unhiddenY   => _unhiddenY;
    public float    hideTime    => _hideTime;
    public CardType cardsType   => _cardsType;         
    public int      cardsAmount => _deck.Count;  // количество карт в колоде

    void Awake()
    {
        _backSpriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        _baseLight          = gameObject.GetComponentInChildren<Light>();
        _baseMeshRenderer   = gameObject.GetComponentInChildren<MeshRenderer>();

        _back = _backSpriteRenderer.sprite;
        _baseLight.color = TestCardController.CARD_MAIN_COLOR[_cardsType];

        if (_cardsType == CardType.SPELL) DoubleDeck();
        UpdateDeckSize();
    }

    void Start()
    {
        Shuffle();
    }

    void Update()
    {
        // TEST
        if (Input.GetKeyDown(KeyCode.H))
        {
            Hide(!_hidden);
        }
        // TEST
    }

    // выдать N карт магу из колоды
    public IEnumerator PassCardsTo(TestMageController owner, int nCards)
    {
        // показать колоду
        yield return Hide(false);
        for (int i = 0; i < nCards; i++)
        {
            // взять последнюю верхнюю карту из колоды
            Card card = TakeLastCard();
            // если карта есть
            if (card != null)
            {
                // создать карту
                TestCardController TestCardController = SpawnCard(card);
                // добавить карту владельцу
                StartCoroutine(owner.AddCard(TestCardController));
                yield return new WaitForSeconds(0.25f);
            }
        }
        // скрыть колоду
        yield return Hide(true);
    }

    // показать\спрятать колоду
    public IEnumerator Hide(bool hide)
    {
        if (hide != _hidden)
        {
            float y = (hide) ? _hiddenY : _unhiddenY;
            iTween.MoveTo(gameObject, iTween.Hash("y", y, "time", _hideTime, "easetype", iTween.EaseType.easeOutSine));
            _hidden = hide;
            yield return new WaitForSeconds(_hideTime);
        }
        yield break;
    }

    // вернуть последнюю верхнюю карту из колоды и удалить ее из колоды
    // если в колоде карт нет, то перемешать колоду со сбросом
    public Card TakeLastCard()
    {
        Card card = null;
        if (_deck.Count > 0)
        {
            card = _deck[0];
            _deck.RemoveAt(0);
            UpdateDeckSize();
        }
        else if (_fold.Count > 0)
        {
            ShuffleWithFold();
            return TakeLastCard();
        }
        return card;
    }

    // спаун карты на поле на месте колоды
    public TestCardController SpawnCard(Card card)
    {
        // создание и настройка объекта карты
        GameObject cardObject         = Instantiate(TestGameManager.instance.cardPrefab, TestGameManager.instance.fieldCenter);
        cardObject.name               = card.cardName;
        cardObject.transform.position = transform.position;
        
        // настройка контроллера карты
        TestCardController TestCardController = cardObject.GetComponent<TestCardController>();
        TestCardController.SetupCard(card, _back);
        
        return TestCardController;
    }

    // перемешать колоду
    public void Shuffle()
    {
        _deck = _deck.OrderBy(a => _random.Next()).ToList();
    }

    // замешать сброс
    public void ShuffleWithFold()
    {
        _deck.AddRange(_fold);
        _fold.Clear();
        Shuffle();
        UpdateDeckSize();
    }

    // удвоить карты в колоде
    void DoubleDeck()
    {
        List<Card> deckToAdd = new List<Card>(_deck);
        if (_cardsType == CardType.SPELL)
            deckToAdd = _deck.FindAll((card) => ((SpellCard)card).order != Order.WILDMAGIC);
        _deck.AddRange(deckToAdd);
        UpdateDeckSize();
    }

    // если количество карт в колоде было изменено, то и изменится размер самой колоды
    void UpdateDeckSize()
    {
        bool isDeckEmpty = _deck.Count == 0;
        _baseLight.enabled = _baseMeshRenderer.enabled = _backSpriteRenderer.enabled = !isDeckEmpty;
        if (!isDeckEmpty)
        {
            float deckSize = _cardThickness * _deck.Count;
            iTween.ScaleTo(gameObject, new Vector3(transform.localScale.x, transform.localScale.y, deckSize), 0.01f);   
        }
    }

}
