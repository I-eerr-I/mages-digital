using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SourceCard : SpellCard
{
    public SourceCard() 
    {
        this.order = Order.SOURCE;
    }
}
