using UnityEngine;
using UnityEngine.Events;

public abstract class SpellCard : Card
{
    public enum Order
    {
        SOURCE,
        QUALITY,
        DELIVERY,
        WILDMAGIC
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

    [Range(0, 18)]
    public int   initiative;
    
}
