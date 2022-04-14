using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{
    public Mage mage;

    public bool isPlayer
    {
        get => gameObject.tag == "Player";
    }

    [SerializeField] private HandController _hand;
    public HandController hand => _hand;

    [SerializeField] private int _health = 20;

    public bool isDead => _health <= 0;

    void Start()
    {
        // TEST
        transform.position = new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0.0f);
        if (GameManager.instance.playerMage == mage) gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        _health++;
        _health--;
        // TEST
    }

    public void TakeCard(DeckController deck)
    {
        Card card = deck.PassCard();
        if (card != null) _hand.AddCard(card);
    }

}
