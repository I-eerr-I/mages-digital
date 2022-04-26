using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class HandController : MonoBehaviour
{
    
    public int handSize = 8;

    public MageController       mageController;        // маг данной руки
    public HandSpellController  spellController;  // заклинание данной руки

    // карты руки
    public List<Card> sources = new List<Card>();
    public List<Card> qualities = new List<Card>();
    public List<Card> deliveries = new List<Card>();
    public List<Card> wildMagics = new List<Card>();
    public List<Card> treasures = new List<Card>();
    public List<Card> deads = new List<Card>();

    // количество заклинаний
    public int spellsCount     => sources.Count + qualities.Count + deliveries.Count + wildMagics.Count;

    void Awake()
    {
        mageController  = gameObject.GetComponentInParent<MageController>();
        spellController = gameObject.GetComponentInChildren<HandSpellController>();
    }


    // добавить карту к руке
    public void AddCard(Card card)
    {
        List<Card> deck = GetDeckOfCardType(card);
        
        if (card.cardType == CardType.SPELL)
            AddSpellCard(deck, (SpellCard) card);
        else
            deck?.Add(card);
    }

    public void AddSpellCard(List<Card> deck, SpellCard card)
    {
        // XXX потенциально медленное решение (сортирует заклинания, при добавлении нового)
        deck.Add(card);
        deck.Sort((c1, c2) => ((SpellCard)c1).sign.CompareTo(((SpellCard)c2).sign));
    }

    // получить список заклинаний для порядка определенного типа
    public List<Card> GetDeckOfOrderType(Order order)
    {
        List<Card> deck = null;
        switch (order)
        {
            case Order.SOURCE:
                deck = sources;
                break;
            
            case Order.QUALITY:
                deck = qualities;
                break;
            
            case Order.DELIVERY:
                deck = deliveries;
                break;
            
            case Order.WILDMAGIC:
                deck = wildMagics;
                break;
        }
        return deck;
    }

    // получить список карт для карты определенного типа
    List<Card> GetDeckOfCardType(Card card)
    {
        List<Card> deck = null;
        switch (card.cardType)
        {
            case CardType.SPELL:
                deck = GetDeckOfOrderType(((SpellCard) card).order);
                break;
            
            case CardType.TREASURE:
                deck = treasures;
                break;

            case CardType.DEAD:
                deck = deads;
                break;
        }
        return deck;
    }


}
