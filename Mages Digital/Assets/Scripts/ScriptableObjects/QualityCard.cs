using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Quality", fileName="Quality Card", order=2)]
public class QualityCard : SpellCard
{


    private void Reset()
    {
        _order = Order.QUALITY;
    }


}
