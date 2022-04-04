using System;
using UnityEngine.Events;

namespace CardsToolKit
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
        PRIMAL,
        ELEMENTAL,
        DARK,
        ILLUSION
    }

    [Serializable] public class Spell : UnityEvent {}

}