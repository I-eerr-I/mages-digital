using Random = System.Random;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    
    private Random random = new Random();

    [Header("Спрятанное состояние колоды")]
    [SerializeField] private float _hiddenY   = 20.0f;  // координата Y колоды в скрытом состоянии
    [SerializeField] private float _unhiddenY = 0.5f;   // координата Y колоды в видимом состоянии
    [SerializeField] private float _hideTime  = 0.25f;  // время для анимации скрытия колоды
    
    [Header("Размер и вид 3D модели колоды")]
    [SerializeField] private float  _cardThickness = 0.025f; // толщина одной карты

    [Header("Колоды")]
    [SerializeField] private CardType _cardsType = CardType.SPELL;    // тип карт в колоде
    [SerializeField] private List<Card>  _deck   = new List<Card>();  // список карт колоды
    [SerializeField] private List<Card>  _fold   = new List<Card>();  // сброс карт
    
    private bool  _hidden  = true;   // спрятана ли колода

    private MeshRenderer   _baseMeshRenderer;   // рендер основы колоды
    private Light          _baseLight;          // свет от основы колоды
    private SpriteRenderer _backSpriteRenderer; // рендер задней (верхней) части колоды (рубашка)

    private Sprite _back; // рубашка карт колоды



    public float    hiddenY        => _hiddenY;   
    public float    unhiddenY      => _unhiddenY;
    public float    hideTime       => _hideTime;
    public CardType cardsType      => _cardsType;         

    public int      cardsAmount    => _deck.Count;               // количество карт в колоде
    public int      allCardsAmount => _deck.Count + _fold.Count; // количество карт в колоде и сбросе

    void Awake()
    {
        _backSpriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        _baseLight          = gameObject.GetComponentInChildren<Light>();
        _baseMeshRenderer   = gameObject.GetComponentInChildren<MeshRenderer>();

        _back = _backSpriteRenderer.sprite;
        _baseLight.color = CardController.CARD_MAIN_COLOR[_cardsType];

        if (_cardsType == CardType.SPELL) DoubleDeck();
        UpdateDeckSize();
    }

    void Start()
    {
        Shuffle();
    }

    // TEST
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Hide(!_hidden);
        }
    }
    // TEST

    // выдать N карт магу из колоды
    public IEnumerator PassCardsTo(MageController owner, int nCards)
    {
        // показать колоду
        yield return Hide(false);

        for (int i = 0; i < nCards; i++)
        {
            // взять последнюю верхнюю карту из колоды
            Card card = TakeLastCard();
            
            // если карта есть
            if (card != null)
            {
                // создать карту
                CardController cardController = SpawnCard(card);
                
                // добавить карту владельцу
                StartCoroutine(owner.AddCard(cardController));
                
                yield return new WaitForSeconds(0.25f);
            }
        }

        // скрыть колоду
        yield return Hide(true);
    }

    // заменить шальную магию в заклинании
    public IEnumerator ReplaceWildMagic(CardController wildMagic)
    {
        // показать колоду со свдигом вверх
        yield return Hide(false, GameManager.instance.spellGroupLocation.position.y);

        Order order                = wildMagic.spellOrder;
        MageController owner       = wildMagic.owner;
        CardController replaceCard = null;

        List<CardController> cardsToFold = new List<CardController>();
        
        float addY = 0.25f;
        while (replaceCard == null && allCardsAmount > 0)
        {
            // спаун карты
            Card card = TakeLastCard();
            CardController cardController = SpawnCard(card);
            StartCoroutine(cardController.PositionFrontUp());
            StartCoroutine(cardController.Highlight(true));

            // сдвиг карты в сторону
            Vector3 position = cardController.transform.position;
            iTween.MoveTo(cardController.gameObject, iTween.Hash("x", position.x + cardController.cardSizeX, "y", position.y + addY, "time", 0.05f));
            addY += 0.2f;
            yield return new WaitForSeconds(0.15f);
            
            
            SpellCard spellCard = (SpellCard) card;
            if (spellCard.order == order)           // если карта на замену найдена
                replaceCard = cardController;       // сохранить ее
            else                                    // иначе
                cardsToFold.Add(cardController);    // добавить в список карт для сброса
        }

        yield return Hide(true);    // спрятать колоду
        
        // отправить ненужные карты в сброс
        foreach (CardController cardToFold in cardsToFold)
        {
            StartCoroutine(AddCardToFold(cardToFold));
            yield return new WaitForSeconds(0.15f);
        }

        if (replaceCard != null) // если нужная карта была найдена
        {
            replaceCard.SetOwner(owner);   // установить владельца карты
            replaceCard.Highlight(false);  // убрать выделение карты

            wildMagic.RemoveOwner();       // убрать владельца у предыдущей карты
            
            StartCoroutine(AddCardToFold(wildMagic, destroy: false));  // отправить предыдущую карту в сброс

            yield return owner.AddToSpell(replaceCard, order, backToHand: false, ownerReaction: false); // добавить новую карту в заклинание
            yield return owner.owner.ShowSpellToAll();  // выровнять карты заклинаний
        }
        yield break;
    }

    // добавить карту в сброс
    public IEnumerator AddCardToFold(CardController cardToFold, bool destroy = true)
    {
        _fold.Add(cardToFold.card);
        yield return cardToFold.FlyOutAndDestroy(destroy: destroy);
    }

    // показать\спрятать колоду
    public IEnumerator Hide(bool hide, float addY = 0.0f)
    {
        if (hide != _hidden)
        {
            float y = (hide) ? _hiddenY : _unhiddenY;
            iTween.MoveTo(gameObject, iTween.Hash("y", y + addY, "time", _hideTime, "easetype", iTween.EaseType.easeOutSine));
            _hidden = hide;
            yield return new WaitForSeconds(_hideTime);
        }
        yield break;
    }

    // вернуть последнюю верхнюю карту из колоды и удалить ее из колоды
    // если в колоде карт нет, то перемешать колоду со сбросом
    public Card TakeLastCard()
    {
        Card card = null;
        if (_deck.Count > 0)
        {
            card = _deck[0];
            _deck.RemoveAt(0);
            UpdateDeckSize();
        }
        else if (_fold.Count > 0)
        {
            ShuffleWithFold();
            return TakeLastCard();
        }
        return card;
    }

    // спаун карты на поле на месте колоды
    public CardController SpawnCard(Card card)
    {
        // создание и настройка объекта карты
        GameObject cardObject         = Instantiate(GameManager.instance.cardPrefab, GameManager.instance.fieldCenter);
        cardObject.name               = card.cardName;
        cardObject.transform.position = transform.position;
        
        // настройка контроллера карты
        CardController cardController = cardObject.GetComponent<CardController>();
        cardController.SetupCard(card, _back);
        
        return cardController;
    }

    // перемешать колоду
    public void Shuffle()
    {
        _deck = _deck.OrderBy(a => random.Next()).ToList();
    }

    // замешать сброс
    public void ShuffleWithFold()
    {
        _deck.AddRange(_fold);
        _fold.Clear();
        Shuffle();
        UpdateDeckSize();
    }

    // удвоить карты в колоде
    void DoubleDeck()
    {
        List<Card> deckToAdd = new List<Card>(_deck);
        if (_cardsType == CardType.SPELL)
            deckToAdd = _deck.FindAll((card) => ((SpellCard)card).order != Order.WILDMAGIC);
        _deck.AddRange(deckToAdd);
        UpdateDeckSize();
    }

    // если количество карт в колоде было изменено, то и изменится размер самой колоды
    void UpdateDeckSize()
    {
        bool isDeckEmpty = _deck.Count == 0;
        _baseLight.enabled = _baseMeshRenderer.enabled = _backSpriteRenderer.enabled = !isDeckEmpty;
        if (!isDeckEmpty)
        {
            float deckSize = _cardThickness * _deck.Count;
            iTween.ScaleTo(gameObject, new Vector3(transform.localScale.x, transform.localScale.y, deckSize), 0.01f);   
        }
    }

}
