using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Source", fileName="Source Card", order=1)]
public class SourceCard : SpellCard
{
    private void Reset()
    {
        _order = Order.SOURCE;
    }
}
