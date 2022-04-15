using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{

    [SerializeField] private List<Card>  _deck;     // список карт колоды
    [SerializeField] private CardType    _cardType; // тип карт в колоде
    
    private Random _random = new Random();
    
    // TEST
    [SerializeField] private Color _color;
    private SpriteRenderer _spriteRenderer;
    // TEST

    
    public CardType cardType => _cardType;

    public int Count => _deck.Count;        // количество карт в колоде


    void Awake()
    {
        // TEST
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>(); 
        _spriteRenderer.color  = _color;
        // TEST

        if (_cardType == CardType.SPELL) DoubleDeck();

        ShuffleDeck();
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
        return card;
    }

    // перемешать колоду
    void ShuffleDeck()
    {
        _deck = _deck.OrderBy(a => _random.Next()).ToList();
    }

    // удвоить карты в колоде
    void DoubleDeck()
    {
        _deck.AddRange(_deck);
    }

}
