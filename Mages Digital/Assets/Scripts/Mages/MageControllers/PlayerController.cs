using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MageController
{

    public Transform handLocation;

    // TEST
    public override IEnumerator DrawCards(DeckController deck, int amount)
    {
        Debug.Log("PLAYER DRAW FROM " + deck.ToString());
        return base.DrawCards(deck, amount);
    }
    // TEST

    public override Card TakeCard(DeckController deck)
    {
        Card card = base.TakeCard(deck);
        GameObject cardObject = Instantiate(cardPrefab, handLocation);
        CardController cardController = cardObject.GetComponent<CardController>();
        cardController.card = card;
    }

}
