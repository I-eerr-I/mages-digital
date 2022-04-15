using CardsToolKit;
using UnityEngine;

public class SpellCard : Card
{
    [SerializeField] protected int _initiative;
    [SerializeField] protected Order _order;
    public Order order => _order;

    [SerializeField] protected Sign _sign;
    public Sign sign => _sign;

    void Reset()
    {
        _initiative = 0;
    }
}
