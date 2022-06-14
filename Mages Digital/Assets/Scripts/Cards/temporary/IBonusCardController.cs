using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBonusCardController : ICardController
{
    
    int _bonusInfoIndexOffset = 0;                  // нужна для перебора карт сокровищ при наведении на них

    void Update()
    {
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseScroll) > 0.0f)
        {
            int addToOffset = (mouseScroll > 0.0f) ? 1 : -1;
            _bonusInfoIndexOffset += addToOffset;
        }
    }

    public override void SetupCard(Card card, DeckController deck = null, Sprite back = null)
    {
        // данные карты
        _card = card;
        // колода карты
        _sourceDeck = deck;
        // передняя часть карты
        _frontSpriteRenderer.sprite = card.front;
        // задняя часть карты
        if (back != null)
            _backSpriteRenderer.sprite  = back;
        // подсветка карты
        Color cardColor;
        cardColor = CARD_MAIN_COLOR[card.cardType];
        _outlineController.SetColor(cardColor);
    }


    public override void OnMouseOverAction()
    {
        ShowCardInfo();
    }

    public override void OnMouseExitAction()
    {
        HideCardInfo();
    }

    public override void OnMouseDownAction()
    {
        if (gm.isChoosingState)
        {
            gm.StopChoosing();
        }
    }

    protected override void ShowCardInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= _cardShowInfoWaitTime)
            UIManager.instance.ShowBonusInfo(owner.GetBonusInfo(indexOffset: _bonusInfoIndexOffset));
    }

    // спрятать информацию о бонусных картах в руке
    protected override void HideCardInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowBonusInfo(null, false);
    }



}
