using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpellController : MonoBehaviour
{
    private HandController _hand;
    public  HandController hand
    {
        get => _hand;
    }

    private CardController source;
    private CardController quality;
    private CardController delivery;
}
