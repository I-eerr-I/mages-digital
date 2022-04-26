using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class HandSpellController : MonoBehaviour
{
    public HandController hand;

    public SpellCard source;
    public SpellCard quality;
    public SpellCard delivery;

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
}
