using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Spell Card")]
public class SpellCard : Card
{
    [SerializeField] private int _initiative = 0;
    public int initiative
    {
        get => _initiative;
    }

    [SerializeField] protected Order _order;
    public Order order
    {
        get => _order;
    }

    [SerializeField] private Sign _sign;
    public Sign sign
    {
        get => _sign;
    }
}
