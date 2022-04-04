using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Quality Card")]
public class QualityCard : SpellCard
{
    private void Reset()
    {
        _order = Order.QUALITY;
    }
}
