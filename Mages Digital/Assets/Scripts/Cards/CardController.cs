using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [SerializeField] private Card _card;
    public Card card
    {
        get => _card;
    }

    
}
