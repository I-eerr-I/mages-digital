using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine.UI;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public static Dictionary<Order, Color> SPELL_CARD_MAIN_COLOR = new Dictionary<Order, Color>()
    {
        { Order.SOURCE,      new Color(1.0f, 0.8427867f, 0.0f) },
        { Order.QUALITY,     new Color(0.9960785f, 0.4745098f, 0.0f) },
        { Order.DELIVERY,    new Color(0.9960785f, 0.0509804f, 0.1529412f) },
        { Order.WILDMAGIC,   new Color(0.0f, 0.60784f, 0.5843f) }
    };

    [SerializeField] private Card _card;
    [SerializeField] private MageController _owner;

    private SpriteRenderer _frontSpriteRenderer;
    private SpriteRenderer _backSpriteRenderer;

    private OutlineController _outlineController;
    

    public bool inHand  = false;
    public bool inSpell = false;

    public Card card => _card;
    public MageController owner => _owner;
    public bool withOwner => _owner != null;
    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;
    public SpriteRenderer backSpriteRenderer  => _backSpriteRenderer;

    void Awake()
    {
        _frontSpriteRenderer     = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer      = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        GameObject outlineObject = transform.GetChild(2).gameObject;
        _outlineController       = outlineObject.GetComponent<OutlineController>();
    }

    public bool IsSpell()
    {
        if (_card)
            return _card.cardType == CardType.SPELL;
        return false;
    }

    public void SetupCard(Card card, Sprite back = null)
    {
        _card = card;
        _frontSpriteRenderer.sprite = card.front;
        if (back != null)
            _backSpriteRenderer.sprite  = back;
        Color cardColor;
        if (card.cardType == CardType.SPELL)
            cardColor = SPELL_CARD_MAIN_COLOR[((SpellCard) card).order];
        else
            cardColor = DeckController.DECK_MAIN_COLOR[card.cardType];
        _outlineController.SetColor(cardColor);
    }

    public void SetOwner(MageController owner)
    {
        _owner = owner;
    } 

    public void OnMouseOverTrigger()
    {
        if (withOwner)
        {
            UIManager.instance.ShowCardInfo(card.front, true);
            if (IsSpell())
                owner.owner.OnSpellCardSelected(this, true);
        }

    }

    public void OnMouseExitTrigger()
    {
        UIManager.instance.ShowCardInfo(card.front, false);
        if (IsSpell() && withOwner)
            owner.owner.OnSpellCardSelected(this, false);
    }

    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    public void OnMouseDownTrigger()
    {
        if (IsSpell() && withOwner)
        {
            SpellCard spellCard = (SpellCard) card;
            Order order = spellCard.order;
            if (inHand)
            {
                if (order == Order.WILDMAGIC)
                    return; // XXX
                StartCoroutine(owner.AddToSpell(this, order));
                StateToSpell();
            }
            else if (inSpell)
            {
                StartCoroutine(owner.BackToHand(this, order));
                StateToHand();
            }
        }
    }

    public IEnumerator RotateFrontUp()
    {
        Vector3 rotation;
        Vector3 current = transform.eulerAngles;
        if (!inHand)
            rotation = new Vector3(current.x, 0.0f, current.z);
        else
            rotation = new Vector3(90.0f, current.y, current.z);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", 0.15f, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }

    public IEnumerator RotateBackUp()
    {
        Vector3 rotation;
        Vector3 current = transform.eulerAngles;
        if (!inHand)
            rotation = new Vector3(current.x, 180.0f, current.z);
        else
            rotation = new Vector3(-90.0f, current.y, current.z);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", 0.15f, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

    public void StateToSpell()
    {
        inHand  = false;
        inSpell = true;
    }

    public void StateToHand()
    {
        inHand  = true;
        inSpell = false;
    }

}
