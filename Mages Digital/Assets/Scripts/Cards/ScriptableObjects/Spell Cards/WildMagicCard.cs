using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Wild Magic", fileName="Wild Magic Card", order=6)]
public class WildMagicCard : SpellCard
{
    private void Reset()
    {
        _order = Order.WILDMAGIC;
    }
}
