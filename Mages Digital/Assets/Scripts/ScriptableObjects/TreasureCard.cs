using CardsToolKit;
using UnityEngine;

[CreateAssetMenuAttribute(menuName="Cards/Treasure", fileName="Treasure Card", order=4)]
public class TreasureCard : Card
{


    void Reset()
    {
        _cardType = CardType.TREASURE;
    }


}
