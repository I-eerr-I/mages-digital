using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Dead", fileName="Dead Card", order=5)]
public class DeadCard : Card
{
    

    void Reset()
    {
        _cardType = CardType.DEAD;
    }


}
