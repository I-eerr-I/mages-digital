using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{

    [SerializeField] private HandController _hand;  // рука мага
    [SerializeField] private int _health = 20;      // здоровье мага


    public Mage mage;  // данные мага
    
    public bool isPlayer => gameObject.tag == "Player"; // является ли маг игроком
    public bool isDead => _health <= 0;                 // мертв ли маг
    
    public HandController hand => _hand;

    // TEST
    public MageController leftMage;
    public MageController rightMage;
    // TEST


    void Start()
    {
        // TEST
        transform.position = new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0.0f);
        if (GameManager.instance.playerMage == mage) gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        _health++;
        _health--;
        // TEST
    }

    // взять карту из колоды
    public void TakeCard(DeckController deck)
    {
        Card card = deck.PassCard();
        if (card != null) _hand.AddCard(card);
    }

}
