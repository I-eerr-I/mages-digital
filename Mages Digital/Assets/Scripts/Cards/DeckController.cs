using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    [SerializeField] private List<Card>  _deck;
    public int Count => _deck.Count;
    
    [SerializeField] private Color       _color; // TEST
    private SpriteRenderer _spriteRenderer; // TEST

    private Random _random = new Random();    

    void Awake()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>(); // TEST
        _spriteRenderer.color  = _color; // TEST

        if (gameObject.tag == "Spells Deck") DoubleDeck();

        ShuffleDeck();
    }

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

    void ShuffleDeck()
    {
        _deck = _deck.OrderBy(a => _random.Next()).ToList();
    }

    void DoubleDeck()
    {
        _deck.AddRange(_deck);
    }
}
