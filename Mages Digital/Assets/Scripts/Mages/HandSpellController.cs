using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class HandSpellController : MonoBehaviour
{
    [SerializeField] private HandController _hand;
    [SerializeField] private MageController _mage;

    [SerializeField] private SpellCard source;
    [SerializeField] private SpellCard quality;
    [SerializeField] private SpellCard delivery;

    public HandController hand => _hand;
    public MageController mage => _mage;

    private List<SpellCard> spellCards = new List<SpellCard>();
    
    void Awake()
    {
        _hand = gameObject.GetComponentInParent<HandController>();
        _mage = _hand.mage;
    }

    public void Reset()
    {
        spellCards = new List<SpellCard>();
    }

    public void SetCardToOrder(SpellCard card, Order order)
    {
        switch (order)
        {
            case Order.SOURCE:
                source = card;
                break;
            
            case Order.QUALITY:
                quality = card;
                break;
            
            case Order.DELIVERY:
                delivery = card;
                break;
        }
    }

    public void PrepareSpellCards()
    {
        spellCards.Clear();
        if (source != null)
            spellCards.Add(source);
        if (quality != null)
            spellCards.Add(quality);
        if (delivery != null)
            spellCards.Add(delivery);
    }

    public bool IsExecutable()
    {
        return spellCards.Count > 0;
    }

    public int GetSpellCount()
    {
        return spellCards.Count;
    }

    public int GetSpellInitiative()
    {
        int initiative = 0;
        foreach (SpellCard card in spellCards)
        {
            initiative += card.initiative;
        }
        return initiative;
    }
}
