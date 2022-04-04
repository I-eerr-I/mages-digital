using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Delivery Card")]
public class DeliveryCard : SpellCard
{
    private void Reset()
    {
        _order = Order.DELIVERY;
    }
}
