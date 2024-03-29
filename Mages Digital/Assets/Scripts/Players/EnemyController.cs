using Convert = System.Convert;
using Random  = System.Random;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class EnemyController : AbstractPlayerController
{

    public Random random = new Random();


    public enum EnemyState
    {
        IDLE,
        CREATING_SPELL,
        SPELL_IS_READY,
        EXECUTING_SPELL,
    }

    
    EnemyState _state = EnemyState.IDLE;

    float _cardMovingToHandTime   = 0.25f;
    float _cardMovingToHandDeltaY = 0.5f; 


    public EnemyState  state => _state;
    public GameManager    gm => GameManager.instance;

    
    void Update()
    {
        if (gm.isSpellCreationState)
        {
            if (_state != EnemyState.CREATING_SPELL && _state != EnemyState.SPELL_IS_READY)
            {
                StartCoroutine(OnSpellCreationState());
            }
        }
        else if (gm.isSpellExecutionState)
        {
            if (_state != EnemyState.IDLE && _state != EnemyState.EXECUTING_SPELL)
            {
                StartCoroutine(OnSpellExecutionState());
            }
        }
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


    public override IEnumerator OnCardAddedToSpell(CardController addedCard, Order order)
    {
        addedCard.SetStateToInSpell();
        addedCard.transform.SetParent(_spellLocation);
        yield break;
    }


    protected override IEnumerator MoveSpellGroup(bool toHand)
    {
        if (gm.isSpellExecutionState)
        {
            if (toHand)
                _state = EnemyState.IDLE;
            else
                _state = EnemyState.EXECUTING_SPELL;
        }


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
            
            if (!toHand) 
                StartCoroutine(cardInSpell.PositionFrontUp());
            else
                StartCoroutine(cardInSpell.PositionBackUp());

            iTween.MoveTo(cardInSpell.gameObject, iTween.Hash("position", position, "time", _spellGroupMovingTime));
            x += step;
        }

        yield return new WaitForSeconds(_spellGroupMovingTime);

        if (toHand)
            _mage.nonNullSpell.ForEach(card => card.SetVisible(false));
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
            card.SetVisible(true);
            StartCoroutine(card.PositionFrontUp());
            Vector3 position = GameManager.instance.spellGroupLocation.position;
            position.y += 1.0f;
            iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", _spellGroupMovingTime));
            yield return new WaitForSeconds(_spellGroupMovingTime);
        }
    }

    public override IEnumerator OnSpellDrop()
    {
        yield break;
    }

    public override IEnumerator ChooseOrder()
    {
        yield break;
    }

    public override IEnumerator ChooseTreasure(bool hasChoiceNotToDrop)
    {
        bool drop = (hasChoiceNotToDrop) ? Convert.ToBoolean(random.Next(2)) : true;
        int treasuresAmount = mage.treasures.Count;
        if (drop && treasuresAmount > 0)
        {
            int index = random.Next(treasuresAmount);
            CardController treasure = mage.treasures[index];
            treasure.SetVisible(true);
            mage.chosenCard = treasure;
        }
        yield break;
    }

    public override IEnumerator ChooseTreasureFromMage(MageController mage, string actionText)
    {
        List<CardController> treasures = mage.treasures;
        int treasuresAmount = treasures.Count;
        if (treasuresAmount > 0)
        {
            int index = random.Next(treasuresAmount);
            CardController treasure = treasures[index];
            _mage.chosenCard = treasure;
        }
        yield break;
    }

    public override IEnumerator ChooseCardFromSpell(List<CardController> spellCards)
    {
        int index = random.Next(spellCards.Count);
        _mage.chosenCard = spellCards[index];
        yield break;
    }

    public override IEnumerator ChooseCardFromHand()
    {
        int index = random.Next(_mage.nSpellsInHand);
        _mage.chosenCard = _mage.GetSpellsInHand()[index];
        yield break;
    }

    public override IEnumerator ChooseTarget(List<MageController> mages)
    {
        yield return CardEffectsManager.instance.HighlightEnemiesOfMage(_mage, 5);

        int index = random.Next(mages.Count);
        MageController enemy = mages[index];
        yield return enemy.mageIcon.HighlightForSomeTime(1.0f);

        _mage.chosenMage = enemy;
    }


    public override void OnSpellCardSelected(CardController cardController, bool isSelected)
    {

    }

    public override void OnMageReset()
    {

    }


    
    public IEnumerator OnSpellCreationState()
    {
        _state = EnemyState.CREATING_SPELL;

        List<CardController> randomSpell = new List<CardController>();
        List<Order> spellOrders = new List<Order>();

        if (_mage.nSpellsInHand > 0)
        {
            // выбрать случаное количество карт в заклинании от 1 до 3
            int nRandomCardsInSpell = random.Next(1, Mathf.Min(3, _mage.nSpellsInHand));

            // взять по случайной карте каждого типа, не считая дикую магию
            foreach (Order order in new List<Order> { Order.SOURCE, Order.QUALITY, Order.DELIVERY })
            {
                List<CardController> hand = _mage.GetSpellHandOfOrder(order);
                
                if (hand.Count > 0)
                {
                    int randomCardIndex = random.Next(hand.Count);
                    CardController randomSpellCard = hand[randomCardIndex];
                    randomSpell.Add(randomSpellCard);
                    spellOrders.Add(randomSpellCard.GetSpellCard().order);
                }
            }

            // удалить лишнии карты из заклинания
            while (randomSpell.Count > nRandomCardsInSpell)
            {
                int randomCardIndexToRemove = random.Next(randomSpell.Count);
                randomSpell.RemoveAt(randomCardIndexToRemove);
                spellOrders.RemoveAt(randomCardIndexToRemove);
            }

            //   добавить карту в заклинание и заменить случайные на дикую магию
            for (int i = 0; i < randomSpell.Count; i++)
            {
                // если в шальной магии есть карты и карта будет заменена на шальную
                if (_mage.wildMagics.Count > 0 && Convert.ToBoolean( random.Next(2) ))
                {
                    // взять шальную магию из руки
                    randomSpell[i] = _mage.wildMagics[0];
                }

                yield return _mage.AddToSpell(randomSpell[i], spellOrders[i]);
            }
        }

        _mage.ReadyToExecute();
        _state = EnemyState.SPELL_IS_READY;
    }


    IEnumerator OnSpellExecutionState()
    {
        _state = EnemyState.IDLE;
        yield break;
    }


    Vector3 GetHandPosition()
    {
        Vector3 position = _mage.mageIcon.transform.position;
        position.y      -= _cardMovingToHandDeltaY;
        return position;
    }

}
