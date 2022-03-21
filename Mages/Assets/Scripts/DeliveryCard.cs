using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DeliveryCard : SpellCard
{

    [Range(0, 18)]
    public int initiative;

    public DeliveryCard() 
    {
        this.order = Order.DELIVERY;
    }

}
