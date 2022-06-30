using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public enum CardState
    {
        IN_HAND,    // в руке (есть владелец карты)
        IN_SPELL,   // в заклинании (есть владелец карты)
        NO_OWNER    // без владельца
    }  

    public enum SpellsToAdd
    {
        SOURCES,
        WITH_SAME_SIGNS_IN_SPELL
    }

}