using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseEventsCardTrigger : MonoBehaviour
{
    private CardController _cardController;

    void Awake()
    {
        _cardController = gameObject.GetComponentInParent<CardController>();
    }

    void OnMouseOver()
    {
        _cardController.OnMouseOverTrigger();
    }

    void OnMouseExit()
    {
        _cardController.OnMouseExitTrigger();
    }

    void OnMouseDown()
    {
        _cardController.OnMouseDownTrigger();
    }

}
