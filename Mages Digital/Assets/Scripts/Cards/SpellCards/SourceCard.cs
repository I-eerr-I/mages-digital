using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Source Card")]
public class SourceCard : SpellCard
{
    private void Reset()
    {
        _order = Order.SOURCE;
    }
}
