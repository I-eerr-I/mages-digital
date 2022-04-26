using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{

    [SerializeField] protected HandController _hand;  // рука мага
    [SerializeField] protected int _health = 20;      // здоровье мага
    [SerializeField] protected TextMesh _healthText;  // текст здоровья мага
    [SerializeField] protected bool _isReady = false; // готовность мага к началу раунда


    public Mage mage;  // данные мага
    
    public bool isDead => _health <= 0; // мертв ли маг
    
    public HandController hand => _hand;
    public int health
    {
        get => _health;
        set 
        {
            _health = value;
            OnHealthChange();
        }
    }
    public bool isReady => _isReady;

    public MageController leftMage  = null;
    public MageController rightMage = null;

    void Awake()
    {
        _hand       = gameObject.GetComponentInChildren<HandController>(); 
    }

    void Start()
    {
        OnHealthChange();
    }

    // реакция на изменение жизней мага
    public virtual void OnHealthChange()
    {
        _healthText.text = health.ToString();
    }

    // добрать нужные карты и стать готовым
    public virtual IEnumerator OnRoundStart()
    {
        if (isDead)
            yield return DrawCards(GameManager.instance.deadsDeck, 1);
        else
            yield return DrawCards(GameManager.instance.spellsDeck, _hand.handSize - _hand.spellsCount);
        _isReady = true;
    }

    // добрать карты из колоды
    public virtual IEnumerator DrawCards(DeckController deck, int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            if (TakeCard(deck))
            {
                // RETURN
                // iTween.MoveTo(deck.cardToPass.gameObject, transform.position, 0.5f);
                // yield return new WaitForSeconds(0.6f);
                // deck.HideCardToPass();
                // RETURN
                continue; // TEST
            }
            else
            {
                break;
            }
        }
        yield break;
    }

    public virtual void Unready()
    {
        _isReady = false;
    }

    // взять карту из колоды, вернуть true если карта добавлена
    public virtual bool TakeCard(DeckController deck)
    {
        Card card = deck.PassCard();
        if (card != null) _hand.AddCard(card);
        return card != null;
    }



}
