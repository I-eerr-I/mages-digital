using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{

    [SerializeField] private List<Card>  _deck = new List<Card>();     // список карт колоды
    [SerializeField] private List<Card>  _fold = new List<Card>();     // сброс карт
    
    private CardType    _cardType; // тип карт в колоде
    
    private Random _random = new Random();
    
    public GameObject cardToPass;

    public CardType cardType => _cardType;
    public int Count => _deck.Count;        // количество карт в колоде


    void Awake()
    {
        _cardType = _deck[0].cardType;
        if (_cardType == CardType.SPELL) DoubleDeck();
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
        Shuffle();
    }

    public void HideCardToPass()
    {
        cardToPass.transform.position = new Vector3(0, 0, -20);
    }

    // удвоить карты в колоде
    void DoubleDeck()
    {
        List<Card> deckToAdd = new List<Card>(_deck);
        if (_cardType == CardType.SPELL)
            deckToAdd = _deck.FindAll((card) => ((SpellCard)card).order != Order.WILDMAGIC);
        _deck.AddRange(deckToAdd);
    }

}
