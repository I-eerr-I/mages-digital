using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MageController
{

    // TEST
    public override IEnumerator DrawCards(DeckController deck, int amount)
    {
        Debug.Log("PLAYER DRAW FROM " + deck.ToString());
        return base.DrawCards(deck, amount);
    }
    // TEST

}
