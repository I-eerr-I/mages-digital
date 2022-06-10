using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class EnemyController : AbstractPlayerController
{
    
    float _cardMovingToHandTime   = 0.25f;
    float _cardMovingToHandDeltaY = 0.5f; 



    void Start()
    {
        _isBot = true;
    }


    public override IEnumerator OnCardAddedToHand(CardController card)
    {
        SetCardHandParent(card);
        // card.SetUndiscoverable();

        StartCoroutine(card.PositionBackUp());
        
        Vector3 position = GetHandPosition();
        
        iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", _cardMovingToHandTime));
        yield return new WaitForSeconds(_cardMovingToHandTime);

        card.SetVisible(false);
    }


    protected override IEnumerator MoveSpellGroup(bool toHand)
    {
        Transform parent = (toHand) ? _spellLocation : GameManager.instance.spellGroupLocation;
        
        float step = _mage.nonNullSpell[0].cardSizeX;
        float x = -(step * (_mage.nCardsInSpell-1)) / 2;
        
        Vector3 spellGroupPosition = GameManager.instance.spellGroupLocation.position;
        
        foreach (CardController cardInSpell in _mage.nonNullSpell)
        {   
            if (!toHand)
                cardInSpell.SetVisible(true);

            spellGroupPosition.x = x;
            Vector3 position = (toHand) ? GetHandPosition() : spellGroupPosition;

            cardInSpell.transform.SetParent(parent);
            
            iTween.MoveTo(cardInSpell.gameObject, iTween.Hash("position", position, "time", _spellGroupMovingTime));
            x += step;
        }

        yield return new WaitForSeconds(_spellGroupMovingTime);

        if (toHand)
            _mage.nonNullSpell.ForEach(card => card.SetVisible(false));
    }

    
    Vector3 GetHandPosition()
    {
        Vector3 position = _mage.mageIcon.transform.position;
        position.y      -= _cardMovingToHandDeltaY;
        return position;
    }

}
