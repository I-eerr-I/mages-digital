using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // маг данной руки
    [SerializeField] private MageController      _mageController;
    // заклинание данной руки
    [SerializeField] private HandSpellController _spellController;

    // карты руки
    private List<Card> _sources    = new List<Card>();
    private List<Card> _qualities  = new List<Card>();
    private List<Card> _deliveries = new List<Card>();
    private List<Card> _wildMagics = new List<Card>();
    private List<Card> _treasures  = new List<Card>();
    private List<Card> _deads      = new List<Card>();

    // количество заклинаний
    public int spellsCount
    {
        get => _sources.Count + _qualities.Count + _deliveries.Count + _wildMagics.Count;
    }

    // добавить карту к руке
    public void AddCard(Card card)
    {
        List<Card> deck = GetDeckOfCardType(card);
        
        deck?.Add(card);
        
        // XXX потенциально медленное решение (сортирует заклинания, при добавлении нового)
        if (card is SpellCard)
            deck.Sort((c1, c2) => ((SpellCard) c1).sign.CompareTo(((SpellCard) c2).sign));
    }

    // получить список карт для карты определенного типа
    List<Card> GetDeckOfCardType(Card card)
    {
        List<Card> deck = null;
        bool isSpellCard = card is SpellCard;
        if (isSpellCard)
        {
            deck = GetDeckOfOrderType(((SpellCard) card).order);
        }
        else if (card is TreasureCard)
        {
            deck = _treasures;
        }
        else if (card is DeadCard)
        {
            deck = _deads;
        }
        return deck;
    }

    // получить список заклинаний для порядка определенного типа
    List<Card> GetDeckOfOrderType(Order order)
    {
        List<Card> deck = null;
        switch (order)
        {
            case Order.SOURCE:
                deck = _sources;
                break;
            
            case Order.QUALITY:
                deck = _qualities;
                break;
            
            case Order.DELIVERY:
                deck = _deliveries;
                break;
            
            case Order.WILDMAGIC:
                deck = _wildMagics;
                break;
        }
        return deck;
    }

}