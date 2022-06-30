using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Delivery", fileName="Delivery Card", order=3)]
public class DeliveryCard : SpellCard
{


    private void Reset()
    {
        _order = Order.DELIVERY;
    }


}
