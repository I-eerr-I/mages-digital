using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{

    [SerializeField] protected HandController _hand;  // рука мага
    [SerializeField] protected int _health   = 20;    // здоровье мага
    [SerializeField] protected bool _isReady = false; // готовность мага к началу раунда


    public Mage mage;  // данные мага
    
    public bool isDead => _health <= 0; // мертв ли маг
    
    public HandController hand => _hand;
    public int health => health;
    public bool isReady => _isReady;

    public MageController leftMage  = null;
    public MageController rightMage = null;

    void Awake()
    {
        _hand = gameObject.GetComponentInChildren<HandController>();
    }

    // добрать нужные карты и стать готовым
    public virtual IEnumerator OnRoundStart()
    {
        if (isDead)
            yield return DrawCards(GameManager.instance.deadsDeck, 1);
        else
            yield return DrawCards(GameManager.instance.spellsDeck, 8 - _hand.spellsCount);
        _isReady = true;
    }

    // добрать карты из колоды
    public virtual IEnumerator DrawCards(DeckController deck, int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            TakeCard(deck);
            yield return new WaitForFixedUpdate();
        }
    }

    public virtual void Unready()
    {
        _isReady = false;
    }

    // взять карту из колоды
    public void TakeCard(DeckController deck)
    {
        Card card = deck.PassCard();
        if (card != null) _hand.AddCard(card);
    }


}
