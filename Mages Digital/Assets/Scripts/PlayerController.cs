using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private MageController _mage;

    [Header("All hand cards constant position parameters")]
    public float handCardsZ =  5.5f;
    public float handCardsY = -2.75f;
    
    [Header("Moving cards to player")]
    public Vector3 cardMovingDestination = new Vector3(0.0f, -5.5f, 7.5f);
    public float cardMovingTime = 1.0f;

    [Header("Spell cards variable position paramters")]
    public float spellsStartX        = 0.0f;
    public float spellsRightMaxX     = 2.5f; 
    public float spellsSelectedZ     = 5.25f;
    public float spellsSelectedY     = -2.5f;
    
    [Header("Cards in-hand rotation")]
    public float spellsRotLeftMaxZ   = 30.0f; 
    public float spellsRotEllipseR   = 0.25f;
    public bool  rotateSpells        = true;

    [Header("Bonus cards variable position parameters")]
    public float bonusesStartX   = 4.5f;


    public MageController mage => _mage;

    void Awake()
    {
        _mage = gameObject.GetComponent<MageController>();
        _mage.owner = this;
    }

    void Update()
    {
        // TEST
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(FitSpellCards());
        }
        // TEST
    }

    public IEnumerator AddToHand(CardController cardController)
    {
        cardController.transform.SetParent(transform);
        float x = cardController.IsSpell() ? spellsStartX : bonusesStartX;
        Vector3 position = GetHandPositionVector(x);
        iTween.MoveTo(cardController.gameObject, iTween.Hash("position", position, "time", 1.0f, "islocal", true));
        iTween.RotateTo(cardController.gameObject, iTween.Hash("rotation", new Vector3(0.0f, 0.0f, 0.0f), "time", 0.15f, "islocal", true));
        if (cardController.IsSpell())
            yield return FitSpellCards();
        yield break;
    }

    public IEnumerator FitSpellCards()
    {
        List<CardController> spellCards = _mage.GetSpells();
        float gap    = (spellsRightMaxX * 2.0f) / (spellCards.Count + 1);
        float rotGap = (spellsRotLeftMaxZ * 2.0f) / (spellCards.Count + 1);
        float x      = -spellsRightMaxX + gap;
        float rotZ   = spellsRotLeftMaxZ - rotGap;
        print("FITTING");
        for (int i = 0; i < spellCards.Count; i++)
        {
            CardController card = spellCards[i];

            card.frontSpriteRenderer.sortingOrder = i;

            Vector3 position = GetHandPositionVector(x);
        
            if (rotateSpells)
            {
                position.y += spellsRotEllipseR*Mathf.Cos(x);
                Vector3 rotation = new Vector3(0.0f, 0.0f, rotZ);
                iTween.RotateTo(card.gameObject, iTween.Hash("rotation", rotation, "time", 0.15f, "islocal", true));
                rotZ -= rotGap;
            }

            print($"{i} {position.x}");
            iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", 0.15f, "islocal", true));
            x    += gap;
        }
        yield break;
    }

    Vector3 GetHandPositionVector(float x)
    {
        return new Vector3(x, handCardsY, handCardsZ);
    }

    public void OnSpellCardSelected(CardController cardController, bool isSelected)
    {
        if (cardController.IsSpell() && cardController.inHand)
        {
            float y = (isSelected) ? spellsSelectedY : handCardsY;
            float z = (isSelected) ? spellsSelectedZ : handCardsZ;
            float x = cardController.transform.position.x;
            if (rotateSpells) y += 0.25f * Mathf.Cos(x);
            iTween.MoveTo(cardController.gameObject, iTween.Hash("position", new Vector3(x, y, z), "time", 0.15f, "islocal", true));
        }
    }

    public IEnumerator OnSpellCardAddedToSpell(CardController addedCard, Order order)
    {
        addedCard.transform.SetParent(GameManager.instance.fieldCenter);
        Transform orderLocation = GetLocationOfOrder(order);
        iTween.MoveTo(addedCard.gameObject, iTween.Hash("position", orderLocation, "time", 0.25f));
        yield return new WaitForSeconds(0.25f);
        addedCard.RotateFrontUp();
        yield return FitSpellCards();
    }

    public Transform GetLocationOfOrder(Order order)
    {
        switch (order)
        {
            case Order.SOURCE:
                return GameManager.instance.sourceLocation;
            case Order.QUALITY:
                return GameManager.instance.qualityLocation;
            case Order.DELIVERY:
                return GameManager.instance.deliveryLocation;
        }
        return null;
    }
}
