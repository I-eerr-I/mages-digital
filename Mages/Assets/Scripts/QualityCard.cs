using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QualityCard : SpellCard
{
    public QualityCard() 
    {
        this.order = Order.QUALITY;
    }
}
