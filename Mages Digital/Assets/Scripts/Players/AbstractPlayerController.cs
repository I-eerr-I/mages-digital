using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public abstract class AbstractPlayerController : MonoBehaviour
{


    protected MageController _mage; // маг под управлением

    protected Order _chosenOrder = Order.WILDMAGIC; // выбранный порядок (для шальной магии1)

    protected Transform _handLocation;   // объект расположения карт в руке
    protected Transform _spellLocation;  // объект расположения карт в заклинании (в руке)
    protected Transform _bonusLocation;  // объект расположения бонусных карт

    protected float _spellGroupMovingTime  = 0.5f;

    protected bool _isBot;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public Order chosenOrder   => _chosenOrder;
    public MageController mage => _mage;
    public bool  isBot         => _isBot;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        _mage = gameObject.GetComponent<MageController>();

        _handLocation  = transform.GetChild(0);
        _spellLocation = transform.GetChild(1);
        _bonusLocation = transform.GetChild(2);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // реакция на добавление карты к руке
    public abstract IEnumerator OnCardAddedToHand(CardController cardController);

    // реакция на добавление карты к заклинанию
    public abstract IEnumerator OnCardAddedToSpell(CardController addedCard, Order order);

    // передвинуть готовое заклинание к руке или в центр поля
    protected abstract IEnumerator MoveSpellGroup(bool toHand = false);

    protected abstract IEnumerator MoveCard(CardController card, bool toHand = false);


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // реакция на сброс заклинания
    public virtual IEnumerator OnSpellDrop()
    {
        yield break;
    }

    // выбрать порядок расположения для карты шальной магии
    public virtual IEnumerator ChooseOrder()
    {
        yield break;
    }

    // реакция на наведение на карту
    public virtual void OnSpellCardSelected(CardController cardController, bool isSelected)
    {
        
    }

    // реакция на сброс параметров мага
    public virtual void OnMageReset()
    {

    }




////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // вернуть объект, к которому присоеденить карту в руке
    protected void SetCardHandParent(CardController card)
    {
        if (card.isSpell)
            card.transform.SetParent(_handLocation);
        else
            card.transform.SetParent(_bonusLocation);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // показать заклинание все для выполнения
    public IEnumerator ShowSpellToAll()
    {
        yield return MoveSpellGroup(toHand: false);
    }

    // спрятать заклинание после выполнения
    public IEnumerator HideSpellFromAll()
    {
        yield return MoveSpellGroup(toHand: true);
    }

    public IEnumerator ShowCardToAll(CardController card, bool highlight = true)
    {
        yield return MoveCard(card, toHand: false);
        if (highlight)
            yield return card.Highlight(true);
    }

    public IEnumerator HideCardFromAll(CardController card)
    {
        yield return MoveCard(card, toHand: true);
    }

}
