using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellCard : Card
{
    public enum Order
    {
        SOURCE,
        QUALITY,
        DELIVERY
    }

    public enum Sign
    {
        ARCANE,
        DARK,
        ELEMENTAL,
        ILLUSION,
        PRIMAL
    }

    public Order order;    
    public Sign  sign;
}
