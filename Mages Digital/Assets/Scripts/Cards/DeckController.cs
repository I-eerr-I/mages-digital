using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{
 
    public static Dictionary<CardType, Color> DECK_MAIN_COLOR = new Dictionary<CardType, Color>()
    {
        { CardType.SPELL,    new Color(1.0f, 0.0f, 0.0f) },
        { CardType.TREASURE, new Color(1.0f, 0.8427867f, 0.0f) },
        { CardType.DEAD,     new Color(0.0f, 0.6603774f, 0.4931389f) }
    };

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
    
    private bool  _hidden       = true;   // спрятана ли колода

    private MeshRenderer   _baseMeshRenderer;
    private Light          _baseLight;
    private SpriteRenderer _backSpriteRenderer;

    private Sprite _back; // рубашка карт колоды

    public float    hiddenY     => _hiddenY;
    public float    unhiddenY   => _unhiddenY;
    public float    hideTime    => _hideTime;
    public CardType cardsType   => _cardsType;         
    public int      cardsAmount => _deck.Count;        // количество карт в колоде

    // TEST
    private int oldCardsAmount;
    // TEST 

    void Awake()
    {
        _backSpriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        _baseLight          = gameObject.GetComponentInChildren<Light>();
        _baseMeshRenderer   = gameObject.GetComponentInChildren<MeshRenderer>();
        _back = _backSpriteRenderer.sprite;
        _baseLight.color = DECK_MAIN_COLOR[_cardsType];
        if (_cardsType == CardType.SPELL) DoubleDeck();
        UpdateDeckSize();
        
        // TEST
        oldCardsAmount = _deck.Count;
        // TEST
    }

    void Start()
    {
        Shuffle();
    }

    void Update()
    {

        // TEST
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Hide(!_hidden);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (_cardsType == CardType.SPELL)
            {
                GameObject cardObject = Instantiate(GameManager.instance.cardPrefab, GameManager.instance.fieldCenter);
                cardObject.transform.position = new Vector3(-2.0f, 1.0f, 0.0f);
                CardController cardController = cardObject.GetComponent<CardController>();
                cardController.SetupCard(_deck[0], _back);
                _deck.RemoveAt(0);
            }
        }

        if (oldCardsAmount != cardsAmount)
        {
            UpdateDeckSize();
            oldCardsAmount = _deck.Count;
        }
        // TEST

    }


    // выдать карту из колоды
    public IEnumerator PassCardsTo(MageController owner, int nCards)
    {
        Hide(false);
        yield return new WaitForSeconds(_hideTime);
        for (int i = 0; i < nCards; i++)
        {
            Card card = TakeLastCard();
            if (card != null)
            {
                CardController cardController = SpawnCard(card, transform.position);
                StartCoroutine(owner.AddCard(cardController, true));
                yield return new WaitForSeconds(0.25f);
            }
        }
        Hide(true);
    }

    public Card TakeLastCard()
    {
        Card card = null;
        if (_deck.Count > 0)
        {
            card = _deck[0];
            _deck.RemoveAt(0);
        }
        else if (_fold.Count > 0)
        {
            ShuffleWithFold();
            return TakeLastCard();
        }
        return card;
    }

    public CardController SpawnCard(Card card, Vector3 position)
    {
        GameObject cardObject = Instantiate(GameManager.instance.cardPrefab, GameManager.instance.fieldCenter);
        cardObject.name = card.cardName;
        cardObject.transform.position = position;
        CardController cardController = cardObject.GetComponent<CardController>();
        cardController.SetupCard(card, _back);
        return cardController;
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


    // показать колоду
    public void Hide(bool hide)
    {
        if (hide != _hidden)
        {
            float y = (hide) ? _hiddenY : _unhiddenY;
            iTween.MoveTo(gameObject, iTween.Hash("y", y, "time", _hideTime, "easetype", iTween.EaseType.easeOutSine));
            _hidden = hide;
        }
    }

}
