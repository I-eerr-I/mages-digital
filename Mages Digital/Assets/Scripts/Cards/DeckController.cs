using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{

    private Random _random = new Random();
    
    [SerializeField] private List<Card>  _deck = new List<Card>();     // список карт колоды
    [SerializeField] private List<Card>  _fold = new List<Card>();     // сброс карт
    
    private CardType _cardsType; // тип карт в колоде


    public CardType cardsType => _cardsType;         
    public int      Count     => _deck.Count;        // количество карт в колоде


    void Awake()
    {
        _cardsType = _deck[0].cardType;
        if (_cardsType == CardType.SPELL) DoubleDeck();
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

}
