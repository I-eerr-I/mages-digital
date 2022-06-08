using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class TestCardController : MonoBehaviour
{
    // словарь для получения цвета подсветки карты в зависимости от порядка в заклинании
    public static Dictionary<Order, Color> SPELL_CARD_MAIN_COLOR = new Dictionary<Order, Color>()
    {
        { Order.SOURCE,      new Color(1.0f, 0.8427867f, 0.0f) },
        { Order.QUALITY,     new Color(0.9960785f, 0.4745098f, 0.0f) },
        { Order.DELIVERY,    new Color(0.9960785f, 0.0509804f, 0.1529412f) },
        { Order.WILDMAGIC,   new Color(0.0f, 0.60784f, 0.5843f) }
    };

    // словарь для получения цвета подсветки карты в зависимости от ее типа
    public static Dictionary<CardType, Color> CARD_MAIN_COLOR = new Dictionary<CardType, Color>()
    {
        { CardType.SPELL,    new Color(1.0f, 0.0f, 0.0f) },
        { CardType.TREASURE, new Color(1.0f, 0.8427867f, 0.0f) },
        { CardType.DEAD,     new Color(0.0f, 0.6603774f, 0.4931389f) }
    };

    [SerializeField] private Card _card;            // данные карты
    [SerializeField] private TestMageController _owner; // маг владеющий картой

    [Header("Анимация карты")]
    [SerializeField] private float _cardFlippingTime     = 0.15f;
    [SerializeField] private float _cardShowInfoWaitTime = 1.5f;

    private float _mouseOverTime = 0.0f; // время, прошедшее с момента наведения на карту

    private SpriteRenderer _frontSpriteRenderer;    // рендер передней части карты
    private SpriteRenderer _backSpriteRenderer;     // рендер задней части (рубашки) карты

    private OutlineController _outlineController;   // управление подсветкой карты

    private Order _spellOrder = Order.WILDMAGIC; // порядок в заклинании (нужен для шальной магии)



    public bool  discoverable  = true; // можно ли взаимодействовать с картой

    public CardState cardState = CardState.NO_OWNER; // состояние карты

    public bool inHand    => cardState == CardState.IN_HAND;  // находится ли карта в руке
    public bool inSpell   => cardState == CardState.IN_SPELL; // находится ли карта в заклинании
    public bool withOwner => cardState != CardState.NO_OWNER; // есть ли у карты владелец
    public bool isSpell   => (_card != null) && (_card.cardType == CardType.SPELL); // является карта картой заклинания
    
    public Order spellOrder
    {
        get => _spellOrder;
        set => _spellOrder = value;
    }

    public Card card => _card;
    public TestMageController owner => _owner;
    public SpriteRenderer frontSpriteRenderer => _frontSpriteRenderer;
    public SpriteRenderer backSpriteRenderer  => _backSpriteRenderer;

    //CHANGE
    public int damage = 0; // Урон от заклинания
    public int healHp = 0;   // Лечение от заклинания

    void Awake()
    {
        _frontSpriteRenderer     = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backSpriteRenderer      = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        _outlineController       = transform.GetChild(2).gameObject.GetComponent<OutlineController>();
    }

    // TEST
    bool isup = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isup)
                StartCoroutine(PositionBackUp());
            else
                StartCoroutine(PositionFrontUp());
            isup = !isup;
        }
    }
    // TEST

    // настроить карту
    public void SetupCard(Card card, Sprite back = null)
    {
        // данные карты
        _card = card;
        // передняя часть карты
        _frontSpriteRenderer.sprite = card.front;
        // задняя часть карты
        if (back != null)
            _backSpriteRenderer.sprite  = back;
        // подсветка карты
        Color cardColor;
        if (card.cardType == CardType.SPELL)
            cardColor = SPELL_CARD_MAIN_COLOR[((SpellCard) card).order];
        else
            cardColor = CARD_MAIN_COLOR[card.cardType];
        _outlineController.SetColor(cardColor);
    }

    // установить владельца карты
    public void SetOwner(TestMageController owner)
    {
        _owner = owner;
    } 

    // триггер при наведении курсора на карту
    // public void OnMouseOverTrigger()
    // {
    //     if (discoverable)
    //     {
    //         if (withOwner && isSpell)
    //         {
    //             _mouseOverTime += Time.deltaTime;
    //             if (_mouseOverTime >= _cardShowInfoWaitTime)
    //                 UIManager.instance.ShowCardInfo(card.front, true);
    //             owner.owner.OnSpellCardSelected(this, true);
    //         }
    //     }
    // }

    // триггер при выходе курсора из области карты
    // public void OnMouseExitTrigger()
    // {
    //     _mouseOverTime = 0.0f;
    //     UIManager.instance.ShowCardInfo(card.front, false);
    //     if (discoverable)
    //     {
    //         if (isSpell && withOwner)
    //             owner.owner.OnSpellCardSelected(this, false);
    //     }
    // }

    // триггер при клике на карту
    // public void OnMouseDownTrigger()
    // {
    //     if (discoverable && GameManager.instance.isSpellCreationState)
    //     {
    //         if (withOwner)
    //         {
    //             if (isSpell)
    //             {
    //                 SpellCard spellCard = (SpellCard) card;
    //                 Order order = spellCard.order;
    //                 if (inHand)
    //                 {
    //                     if (order == Order.WILDMAGIC)
    //                         StartCoroutine(owner.AddWildMagicToSpell(this));
    //                     else
    //                         StartCoroutine(owner.AddToSpell(this, order));
    //                 }
    //                 else if (inSpell)
    //                 {
    //                     StartCoroutine(owner.BackToHand(this, spellOrder));
    //                 }
    //             }
    //         }
    //     }
    // }


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

    public void StateToNoOwner()
    {
        cardState = CardState.NO_OWNER;
        _outlineController.SetProperties(true, false);
    }

    public void StateToInSpell()
    {
        cardState = CardState.IN_SPELL;
        _outlineController.SetProperties(true, false);
    }

    public void StateToInHand()
    {
        cardState = CardState.IN_HAND;
        _outlineController.SetProperties(false, true);
    }

    // CHANGE
    public int RollDice(int numberDice)
    {
        int totalRoll = 0; // Счет выпавших кубиков
        for (int i = 1; i < numberDice;  i++)
        {
           totalRoll += Random.Range(1, 6); // Бросок кубика
        }
        return totalRoll;
    }

    // CHANGE
    public int NumberDice(Sign sign)
    {
        int numberDice = 0; // Кол-во кубиков
        // Цикл нахождения кол-во одинаковых знаков карты заклинания => кол-во кубиков 
        foreach(TestCardController spellCard in owner.spell)
        {
            if(spellCard == null)
            {
                continue;
            }
            if(((SpellCard) spellCard.card).sign == sign)
            {
                numberDice+= 1;
            }
        }
        return numberDice;
    }

////////////////////////////////////////////////////////////////////////////////////
/* 
Методы карт
*/
////////////////////////////////////////////////////////////////////////////////////

    // foreach в одну строчку 
    // listTargets.ForEach(mage => print(mage));

    public void ExecuteSpell()
    {
        StartCoroutine(card.spell);
    }

    // Урон магу (Урон, Цель)
    public void DamageToTarget(int damage, TestMageController target)
    {
        target.TakeDamage(damage);
    } 

    // Урон нескольким магам (Урон, Лист Целей)
    public void DamageToTargets(int damage, List<TestMageController> listTargets)
    {
        foreach(TestMageController target in  listTargets)
        {
            DamageToTarget(damage, target);
        }
        
    } 

    // Лечение мага (Жизни, Цель)
    public void HealToTarget(int heal, TestMageController target)
    {
        target.Heal(heal);
    }
    
    // Урон магу и его соседям слева и справа
    public void DamageToTargetsNeighbors(int damage, int damageNeighbors , List<TestMageController> listTargets)
    {
        foreach(TestMageController target in  listTargets)
        {
            DamageToTarget(damage, target);
            DamageToTarget(damageNeighbors, target.rightMage);
            DamageToTarget(damageNeighbors, target.leftMage);
        }
    } 

    // Увеличение урона от кол-ва одинаковых знаков (некоторые карты)
    public int BuffDamageSing(Sign sign)
    {
        int BuffDamage = 0; // Стартовое увеличение урона
        // Цикл нахождения кол-во одинаковых знаков карты заклинания => Увеличение урона
        foreach(TestCardController spellCard in owner.spell)
        {
            if(spellCard == null)
            {
                continue;
            }
            if(((SpellCard) spellCard.card).sign == sign)
            {
                BuffDamage+= 1;
            }
        }
        return BuffDamage;
    }
////////////////////////////////////////////////////////////////////////////////////
/* 
Карты заклинаний
*/
////////////////////////////////////////////////////////////////////////////////////

    // От Короля Оберона
    public IEnumerator  OtKoroluaOberona()
    {
        healHp = 2;
        HealToTarget(healHp, owner);
        
        yield break;
    }

    // Драконий сундук
    public IEnumerator  DrakoniuSundyk()
    {
        
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
    
        var listTargets = new List<TestMageController> {}; // Лист списка целей
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
            damage = 3;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        // Нахождение магов без сокровища
        foreach(TestMageController mage in TestGameManager.instance.mages)
        {
            if(mage.treasures.Count == 0)
            {
                if(mage != owner)
                {
                    listTargets.Add(mage);
                }  
            }
        }

        DamageToTargets(damage, listTargets);

        yield break;
    }
    
    // Договор с Дьяволом
    // Недописанная карта
    // Нет отжатия прихода у жертвы и добавление к своему заклинания
    public IEnumerator DogovorSDuavolom()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого Живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
        damage = 2;
        //Отжать у жертвы Delivery и добавить к заклинанию
        }

        DamageToTargets(damage, listTargets);
        yield break;
    }

    // Кислотный слив
    public IEnumerator KislotnuiSliv()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        var listTargets = new List<TestMageController> {}; // Лист целей
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
            damage = 4;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        listTargets.Add(owner.leftMage);
        listTargets.Add(owner.rightMage);

        DamageToTargets(damage, listTargets);
        
        yield break;
    }

    // Экзорцизм
    public IEnumerator Exorcism()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        var listTargets = new List<TestMageController> {}; // Лист целей
        var listTargetsDeads = new List<TestMageController> {}; // Лист целей с жетоном недобитого колдуна
        int damageDeads = 0; // Урон недобитым магам


        foreach(TestMageController mage in TestGameManager.instance.mages)
        {
            if(mage != owner)
            {
                if(mage.deadMedals != 0)
                {
                    listTargetsDeads.Add(mage); // сохранение магов с жетоном недобитого колдуна
                }
                else
                {        
                    listTargets.Add(mage); // без жетона                 
                }
            }
        }

        if      (resultDice <= 4)
        {
            damage = 1;
            DamageToTarget(damage, owner);
        }
        else if (resultDice <= 9)
        {
            damage = 2;
            damageDeads = 4;
            DamageToTargets(damage, listTargets);
            DamageToTargets(damageDeads, listTargetsDeads);
        }
        else
        {
            damage = 4;
            damageDeads = 8;
            DamageToTargets(damage, listTargets);
            DamageToTargets(damageDeads, listTargetsDeads);
        }
        
        yield break;
    }
    
    // Петушок
    public IEnumerator Petyshok()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 1;}
        else                     {damage = 7;}

        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого Живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        DamageToTargets(damage, listTargets);
        yield break;
    }
    
    // Шарах Молнии
    public IEnumerator SharahMolnuu()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        var listTargets = new List<TestMageController> {}; // Лист целей

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        listTargets.Add(owner.leftMage);

        if (owner.leftMage.leftMage != owner)
            listTargets.Add(owner.leftMage.leftMage);

        DamageToTargets(damage, listTargets);
        yield break;
    }
    
    // Химерический Хохот
    public IEnumerator HimericheskiuXoxot()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 4;}

        DamageToTarget(damage, owner.rightMage); //Правый маг
        yield break;
    }
    
    // Ядерный синтез
    public IEnumerator YaderniuSintez()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        int damageNeighbors = 0; // Урон соседям

        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого Живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        if      (resultDice <= 4)
        {
            damage = 1;
            DamageToTargets(damage, listTargets);
        }
        else if (resultDice <= 9)
        {
            damage = 3;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, listTargets);// Урон магу и его соседям справа и слева
        }
        else
        {
            damage = 5;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, listTargets);// Урон магу и его соседям справа и слева
        }
        
        yield break;
    }

    // Раздвоитель личности
    // Недописанная карта
    // Нет сброса выбранного жертвой сокровища
    public IEnumerator RazdvoitelLichnosti()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                  
        {
            // if(owner.leftMage.ChooseAndDropTreasure())
            // {
            //     damage = 3;
            // }
            // else
            // {
            //     damage = 6;
            // }
        }

        DamageToTarget(damage, owner.leftMage); //Левый маг
        yield break;
    }
    
    // Кулак природы
    public IEnumerator KylakPrirodi()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        DamageToTarget(damage, owner.leftMage); //Левый маг
        yield break;
    }
    // Шалтай Болтай
    public IEnumerator  ShaltaiBoltai()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        magesWithOutOwner.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else
        {
            damage = 5;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        DamageToTargets(damage, listTargets);

        yield break;
    }

    // Удар милосердия
    public IEnumerator  YdarMiloserdiya()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        magesWithOutOwner.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 3;}

        DamageToTargets(damage, listTargets);

        yield break;
    }
    
    // Змеиный жор
    public IEnumerator  ZmeinuiJor()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        var listTargets = new List<TestMageController> {}; // Лист целей
        resultDice = 10;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 2 * BuffDamageSing(((SpellCard) card).sign);}// Увеличение урона на кол-во знаков травы(для этой карты)

        listTargets.Add(owner.leftMage);
        listTargets.Add(owner.rightMage);

        DamageToTargets(damage, listTargets);

        yield break;
    }

    // Отсос Мозга
    // Недописанная карта
    // Нет выбора врага
    // Нет отжатия сокровища у данного врага
    public IEnumerator  OtsosMozga()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        // var target = owner.ChooseEnemyMage(); // Враг по выбору

        // if      (resultDice <= 4){damage = 1;}
        // else if (resultDice <= 9){damage = 3;}
        // else                     
        // {
        //     damage = 4;
        //     TakeEnemyTreasures(owner, target);// Отжать сокровище у врага по своему выбору
        // }

        // DamageToTarget(damage, target);

        yield break;
    }
    
    // Нетерпеливый
    // Недописанная карта
    // Нет свойства начинать ход первым
    // Проверка во время определения очередност хода игроков
    public IEnumerator  Neterpeliviu()
    {
        List<TestMageController> magesWithOutOwner = TestGameManager.instance.mages.FindAll(mage => owner !=mage);
        damage = 1;
        DamageToTargets(damage, magesWithOutOwner);

        yield break;
    }

    // Ритуальный
    // Недописанная карта
    // Нет выбора врага
    public IEnumerator  Rityalnui()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        // var target = owner.ChooseEnemyMage(); // Враг по выбору

        if(resultDice <= 4)
        {
            damage = 3;
            DamageToTarget(damage, owner);
        }
        // else if (resultDice <= 9)
        // {
        //     damage = 3;
        //     DamageToTarget(damage, target);
        // }
        // else                     
        // {
        //     damage = 5;
        // DamageToTarget(damage, target);
        // }

        yield break;
    }

    // Дьявольский
    // Недописанная карта
    // Нет выбора врага
    public IEnumerator  Dyavolskiu()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        int selfDamage = 0;
        // var target = owner.ChooseEnemyMage(); // Враг по выбору

        // if(resultDice <= 4)
        // {
        //     damage = 2;
        //     DamageToTarget(damage, target);
        // }
        // else if (resultDice <= 9)
        // {
        //     damage = 4;
        //     selfDamage = 1;
        //     DamageToTarget(damage, target);
        //     DamageToTarget(selfDamage, owner);
        // }
        // else                     
        // {
        //     damage = 5;
        //     selfDamage = 2;
        //     DamageToTarget(damage, target);
        //     DamageToTarget(selfDamage, owner);
        // }
        yield break;
    }


    
    // Дискотечный
    // Недописанная карта
    // Нужен метод выбора заводилы или прихода
    public IEnumerator  Diskotechnui()
    {
        // card.ExecuteSpell(ChooseCardInSpell());
        yield break;
    }
}
