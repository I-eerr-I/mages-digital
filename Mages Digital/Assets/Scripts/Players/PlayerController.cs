using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class PlayerController : AbstractPlayerController
{



////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // РУКА
    bool _handIsHidden   = false;  // видимость карт руки
    float _handHiddenY   = -1.5f;   // локальная координата Y руки в спрятанном состоянии
    float _handUnhiddenY = 0.0f;    // локальная координата Y руки в не спрятанном состоянии
    float _handHideTime  = 1.0f;    // время скрытия руки
    float _handCardsZ    =  5.5f;    // локальная координата Z в руке по-умолчанию
    float _handCardsY    = -2.75f;   // локальная координата Y в руке по-умолчанию


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // ОБЩЕЕ РАСПОЛОЖЕНИЕ КАРТ В РУКЕ
    Vector3 _cardMovingDestination       = new Vector3(0.0f, -5.5f, 7.5f); // глобальная позиция, куда карта перемещается после спауна
    float   _bonusCardMovingToHandTime   = 1.0f;  // время перемещения карты к руке
    float   _cardMovingToSpellTime       = 0.25f; // время перемещения карты к заклинанию
    float   _cardAvgFittingTime          = 0.15f; // среднее время выравнивания карты заклинания в руке


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // РАСПОЛОЖЕНИЕ КАРТ ЗАКЛИНАНИЙ В РУКЕ
    float _spellsRightMaxX      = 3.0f;     // локальное крайнее правое зачение X для заклинаний
    float _spellsSelectedZ      = 5.25f;    // локальная координата Z при наведении на карту
    float _spellsSelectedY      = -2.5f;    // локальная координата Y при наведении на карту

    // НАКЛОН КАРТ ЗАКЛИНАНИЙ В РУКЕ
    float _spellsRotLeftMaxZ    = 30.0f;    // максимальный угол наклона влево
    float _spellsRotEllipseR    = 0.25f;    // радиус (высота) эллипса при наклоне
    bool  _rotateSpells         = false;     // применять ли наклон карт в руке

    // АНИМАЦИЯ ПОДТВЕРЖДЕНИЯ ЗАКЛИНАНИЯ
    float _spellUpY             = 1.5f;  // координата Y при подъеме карты вверх
    float _spellDownY           = 1.0f;  // координата Y при опускании карты вниз
    float _rightLeftSpellX      = 2.0f;  // насколько в сторону отоднинуть правую и левую карты
    float _spellUpTime          = 1.0f;  // время подъема карт
    float _rightLeftSpellEndX   = 1.25f; // конечная глобальная координата X
    float _spellDownTime        = 0.25f;  // время падения карт
    Vector3 _shakeAmount        = new Vector3(1.0f, 1.0f, 1.0f); // диапазон тряски
    float _shakeTime            = 0.25f; // время тряски после готовности заклинания


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // БОНУСНЫЕ КАРТЫ
    float _bonusesStartX  = 4.5f; // локальная координата X в руке по-умолчанию для бонусных карт
    int   _bonusInfoIndexOffset = 0;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // ВЫБОР
    float _choosingFieldYDelta = 20.0f;
    float _choosingFieldMovingTime = 1.0f;
    float _handChoosingY = 10.0f;
    float _handChoosingZ = 1.5f;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public bool readyToExecute => _mage.readyToExecute;
    
    public Transform fieldCenter => GameManager.instance.fieldCenter;
    public Transform outOfField  => GameManager.instance.outOfField;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Update()
    {
        if (Input.GetButton("Execute"))
        {
            StartCoroutine(OnExecute());
        }

        if (GameManager.instance.isChoosingState)
        {
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(mouseScroll) > 0.0f)
            {
                int addToOffset = (mouseScroll > 0.0f) ? 1 : -1;
                _bonusInfoIndexOffset += addToOffset;
            }
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public override IEnumerator OnCardAddedToHand(CardController card)
    {
        SetCardHandParent(card);

        StartCoroutine(card.PositionFrontUp());

        Vector3 position = GetHandPositionVector(_bonusesStartX);

        if (!card.isSpell)
            iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", _bonusCardMovingToHandTime, "islocal", true));
        else
            yield return FitSpellCardsInHand();
        yield break;
    }


    protected override IEnumerator MoveSpellGroup(bool toHand = false)
    {
        if (mage.nonNullSpell.Count > 0)
        {
            Transform parent = (toHand) ? _spellLocation : GameManager.instance.spellGroupLocation;
            
            float step = _mage.nonNullSpell[0].cardSizeX;
            float x = -(step * (_mage.nCardsInSpell-1)) / 2;
            
            Vector3 spellGroupPosition = GameManager.instance.spellGroupLocation.position;
            
            foreach (CardController cardInSpell in _mage.nonNullSpell)
            {   
                spellGroupPosition.x = x;
                Vector3 position = (toHand) ? GetHandPositionVector(x) : spellGroupPosition;
                
                cardInSpell.transform.SetParent(parent);
                
                iTween.MoveTo(cardInSpell.gameObject, iTween.Hash("position", position, "time", _spellGroupMovingTime, "islocal", toHand));
                x += step;
            }
            
            yield return new WaitForSeconds(_spellGroupMovingTime);
        }
    }

    protected override IEnumerator MoveCard(CardController card, bool toHand = false)
    {

        if (toHand)
        {
            yield return OnCardAddedToHand(card);
        }
        else
        {
            SetCardHandParent(card);            
            iTween.MoveTo(card.gameObject, iTween.Hash("position", GameManager.instance.spellGroupLocation.position, "time", _spellGroupMovingTime));
            yield return new WaitForSeconds(_spellGroupMovingTime);
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public override IEnumerator OnSpellDrop()
    {
        if (_handIsHidden)
            yield return HideHand(false);
    }


    public override IEnumerator OnCardAddedToSpell(CardController addedCard, Order order)
    {
        addedCard.transform.SetParent(GameManager.instance.spellGroupLocation);

        Transform orderLocation = GetLocationOfOrder(order);

        iTween.MoveTo(addedCard.gameObject, iTween.Hash("position", orderLocation.position, "time", _cardMovingToSpellTime));
        yield return new WaitForSeconds(_cardMovingToSpellTime);
        yield return addedCard.PositionFrontUp();
        yield return FitSpellCardsInHand();
    }


    public override IEnumerator ChooseOrder()
    {
        _chosenOrder = Order.WILDMAGIC;

        // все котроллеры мест расположения карт заклинаний
        List<SpellLocationController> controllers = GameManager.instance.spellLocationControllers;
        
        // запустить выбор расположения карты
        _mage.SetAllDiscoverable(false);
        foreach (SpellLocationController controller in controllers)
            yield return controller.StartChoice();
        _mage.SetAllDiscoverable(true);

        // ожидать выбора игрока
        yield return new WaitWhile(() => GameManager.instance.isChoosingState && controllers.TrueForAll(x => !x.isOrderChosen));
        
        // отключить выбор
        _mage.SetAllDiscoverable(false);
        foreach(SpellLocationController controller in controllers)
            yield return controller.EndChoice();
        yield return new WaitForSeconds(0.05f);
        _mage.SetAllDiscoverable(true);

        // сохранить выбор игрока
        if (GameManager.instance.isChoosingState)
            _chosenOrder = controllers.FindAll(x => x.isOrderChosen)[0].chosenOrder;
    }

    public override IEnumerator ChooseTreasure(bool hasChoiceNotToDrop)
    {
        GameManager.instance.SetChoosingState();

        yield return new WaitWhile(() => {
            UIManager.instance.ShowBonusInfo(
                _mage.GetBonusInfo(_bonusInfoIndexOffset), 
                withDropButton: true, 
                withCancleButton: hasChoiceNotToDrop, 
                choosingMage: _mage
            );
            return _mage.chosenCard == null && GameManager.instance.isChoosingState;
        });

        UIManager.instance.ShowBonusInfo(null, show: false);
    }

    public override IEnumerator ChooseTreasureFromMage(MageController mage, string actionText)
    {
        GameManager.instance.SetChoosingState();

        List<CardController> treasures = mage.treasures;

        yield return new WaitWhile(() => {
            UIManager.instance.ShowBonusInfo(
                _mage.GetBonusInfoFromList(treasures, _bonusInfoIndexOffset), 
                withDropButton: true, 
                choosingMage: _mage,
                dropButtonText: actionText
            );
            return _mage.chosenCard == null && GameManager.instance.isChoosingState;
        });

        UIManager.instance.ShowBonusInfo(null, show: false);
    }

    public override IEnumerator ChooseCardFromSpell(List<CardController> spellCards)
    {
        GameManager.instance.SetChoosingState();

        Vector3 oldFieldPosition = fieldCenter.position;
        
        Vector3 newFieldPosition = oldFieldPosition;
        newFieldPosition.y += _choosingFieldYDelta;

        Transform myParent = transform.parent;

        List<Transform> cardParents = new List<Transform>();
        foreach (CardController card in spellCards)
        {
            cardParents.Add(card.transform.parent);
            card.transform.SetParent(outOfField);
        }
        transform.SetParent(fieldCenter);

        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", newFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime);

        spellCards.ForEach(card => card.OnChoosingCardState());
        yield return new WaitWhile(() => _mage.chosenCard == null);
        spellCards.ForEach(card => card.OnChoosingCardStateEnd());

        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", oldFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime);

        for (int i = 0; i < spellCards.Count; i++)
            spellCards[i].transform.SetParent(cardParents[i]);

        transform.SetParent(myParent);

        GameManager.instance.StopChoosing();

    }

    public override IEnumerator ChooseCardFromHand()
    {
        GameManager.instance.SetChoosingState();

        Vector3 oldFieldPosition = fieldCenter.position;
        
        Vector3 newFieldPosition = oldFieldPosition;
        newFieldPosition.y += _choosingFieldYDelta;

        Transform myParent = transform.parent;

        Transform handParent = _handLocation.parent;
        _handLocation.SetParent(outOfField);
        transform.SetParent(fieldCenter);


        Vector3 oldHandPosition = _handLocation.position;
        iTween.MoveTo(_handLocation.gameObject, iTween.Hash("position", new Vector3(0.0f, _handChoosingY, _handChoosingZ), "time", _choosingFieldMovingTime));
        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", newFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime);

        List<CardController> hand = _mage.GetSpellsInHand();
        hand.ForEach(card => card.OnChoosingCardState());
        yield return new WaitWhile(() => _mage.chosenCard == null);
        hand.ForEach(card => card.OnChoosingCardStateEnd());

        _handLocation.SetParent(handParent);

        iTween.MoveTo(_handLocation.gameObject, iTween.Hash("position", oldHandPosition, "time", _choosingFieldMovingTime));
        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", oldFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime + 0.5f);
        transform.SetParent(myParent);

        GameManager.instance.StopChoosing();
    }

    public override IEnumerator ChooseTarget(List<MageController> mages)
    {
        GameManager.instance.SetChoosingState();

        Transform fieldCenter = GameManager.instance.fieldCenter;
        Vector3 oldFieldPosition = fieldCenter.position;
        
        Vector3 newFieldPosition = oldFieldPosition;
        newFieldPosition.y += _choosingFieldYDelta;

        Transform outOfField  = GameManager.instance.outOfField;
        Transform myParent = transform.parent;
        
        List<Transform> enemyParents = new List<Transform>();
        foreach (MageController enemy in mages)
        {
            enemyParents.Add(enemy.transform.parent);
            enemy.transform.SetParent(outOfField);
        }
        transform.SetParent(fieldCenter);
        
        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", newFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime);

        
        mages.ForEach(enemy => enemy.mageIcon.OnChoosingEnemyState());
        yield return new WaitWhile(() => _mage.chosenMage == null);
        mages.ForEach(enemy => enemy.mageIcon.OnChoosingEnemyStateEnd());


        iTween.MoveTo(fieldCenter.gameObject, iTween.Hash("position", oldFieldPosition, "time", _choosingFieldMovingTime));
        yield return new WaitForSeconds(_choosingFieldMovingTime);

        for (int i = 0; i < mages.Count; i++)
            mages[i].transform.SetParent(enemyParents[i]);

        transform.SetParent(myParent);

        GameManager.instance.StopChoosing();
    }

    
    public override void OnSpellCardSelected(CardController cardController, bool isSelected)
    {
        if (cardController.isSpell && cardController.inHand)
        {
            float y = (isSelected) ? _spellsSelectedY : _handCardsY;
            float z = (isSelected) ? _spellsSelectedZ : _handCardsZ;
            float x = cardController.transform.position.x;
            if (_rotateSpells) y += 0.25f * Mathf.Cos(x);
            iTween.MoveTo(cardController.gameObject, iTween.Hash("position", new Vector3(x, y, z), "time", _cardAvgFittingTime, "islocal", true));
            if (!isSelected && cardController.isSpell)
                StartCoroutine(FitSpellCardsInHand());
        }
    }


    public override void OnMageReset()
    {
        StartCoroutine(HideHand(false));
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // реакция на подтверждение карт в заклинании
    IEnumerator OnExecute()
    {
        if (_mage.spellIsReady && !_mage.readyToExecute)
        {
            yield return LockSpell();
        }
        yield break;
    }


    // выровнять карты заклинаний в руке
    IEnumerator FitSpellCardsInHand()
    {
        List<CardController> spellCards = _mage.GetSpellsInHand();
        float step    = (_spellsRightMaxX * 2.0f) / (spellCards.Count + 1);
        float rotStep = (_spellsRotLeftMaxZ * 2.0f) / (spellCards.Count + 1);
        float x      = -_spellsRightMaxX + step;
        float z      = _handCardsZ;
        float rotZ   = _spellsRotLeftMaxZ - rotStep;
        for (int i = 0; i < spellCards.Count; i++)
        {
            CardController card = spellCards[i];

            card.transform.SetSiblingIndex(i);
            card.frontSpriteRenderer.sortingOrder = i;

            Vector3 position = new Vector3(x, _handCardsY, _handCardsZ);
        
            if (_rotateSpells)
            {
                position.y += _spellsRotEllipseR*Mathf.Cos(x);
                Vector3 rotation = new Vector3(0.0f, 0.0f, rotZ);
                iTween.RotateTo(card.gameObject, iTween.Hash("rotation", rotation, "time", _cardAvgFittingTime, "islocal", true));
                rotZ -= rotStep;
            }
            iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", _cardAvgFittingTime, "islocal", true));
            
            z += 0.01f;
            x    += step;
        }
        yield break;
    }

    // анимация готовности карт заклинания
    IEnumerator LockSpell()
    {
        _mage.SetAllDiscoverable(false);

        foreach (CardController spellCard in _mage.nonNullSpell)
        {
            spellCard.SetUndiscoverable();
            Hashtable hashtable = new Hashtable();
            hashtable.Add("time", _spellUpTime);
            hashtable.Add("y", _spellUpY);
            if (spellCard.spellOrder != Order.QUALITY)
            {
                float x = (spellCard.spellOrder == Order.SOURCE) ? -_rightLeftSpellX : _rightLeftSpellX;
                hashtable.Add("x", x);
            }
            iTween.MoveTo(spellCard.gameObject, hashtable);
        }

        yield return new WaitForSeconds(_spellUpTime);

        foreach (CardController spellCard in _mage.nonNullSpell)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("time", _spellDownTime);
            hashtable.Add("y", _spellDownY);
            if (spellCard.spellOrder != Order.QUALITY)
            {
                float x = (spellCard.spellOrder == Order.SOURCE) ? -_rightLeftSpellEndX : _rightLeftSpellEndX;
                hashtable.Add("x", x);
            }
            hashtable.Add("easetype", iTween.EaseType.easeOutExpo);
            iTween.MoveTo(spellCard.gameObject, hashtable);
        }

        iTween.ShakePosition(transform.parent.gameObject, _shakeAmount, _shakeTime);
        iTween.ShakePosition(gameObject, _shakeAmount, _shakeTime);

        yield return new WaitForSeconds(_shakeTime);

        yield return HideHand(true);

        yield return MoveSpellGroup(true);

        _mage.SetAllDiscoverable(true);

        _mage.ReadyToExecute();
    }

    // анимация скрытия карт руки
    IEnumerator HideHand(bool hide)
    {
        _handIsHidden = hide;
        if (hide)
            _mage.GetSpellsInHand().ForEach(card => card.SelectCard(false));
        float y = (hide) ? _handHiddenY : _handUnhiddenY;
        iTween.MoveTo(_handLocation.gameObject, iTween.Hash("y", y, "time", _handHideTime, "islocal", true));
        yield return new WaitForSeconds(_handHideTime);
        StartCoroutine(FitSpellCardsInHand());
    }

    
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    // вернуть расположение места для карты заклинания определенного порядка
    Transform GetLocationOfOrder(Order order)
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

    // вернуть локальный вектор расположения карты в руке
    Vector3 GetHandPositionVector(float x)
    {
        return new Vector3(x, _handCardsY, _handCardsZ);
    }

}