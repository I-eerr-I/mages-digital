using System;
using UnityEngine.Events;

namespace CardsToolKit
{

    public enum Order
    {
        SOURCE,     // заводила
        QUALITY,    // наворот
        DELIVERY,   // приход
        WILDMAGIC   // шальная магия
    }

    public enum Sign
    {
        ARCANE,     // порча
        PRIMAL,     // трава
        ELEMENTAL,  // угар
        DARK,       // мрак
        ILLUSION    // кумар
    }

    public enum CardType
    {
        SPELL,      // заклинание
        TREASURE,   // сокровище
        DEAD        // дохлый колдун
    }

    [Serializable] public class Spell : UnityEvent {}


}