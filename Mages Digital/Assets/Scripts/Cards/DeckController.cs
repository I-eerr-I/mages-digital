using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    [Header("Hidden/unhidden states")]
    [SerializeField] private float _hiddenZ   =  0.75f;
    [SerializeField] private float _unhiddenZ = -0.75f;
    private bool _hidden = true;

    [Header("Decks")]
    [SerializeField] private List<Card>  _deck = new List<Card>();     // список карт колоды
    [SerializeField] private List<Card>  _fold = new List<Card>();     // сброс карт

    private Random _random = new Random();
    
    private CardType _cardsType; // тип карт в колоде

    private BoxCollider _collider;

    public float    hiddenZ   => _hiddenZ;
    public float    unhiddenZ => _unhiddenZ;
    public CardType cardsType => _cardsType;         
    public int      Count     => _deck.Count;        // количество карт в колоде


    void Awake()
    {
        _collider = gameObject.GetComponent<BoxCollider>();

        _cardsType = _deck[0].cardType;
        if (_cardsType == CardType.SPELL) DoubleDeck();
    }

    void Start()
    {
        SetHiddenState(_hidden, true);
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

    void SetHiddenState(bool hidden, bool changePosition=false)
    {
        _hidden = hidden;
        _collider.enabled = !hidden;
        if (changePosition)
        {
            float z = (hidden) ? _hiddenZ : _unhiddenZ;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }
    }

    public void Unhide()
    {
        if (_hidden)
        {
            iTween.MoveTo(gameObject, iTween.Hash("z", _unhiddenZ, "time", 0.5f, "easetype", iTween.EaseType.easeInExpo));
            SetHiddenState(!_hidden);
        }
    }

    public void Hide()
    {
        if (!_hidden)
        {
            iTween.MoveTo(gameObject, iTween.Hash("z", _hiddenZ, "time", 0.5f, "easetype", iTween.EaseType.easeInExpo));
            SetHiddenState(!_hidden);
        }
    }

}
