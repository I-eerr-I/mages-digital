using CardsToolKit;
using UnityEngine;

public class SpellCard : Card
{

    [SerializeField] protected int   _initiative;
    [SerializeField] protected Order _order;
    [SerializeField] protected Sign _sign;
    

    public Order order => _order;
    public Sign sign => _sign;


    void Reset()
    {
        _initiative = 0;
        _cardType   = CardType.SPELL;
    }

}
