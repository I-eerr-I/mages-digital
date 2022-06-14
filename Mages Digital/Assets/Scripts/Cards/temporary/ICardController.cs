using Random = System.Random;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public abstract class ICardController : MonoBehaviour
{
    public Random random = new Random();

    public const string POSITION_LEFT  = "left";  // Позиция слева
    public const string POSITION_RIGHT = "right"; // Позиция справа

    // словарь для получения цвета подсветки карты в зависимости от ее типа
    public static Dictionary<CardType, Color> CARD_MAIN_COLOR = new Dictionary<CardType, Color>()
    {
        { CardType.SPELL,    new Color(1.0f, 0.0f, 0.0f) },
        { CardType.TREASURE, new Color(1.0f, 0.8427867f, 0.0f) },
        { CardType.DEAD,     new Color(0.0f, 0.6603774f, 0.4931389f) }
    };

    [SerializeField] protected Card _card;                 // данные карты

    [SerializeField] protected MageController _owner;      // маг владеющий картой
    [SerializeField] protected DeckController _sourceDeck; // колода, откуда была создана карта
    [SerializeField] protected bool _visible = true;       // видимость карты


    protected float _cardFlippingTime          = 0.15f; // время переворота карты
    protected float _cardShowInfoWaitTime      = 1.5f;  // сколько держать курсор на карте, чтобы показать ее информацию
    protected float _cardHighlightDeltaY       = 1.0f;  // насколько поднять карту вверх для выделения
    protected float _cardHighlightTime         = 1.0f;  // время поднятия карты для выделения
    protected float _cardHighlightLightDelta   = 2.0f;  // увеличение дальности света (яркости) при выделении
    protected float _cardFlyOutY               = 10.0f; // глобальная координата Y при вылете карты за экран
    protected float _cardFlyOutXMin            = -3.0f; // минимальное значение X при вылете карты за экран
    protected float _cardFlyOutXMax            = 3.0f;  // максимальное значение X при вылете карты за экран
    protected float _cardFlyOutTime            = 1.0f;  // время вылета карты за экран

    protected bool _discoverable = true;               // можно ли взаимодействовать с картой
    protected bool _isMouseOver = false;              // находится ли курсор на карте
    protected float _mouseOverTime = 0.0f;               // время, прошедшее с момента наведения на карту

    protected CardState cardState = CardState.NO_OWNER; // состояние карты

    protected GameObject        _middle;              // середина карты
    protected Light             _middleLight;         // источник света карты
    protected MeshRenderer      _middleMeshRenderer;  // рендер середины карты
    protected BoxCollider       _middleBoxCollider;   // коллайдер середины карты
    protected SpriteRenderer    _frontSpriteRenderer; // рендер передней части карты
    protected SpriteRenderer    _backSpriteRenderer;  // рендер задней части (рубашки) карты
    protected OutlineController _outlineController;   // управление подсветкой карты

    public Card           card       => _card;
    public MageController owner      => _owner;

    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;

    public GameManager gm => GameManager.instance;

    public CardEffectsManager effects => CardEffectsManager.instance;

    public CardType cardType     => _card.cardType;
    public bool  inHand          => cardState == CardState.IN_HAND;     // находится ли карта в руке
    public bool  inSpell         => cardState == CardState.IN_SPELL;    // находится ли карта в заклинании
    public bool  withOwner       => cardState != CardState.NO_OWNER;    // есть ли у карты владелец
    public bool  withSourceDeck  => _sourceDeck != null;                // создана ли карта колодой
    public float cardSizeX       => _middle.transform.localScale.x;     // ширина карты


    protected void Awake()
    {
        _frontSpriteRenderer     = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer      = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        
        _middle = transform.GetChild(2).gameObject;
        _middleLight        = _middle.GetComponent<Light>();
        _outlineController  = _middle.GetComponent<OutlineController>();
        _middleMeshRenderer = _middle.GetComponent<MeshRenderer>();
        _middleBoxCollider  = _middle.GetComponent<BoxCollider>();
    }

    protected virtual void Start()
    {
        SetVisible(_visible);
    }

    // настроить карту
    public abstract void SetupCard(Card card, DeckController deck = null, Sprite back = null);

    // отправить карту в сброс
    public void ToFold(bool destroy = true)
    {
        if (withOwner)
        {
            // owner.RemoveCard(this); UNCOMMENT
            RemoveOwner();
        }
        if (_sourceDeck != null) 
            // _sourceDeck.AddCardToFold(this); UNCOMMENT
        StartCoroutine(FlyOutAndDestroy(destroy: destroy));
    }


    // вылет карты за экран и уничтожение
    protected IEnumerator FlyOutAndDestroy(bool destroy = true)
    {
        float x = (float) random.NextDouble() * (_cardFlyOutXMax - _cardFlyOutXMin) + _cardFlyOutXMin;
        
        Hashtable hashtable = new Hashtable();
        hashtable.Add("y", _cardFlyOutY);
        hashtable.Add("x", x);
        hashtable.Add("time", _cardFlyOutTime);
        if (destroy)
            hashtable.Add("oncomplete", "DestoyObject");
        
        iTween.MoveTo(gameObject, hashtable);
        
        yield break;
    }

    // перевернуть карту лицевой строной вверх
    public IEnumerator PositionFrontUp()
    {
        Vector3 rotation;
        if (inHand)
            rotation = new Vector3(0.0f, 0.0f, 0.0f);
        else
            rotation = new Vector3(90.0f, 0.0f, 0.0f);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", _cardFlippingTime, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }

    // перевернуть карту рубашкой вверх
    public IEnumerator PositionBackUp()
    {
        Vector3 rotation;
        if (inHand)
            rotation = new Vector3(0.0f, 180.0f, 0.0f);
        else
            rotation = new Vector3(-90.0f, 0.0f, 0.0f);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", rotation, "time", _cardFlippingTime, "islocal", inHand));
        yield return new WaitForSeconds(0.15f);
    }

    // выделение карты (обычно в заклинании)
    public IEnumerator Highlight(bool highlight)
    {
        if (_outlineController != null)
            _outlineController.enabled = !highlight;

        _middleLight.enabled  = highlight;
        _middleLight.range   += (highlight ? 1.0f : -1.0f) * _cardHighlightLightDelta;
        
        float y = transform.position.y;
        y += (highlight ? 1.0f : -1.0f) * _cardHighlightDeltaY;
        
        iTween.MoveTo(gameObject, iTween.Hash("y", y, "time", _cardHighlightTime));
        
        yield return new WaitForSeconds(_cardHighlightTime);
    }

    // выполнение заклинания карты
    public IEnumerator ExecuteSpell()
    {
        if (card.spell != "")
            yield return StartCoroutine(card.spell);
    }

    // триггер при наведении курсора на карту
    public void OnMouseOverTrigger()
    {
        if (_discoverable)
        {
            _isMouseOver = true;
            if (withOwner)
            {
                OnMouseOverAction();
            }
        }
    }

    // триггер при выходе курсора из области карты
    public void OnMouseExitTrigger()
    {
        _isMouseOver = false;
        OnMouseExitAction();
    }

     // триггер при клике на карту
    public void OnMouseDownTrigger()
    {
        if (_discoverable)
        {
            OnMouseDownAction();
        }
    }

    public abstract void OnMouseOverAction();

    public abstract void OnMouseExitAction();

    public abstract void OnMouseDownAction();

    // показать информацию о карте
    protected abstract void ShowCardInfo();

    protected abstract void HideCardInfo();

    // выделить карту
    protected void SelectCard(bool select)
    {
        // owner.owner.OnSpellCardSelected(this, select); UNCOMMENT
    }

    // установить видимость карты
    public void SetVisible(bool visible)
    {
        _visible = visible;
        _frontSpriteRenderer.enabled = visible;
        _backSpriteRenderer.enabled  = visible;
        _middleMeshRenderer.enabled  = visible;
        _middleBoxCollider.enabled   = visible;
        _outlineController.enabled   = visible;
    }

    // установить карту доступной для нажатия и наведения мышью
    public void SetDiscoverable()
    {
        _discoverable = true;
        _outlineController.enabled = true;
    }

    // установить карту недоступной для нажатия и наведения мышью
    public void SetUndiscoverable()
    {
        _discoverable = false;
        _outlineController.enabled = false;
    }

    // установить владельца карты
    public void SetOwner(MageController owner)
    {
        _owner = owner;
    }

    // удалить владельца карты
    public void RemoveOwner()
    {
        _owner = null;
        SetStateToNoOwner();
    }

    public void SetStateToNoOwner()
    {
        cardState = CardState.NO_OWNER;
        _outlineController.SetProperties(true, false);
    }


    public void SetStateToInSpell()
    {
        cardState = CardState.IN_SPELL;
        _outlineController.SetProperties(true, false);
    }


    public void SetStateToInHand()
    {
        cardState = CardState.IN_HAND;
        _outlineController.SetProperties(false, true);
    }

    public void DestoyObject()
    {
        Destroy(gameObject);
    }







    public enum TargetType
    {
        RANDOM,
        LOW_HP,
        HIGH_HP
    }


    public IEnumerator OnDiceRoll(List<int> rolls, bool showBonus = true)
    {
        if (owner.bonusDice > 0 && showBonus)
            owner.mageIcon.ShowInfoText($"+{owner.bonusDice}");

        yield return effects.RollDice(rolls);
        if (showBonus)
            owner.mageIcon.HideInfoText();
    }

    public IEnumerator OnRandomEnemy(MageController enemy)
    {
        yield return HighlightEnemies(5);

        enemy.mageIcon.Highlight(true);
        yield return new WaitForSeconds(0.5f);
        enemy.mageIcon.Highlight(false);
    }

    public IEnumerator HighlightEnemies(int nTimes)
    {
        for (int i = 0; i < nTimes; i++)
        {
            List<MageController> mages = gm.aliveMages.OrderBy(mage => random.Next()).ToList();
            foreach (MageController mage in mages)
            {
                if (mage == owner)
                    continue;
                mage.mageIcon.Highlight(true);
                yield return new WaitForSeconds(0.1f);
                mage.mageIcon.Highlight(false);
            }   
        }
    }

    public List<int> RollDice(int numberDice)
    {
        List<int> rolls = new List<int>();
        for (int i = 0; i < numberDice;  i++)
        {
            int result = random.Next(1, 7); // Бросок кубика
            rolls.Add(result);
        }

        return rolls;
    }

    public int NumberDice(Sign sign)
    {
        int numberDice = 0; // Кол-во кубиков

        // Цикл нахождения кол-во одинаковых знаков карты заклинания => кол-во кубиков 
        List<CardController> currentSpell = owner.nonNullSpell.ToList();
        numberDice = currentSpell.Count(spellCard => spellCard.GetSpellCard().sign == sign);

        numberDice += owner.bonusDice;
        
        return numberDice;
    }

    // Урон магу (Урон, Цель)
    public IEnumerator DamageToTarget(int damage, MageController target)
    {
        yield break;
        // target.TakeDamage(damage, this); UNCOMMENT
        // yield return effects.Attack(transform.position, target.mageIcon.transform.position, this); UNCOMMENT
    } 

    // Лечение мага (Жизни, Цель)
    public IEnumerator HealToTarget(int healHp, MageController target)
    {
        target.Heal(healHp);
        yield break;
    }

    // Урон нескольким магам (Урон, Лист Целей)
    public IEnumerator DamageToTargets(int damage, List<MageController> listTargets)
    {
        foreach(MageController target in  listTargets)
            yield return DamageToTarget(damage, target);

        yield break;
    }

    // Метод возвращает цель, если цель мертва берет следующую цель или слева или справа
    public MageController IsDeadTargetLeftOrRight(MageController target, string position)
    {
        while (target.isDead)// Пока цель мертва
        {
            if(position == POSITION_LEFT)
                target = target.leftMage; // Берем следующего левого мага
            else if(position == POSITION_RIGHT)
                target = target.rightMage; // Берем следующего левого мага
        }
        return target;
    }

    // Урон магу и его соседям слева и справа
    public IEnumerator DamageToTargetsNeighbors(int damage, int damageNeighbors , List<MageController> listTargets)
    {
        foreach(MageController target in  listTargets)
        {
            yield return DamageToTarget(damage, target);
            
            yield return DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));
            yield return DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));
        }
    } 

    public List<MageController> FindTargets(TargetType targetType)
    {
        switch (targetType)
        {
            case TargetType.HIGH_HP:
                return HighHpTargets();
            case TargetType.LOW_HP:
                return LowHpTargets();
            case TargetType.RANDOM:
                return RandomEnemy();
        }
        return null;
    }

    // Метод возвращающий случайного врага(принимает владельца)
    public List<MageController> RandomEnemy()
    {
        // Создаем список без владельца карты
        List<MageController> magesWithoutOwner = AllEnemies();
        // Рандомим индекс
        int index = random.Next(magesWithoutOwner.Count);
        // Возвращаем мага из списка
        List<MageController> targets = new List<MageController>() { magesWithoutOwner[index] };
        return targets;
    }

    // Метод возвращает лист целей, Нахождение хилого мага из списка живых
    public List<MageController> LowHpTargets()
    {
        List<MageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        List<MageController> listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    // Метод возвращает лист целей, Нахождение живучего мага из списка живых
    public List<MageController> HighHpTargets()
    {
        
        List<MageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        List<MageController> listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    //Метод возвращает лист целей, нахождение каждого врага
    public List<MageController> AllEnemies()
    {
        List<MageController> magesWithOutOwner = gm.aliveMages.FindAll(mage => owner !=mage);

        return magesWithOutOwner;
    }

    // Увеличение урона от кол-ва одинаковых знаков (некоторые карты)
    public int BuffDamageSign(Sign sign)
    {
        int buffDamage = 0; // Стартовое увеличение урона
        // Цикл нахождения кол-ва одинаковых знаков карты заклинания => Увеличение урона
        foreach (CardController spellCard in owner.nonNullSpell)
        {
            if(spellCard.GetSpellCard().sign == sign)
                buffDamage+= 1;
        }
        return buffDamage;
    }




}
