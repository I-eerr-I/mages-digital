using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public static Dictionary<Order, Color> SPELL_CARD_MAIN_COLOR = new Dictionary<Order, Color>()
    {
        { Order.SOURCE,      new Color(1.0f, 0.8427867f, 0.0f) },
        { Order.QUALITY,     new Color(0.9960785f, 0.4745098f, 0.0f) },
        { Order.DELIVERY,    new Color(0.9960785f, 0.0509804f, 0.1529412f) }
    };

    public Card card;
    public MageController owner;

    private SpriteRenderer _frontSpriteRenderer;
    private SpriteRenderer _backSpriteRenderer;
    private Outline _outline;
    private Light _outlineLight;
    

    void Awake()
    {
        _frontSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer  = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        GameObject outlineObject = transform.GetChild(2).gameObject;
        _outline = outlineObject.GetComponent<Outline>();
        _outlineLight = outlineObject.GetComponent<Light>();

        // TEST
        _frontSpriteRenderer.material.SetFloat("_OutlineEnabled", 0.0f);
        // TEST
    }

    public void SetupCard(Card card, Sprite back)
    {
        this.card = card;
        _frontSpriteRenderer.sprite = card.front;
        _backSpriteRenderer.sprite  = back;
        Color cardColor;
        if (card.cardType == CardType.SPELL)
            cardColor = SPELL_CARD_MAIN_COLOR[((SpellCard) card).order];
        else
            cardColor = DeckController.DECK_MAIN_COLOR[card.cardType];
        _outline.OutlineColor = cardColor;
        _outlineLight.color   = cardColor;
    }

    // TEST
    void OnMouseOver()
    {
        _frontSpriteRenderer.material.SetFloat("_OutlineEnabled", 1.0f);
    }

    void OnMouseExit()
    {
        _frontSpriteRenderer.material.SetFloat("_OutlineEnabled", 0.0f);
    }

    void OnMouseDown()
    {
        print("Clicked!");
    }
    // TEST

    public void SetBack(Sprite back)
    {
        _backSpriteRenderer.sprite = back;
    }


}
