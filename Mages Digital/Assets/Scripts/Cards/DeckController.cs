using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
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
    
    private bool  _hidden       = true;   // спрятана ли колода
    private bool _idleAnimation = false;  // включена анимация колоды


    public float    hiddenY     => _hiddenY;
    public float    unhiddenY   => _unhiddenY;
    public CardType cardsType   => _cardsType;         
    public int      cardsAmount => _deck.Count;        // количество карт в колоде

    // TEST
    private int oldCardsAmount;
    // TEST 

    void Awake()
    {
        if (_cardsType == CardType.SPELL) DoubleDeck();

        UpdateDeckSize();
        
        // TEST
        oldCardsAmount = _deck.Count;
        // TEST
    }

    void Update()
    {

        // TEST
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Hide(!_hidden);
        }

        if (oldCardsAmount != cardsAmount)
        {
            UpdateDeckSize();
            oldCardsAmount = _deck.Count;
        }
        // TEST

    }


    // выдать карту из колоды
    public Card PassCard()
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
            return PassCard();
        }
        return card;
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
    }

    // удвоить карты в колоде
    void DoubleDeck()
    {
        List<Card> deckToAdd = new List<Card>(_deck);
        if (_cardsType == CardType.SPELL)
            deckToAdd = _deck.FindAll((card) => ((SpellCard)card).order != Order.WILDMAGIC);
        _deck.AddRange(deckToAdd);
    }

    // если количество карт в колоде было изменено, то и изменится размер самой колоды
    void UpdateDeckSize()
    {
        gameObject.SetActive(true);
        float deckSize = _cardThickness * _deck.Count;
        iTween.ScaleTo(gameObject, new Vector3(transform.localScale.x, transform.localScale.y, deckSize), 0.01f);   
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
