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
    public string positionLeft = "left"; // Позиция слева
    public string positionRight= "right"; // Позиция справа

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
        for (int i = 0; i < numberDice;  i++)
        {
           totalRoll += Random.Range(1, 7); // Бросок кубика
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

    // public enum TargetType
    // {
    //     RANDOM,
    //     LOW_HP,
    //     HIGH_HP,
    //     ALL
    // }

    // public List<MageController> FindTargets(TargetType targetType)
    // {
    //     switch (targetType)
    //     {
    //         case TargetType.ALL:
    //             return AllEnemies();
    //         case TargetType.HIGH_HP:
    //             return HighHpTargets();
    //         case TargetType.LOW_HP:
    //             return LowHpTargets();
    //         case TargetType.RANDOM:
    //             return RandomEnemy();
    //     }
    // }

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
    public void HealToTarget(int healHp, TestMageController target)
    {
        target.Heal(healHp);
    }
    
    // Урон магу и его соседям слева и справа
    public void DamageToTargetsNeighbors(int damage, int damageNeighbors , List<TestMageController> listTargets)
    {
        foreach(TestMageController target in  listTargets)
        {
            DamageToTarget(damage, target);
            
            DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.rightMage, positionRight));
            DamageToTarget(damageNeighbors, IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));
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
    
    // Метод возвращающий случайного врага(принимает владельца)
    public TestMageController RandomEnemy()
    {
        // Создаем список без владельца карты
        List<TestMageController> magesWithOutOwner = AllEnemies();
        // Рандомим индекс
        int index = Random.Range(0, magesWithOutOwner.Count);
        // Возвращаем мага из списка
        return magesWithOutOwner[index];
    }
    
    // Метод возвращает цель, если цель мертва берет следующую цель или слева или справа
    public TestMageController IsDeadTargetLeftOrRight(TestMageController target, string position)
    {
        while (target.isDead)// Пока цель мертва
        {
            if(position == "left")
            {
                target = target.leftMage; // Берем следующего левого мага
            }
            else if(position == "right")
            {
                target = target.rightMage; // Берем следующего левого мага
            }
        }
        return target;
    }

    // Метод возвращает лист целей, Нахождение хилого врага из списка живых
    public List<TestMageController> LowHpTargets()
    {
        List<TestMageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    // Метод возвращает лист целей, Нахождение живучего врага из списка живых
    public List<TestMageController> HighHpTargets()
    {
        
        List<TestMageController> magesWithOutOwner = AllEnemies();
        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем

        return listTargets;
    }

    //Метод возвращает лист целей, нахождение каждого врага
    public List<TestMageController> AllEnemies()
    {
        List<TestMageController> magesWithOutOwner = TestGameManager.instance.aliveMages.FindAll(mage => owner !=mage);

        return magesWithOutOwner;
    }
    
////////////////////////////////////////////////////////////////////////////////////
/* 
Карты заклинаний
*/
////////////////////////////////////////////////////////////////////////////////////
// Перебрать все карты и исправить на списки живых магов ( Вроде сделанно )
// Перебрать все карты в которых есть могучий бросок, добавить баф от карты дохлого или мордоеда или и то и другое
// Нужно поле у мага, есть ли у него доп кубик 
// Нужно во всех картах прихода проверять, нет ли в заклинании "От шерочки с Машерочкой"

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
        foreach(TestMageController mage in TestGameManager.instance.aliveMages)
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

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
        damage = 2;
        //Отжать у жертвы Delivery и добавить к заклинанию
        }

        DamageToTargets(damage, HighHpTargets());
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


        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, positionRight));

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

        foreach(TestMageController mage in AllEnemies())
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

        DamageToTargets(damage, HighHpTargets());
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

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));

        if (IsDeadTargetLeftOrRight(owner.leftMage, positionLeft).leftMage != owner)
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
        
        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, positionRight)); //Правый маг
        yield break;
    }
    
    // Ядерный синтез
    public IEnumerator YaderniuSintez()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        int damageNeighbors = 0; // Урон соседям

        if      (resultDice <= 4)
        {
            damage = 1;
            DamageToTargets(damage, HighHpTargets());
        }
        else if (resultDice <= 9)
        {
            damage = 3;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, HighHpTargets());// Урон магу и его соседям справа и слева
        }
        else
        {
            damage = 5;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, HighHpTargets());// Урон магу и его соседям справа и слева
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
            // if(IsDeadTargetLeftOrRight(owner.leftMage, positionLeft).ChooseAndDropTreasure())
            // {
            //     damage = 3;
            // }
            // else
            // {
            //     damage = 6;
            // }
        }
        
        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.leftMage, positionLeft)); //Левый маг
        yield break;
    }
    
    // Кулак природы
    public IEnumerator KylakPrirodi()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.leftMage, positionLeft)); //Левый маг
        yield break;
    }
    // Шалтай Разболтай
    public IEnumerator  ShaltaiRazboltaui()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else
        {
            damage = 5;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        DamageToTargets(damage, LowHpTargets());

        yield break;
    }

    // Удар милосердия
    public IEnumerator  YdarMiloserdiya()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 3;}

        DamageToTargets(damage, LowHpTargets());

        yield break;
    }
    
    // Змеиный жор
    public IEnumerator  ZmeinuiJor()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        var listTargets = new List<TestMageController> {}; // Лист целей
        
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 2 * BuffDamageSing(((SpellCard) card).sign);}// Увеличение урона на кол-во знаков травы(для этой карты)

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, positionRight));

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
        damage = 1;
        DamageToTargets(damage, AllEnemies());

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
        // int selfDamage = 0;
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
    
    //Двуличный
    // Недописанная карта
    // Нет выбора колдуна
    public IEnumerator  Dvylichniu()
    {
        // var target = owner.ChooseMage(); // Маг по выбору

        // выдать сокровище выбранному магу
        // yield return TestGameManager.instance.treasuresDeck.PassCardsTo(target, 1); 

        // damage = target.treasures.Count * 2; // Урон = Кол-во сокровищ * 2
        
        // DamageToTarget(damage, target);

        yield break;
    }

    // Опарышный
    // Недоделанная карта
    // Не учитывает сокровища
    public IEnumerator  Oparishniu()
    {
        damage = 2 * BuffDamageSing(((SpellCard) card).sign);// Увеличение урона на кол-во знаков мрака(для этой карты)
        DamageToTargets(damage, HighHpTargets());

        yield break;
    }
    
    // Котострофический
    public IEnumerator  Kotostroficheskiu()
    {
        // Словарь маг key бросок value
        Dictionary<TestMageController, int> magesAndRolls = new Dictionary<TestMageController, int>();

        int numberDice = 1;// кол-во кубиков
        
        foreach(TestMageController mage in TestGameManager.instance.aliveMages)
        {
            int resultDice = RollDice(numberDice);
            if(mage == owner )
            {
                resultDice = resultDice + 2;// Если цель владелец, +2 к результату
            }
            magesAndRolls.Add(mage, resultDice); 
        }

        var maxValue = magesAndRolls.Values.Max();// Нахождение максимального броска

        foreach (var mageRoll in magesAndRolls)
        {
            if( mageRoll.Value == maxValue )
            {
                // Если маг выкинул максимальное число, получает сокровище
                yield return TestGameManager.instance.treasuresDeck.PassCardsTo(mageRoll.Key, 1);
            }
            else
            {
                // Остальные получают урон = броску
                DamageToTarget(mageRoll.Value,mageRoll.Key);
            }
        }
        yield break;
    }
    
    //Мозголомный
    public IEnumerator  Mozgolomniu()
    {
        // Урон
        damage = 3;
        // Выбираем случайного врага
        var randomEnemy = RandomEnemy();
        // Наносим урон
        DamageToTarget(damage, randomEnemy);
        // Берем сокровище для себя и случайного врага
        yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1);
        yield return TestGameManager.instance.treasuresDeck.PassCardsTo(randomEnemy, 1);
        
        yield break;
    }
    
    // Разрывной
    // Недописанная карта
    // Нет выбора врага
    // Нет выбора сокровища для сброса
    public IEnumerator  Razrivnoi()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        // int selfDamage = 0;
        // var target = owner.ChooseEnemyMage(); // Враг по выбору

        // if(resultDice <= 4)
        // {
        //     damage = 1;
        //     DamageToTarget(damage, target);
        // }
        // else if (resultDice <= 9)
        // {
        //     damage = 3;
        //     selfDamage = 1;
        //     DamageToTarget(damage, target);
        //     DamageToTarget(selfDamage, owner);
        // }
        // else                     
        // {
        //     damage = 4;
        //     DamageToTarget(damage, target);
        //     DropEnemyTreasures(target);
        // }
        yield break;
    }

    // Непонятный
    // Недописанная карта
    // Не учитывает знак который дает сокровище
    public IEnumerator  Neponuatnui()
    {
        
        int squares = owner.nonNullSpell.Select(spellCard => ((SpellCard) spellCard.card).sign).Distinct().Count();
        damage = 1 * (squares + owner.treasures.Count);
        
        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, positionRight));

        yield break;
    }
    
    //Шипастый
    public IEnumerator Shipastui()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){damage = 1; healHp = 0;}
        else if (resultDice <= 9){damage = 1; healHp = 1;}
        else                     {damage = 3; healHp = 3;}

        HealToTarget(healHp, owner);
        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, positionRight));
        
        yield break;
    }

    // Загнивающий
    // Недописанная карта
    // Убрать из списка сходивших магов
    // Нужна функция сбрасывания Quality у таргетов
    public IEnumerator Zagnivaushui()
    {
        List<TestMageController> magesWithOutOwner = TestGameManager.instance.aliveMages.FindAll(mage => owner !=mage);
        // Убрать с листа сходивших магов 

        magesWithOutOwner.Sort((mage1, mage2) => -mage1.health.CompareTo(mage2.health)); // Нахождение самого Живучего мага
        int maxHp = magesWithOutOwner[0].health; // Сохранение его здоровья
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health == maxHp); // Поиск магов с таким же здоровьем
        // DropQuality(listTargets);
        
        yield break;
    }
    
    //Каменючный
    public IEnumerator Kamenuchnui()
    {
        TestMageController target = owner.leftMage;// Берем левого врага у владельца

        damage = 1;

        while (target != owner)// Пока цель не владелец
        {
            if (!target.isDead)// Если враг не умер
            {
                DamageToTarget(damage, target);
                damage++;
            }
            target = target.leftMage; // Берем следующего левого мага
        }
        yield break;
    }

    // Аппетитный
    // Недописанная карта
    // Не учитывает знак который дает сокровище
    public IEnumerator  Appetitnui()
    {
        // Подсчет уникальных знаков в заклинании
        int uniqueSigns = owner.nonNullSpell.Select(spellCard => ((SpellCard) spellCard.card).sign).Distinct().Count();

        damage = 1 * (uniqueSigns);
        
        List<TestMageController> magesWithOutOwner = AllEnemies();// Все живые враги
        var listTargets = magesWithOutOwner.FindAll(mage => mage.health % 2 != 0); // Поиск магов с нечетным здоровьем
        DamageToTargets(damage, listTargets);
        yield break;
    }
    
    // Адовый
    // Недописанная карта
    // Не учитывает заклинания
    public IEnumerator  Adovui()
    {
        damage = 1 * BuffDamageSing(((SpellCard) card).sign);// Увеличение урона на кол-во знаков мрака(для этой карты)
        DamageToTargets(damage, AllEnemies());

        yield break;
    }
    
    //От старого Жгуна
    public IEnumerator  OtStarogoJgyna()
    {
        int numberDice = 1; // Кол-во кубиков 
        int resultDice = RollDice(numberDice); // Результат кубиков

        if(resultDice <= 3)
        {
            DamageToTarget(resultDice, owner); // Урон владельцу сколько выпало
        }
        else
        {
            HealToTarget(resultDice, owner); // Лечение владельцу, сколько выпало
        }

        yield break;
    }
    
    // От Драконьера
    // Недописанная карта
    // Не учитывает знак который дает сокровище
    public IEnumerator  OtDrakonera()
    {
        // Подсчет уникальных знаков в заклинании
        int uniqueSigns = owner.nonNullSpell.Select(spellCard => ((SpellCard) spellCard.card).sign).Distinct().Count();

        damage = 1 * (uniqueSigns);

        DamageToTargets(damage, AllEnemies());
        yield break;
    }
    
    // Кубический
    public IEnumerator  Kybicheskui()
    {
        // Словарь маг key бросок value
        Dictionary<TestMageController, int> magesAndRolls = new Dictionary<TestMageController, int>();

        int numberDice = 1; // кол-во кубиков

        int firstResultOwner  = RollDice(numberDice); // Результат первого кубика владельца
        int secondResultOwner = RollDice(numberDice); // Результат второго кубика владельца

        foreach(TestMageController mage in AllEnemies())
        {
            int resultDice = RollDice(numberDice);
            magesAndRolls.Add(mage, resultDice); // Каждый маг кидает кубик
        }

        foreach (var mageRoll in magesAndRolls)
        {
            // Если совпало с 1 кубиком, получает урон = броску
            if( mageRoll.Value == firstResultOwner )  
            {
                DamageToTarget(mageRoll.Value, mageRoll.Key); 
            }
            // Если совпало с 2 кубиком, получает урон = броску
            else if ( mageRoll.Value == secondResultOwner)
            {
                DamageToTarget(mageRoll.Value, mageRoll.Key);
            }
        }
        yield break;
    }

    // Рубин в башке
    // Недописанная карта
    // Нет выбора цели
    // Нет добавления случайной карты к заклинанию
    // Нет добавления выбранной карты к заклинанию
    public IEnumerator  RubinVBashke()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        // var target = owner.ChooseEnemyMage(); // Враг по выбору
        damage = 1;
        //DamageToTarget(damage, target);
        //if (resultDice <= 9)
        // {
             // Случайная карта с руки к заклинанию   
        // }
        // else                     
        // {
            // Выбранная карта с руки к заклинанию
        // }

        yield break;
    }

    // Громобойный
    // Недоделанная карта
    // Не учитывает знаки сокровищ
    public IEnumerator  Gromoboinui()
    {
        // Урон
        damage = 2;

        // Подсчет уникальных знаков в заклинании
        int uniqueSigns = owner.nonNullSpell.Select(spellCard => ((SpellCard) spellCard.card).sign).Distinct().Count();

        for(int i = 0; i < uniqueSigns; i++ )
        {
            // Выбираем случайного врага
            var randomEnemy = RandomEnemy();
            // Наносим урон
            DamageToTarget(damage, randomEnemy);
        }

        yield break;
    }

    // Отборный
    public IEnumerator  Otbornui()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9)
        {
            damage = 2;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }
        else
        {
            damage = 5;
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        DamageToTargets(damage, HighHpTargets());

        yield break;
    }

    // Качковый
    // Недописанная карта
    // Нет добавления случайно карты к своему заклинанию
    public IEnumerator  Kachkovui()
    {
        healHp = 2;
        HealToTarget(healHp, owner);

        List<TestMageController> allMages = TestGameManager.instance.aliveMages.ToList();
        allMages.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага

        int maxHp = allMages[0].health; // Сохранение его здоровья

        if(owner.health == maxHp)
        {

            // Добавить случайную карту к своему заклинанию
        }
        
        yield break;
    }

    // Мошоночный 
    // Недописанная карта
    // Нет права выбора у игроков
    public IEnumerator Moshonochnui()
    {
       
        damage = 3;
        
        // if(!ChooseAndDropTreasure(IsDeadTargetLeftOrRight(owner.leftMage, positionLeft)))
        // {
        //     DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));
        // }
        // if(!ChooseAndDropTreasure(IsDeadTargetLeftOrRight(owner.RightMage, positionRight)))
        // {
        //     DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.RightMage, positionRight)); 
        // }

        yield break;
    }

    // От Бена Вуду
    // Недописанная карта
    // Нет сброса сокровища
    public IEnumerator  OtBenaVydy()
    {             
        int numberDice = 1; // кол-во кубиков

        foreach(TestMageController mage in AllEnemies())
        {
            int resultDice = RollDice(numberDice);
            DamageToTarget(resultDice, mage);
        }
        //ChooseAndDropTreasure(owner);

        yield break;
    } 

    
    // От Сера Кладомота
    public IEnumerator  OtSeraKladomota()
    {
        int numberDice = 1; // кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить сокровище

        yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        
        foreach(TestMageController mage in AllEnemies())
        {
            int resultDice = RollDice(numberDice);
            if (resultDice == necessaryResult)
                yield return TestGameManager.instance.treasuresDeck.PassCardsTo(mage, 1);
        }

        yield break;
    }
    
    // От Магмуда Поджигая
    // Недоделанная карта
    // Нет метода выбора 
    public IEnumerator  OtMagmydaPodjigatelua()
    {
        int damageSolo = 3; // Урон одному врагу
        int damageAll  = 1; // Урон всем
        // if(Choose() == positionLeft)
        // {
               // Нанести урон левому врагу
        //     DamageToTarget(damageSolo, IsDeadTargetLeftOrRight(owner.leftMage, positionLeft));
        // }
        // else
        // {
               // Нанести урон всем врагам
        //     DamageToTargets(damageAll, AllEnemies());
        // }
 
        yield break;
    }

    // От Мордоеда
    // Недописанная карта
    // Скопировать решение у карты дохлый колдун
    public IEnumerator  OtMordoeda()
    {
        // Скопировать решение у карты дохлый колдун
        yield break;
    }
    // В этом раунде добавляй 1 кубик к своим могучим броскам.

    // От Горячей штучки
    public IEnumerator  OtGoruacheiShtychki()
    {
        damage = 3;
        DamageToTargets(damage, HighHpTargets());

        yield break;
    }

    // От профессора Ахалая
    // Недописанная карта
    // Дописать шальную магию и проверку на наличие ее в заклинании
    public IEnumerator  OtProfessoraAxalaya()
    {
        damage = 3;
        DamageToTarget(damage, RandomEnemy());

        // if(owner.spell.Where(card => card == ShalnayaMagiya))
        //     yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // Жмураган
    public IEnumerator Jmyragan()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика

        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 6;}
        

        DamageToTargets(damage, HighHpTargets());
        yield break;
    }

    //От Шерочки с Машерочкой
    // Недоделанная карта
    // А хуй его знает что сюда писать
    public IEnumerator OtSherochkiSMasherochkoi()
    {
        yield break;
    }

    // От Тай Тьфуна
    // Недописанная карта
    // Поменять список целей на список уже сходивших
    public IEnumerator OtTauiTfyna()
    {
        damage = 3;

        // DamageToTargets(damage, ShodivshieMagi);
        yield break;
    }

    
    // Отсос Мозга
    // Недописанная карта
    // Нет выбора врага
    // Нет отжатия сокровища у данного врага
    public IEnumerator  OtSfinksenona()
    {
        // var target = owner.ChooseEnemyMage(); // Враг по выбору
 
        // TakeEnemyTreasures(owner, target);// Отжать сокровище у врага по своему выбору
     
        yield break;
    }

    // От бухого Уокера
    // Недописанная карта
    // Не учитывает знаки от сокровищ
    public IEnumerator  OtByhogoYokera()
    {
        damage = 3;
        // Словарь маг key бросок value
        Dictionary<TestMageController, int> magesAndRolls = new Dictionary<TestMageController, int>();

        int numberDice = 1;// кол-во кубиков
        
        foreach(TestMageController mage in TestGameManager.instance.aliveMages)
        {
            int resultDice = RollDice(numberDice);
            if(mage == owner )
            {
                // Если цель владелец, +1 за каждый уникальный знак
                resultDice = resultDice + BuffDamageSing(((SpellCard) card).sign);
            }
            magesAndRolls.Add(mage, resultDice);
        }

        var minValue = magesAndRolls.Values.Min();// Нахождение минимального броска

        foreach (var mageRoll in magesAndRolls)
        {
            if( mageRoll.Value == minValue )
            {
                DamageToTarget(damage, mageRoll.Key);
            }
        }
        yield break;
    }
    
    // От Розового Бутончика
    // Недописанная карта
    // Не учитывает знаки от сокровищ
    public IEnumerator  OtRozovogoBytonchika()
    {
        // Уникальные знаки
        int squares = owner.nonNullSpell.Select(spellCard => ((SpellCard) spellCard.card).sign).Distinct().Count();
        healHp = 1 * squares;
        HealToTarget(healHp, owner);
        
        yield break;
    }

    // От Поганого Мерлина
    public IEnumerator  OtPoganogoMerlina()
    {
        damage = 1 * TestGameManager.instance.aliveMages.Count();
        DamageToTargets(damage, HighHpTargets());
        yield return TestGameManager.instance.deadsDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // От Брадострела
    // Недописанная карта
    // Нужен метод копии прихода
    public IEnumerator  OtBradostrela()
    {
        // card.ExecuteSpell(owner.spell.deliveries);
        yield break;
    }

    // От Пыща с Тыдыщем
    // Полностью ненаписанная карта
    public IEnumerator  OtPushaSTudushem()
    {
        // Игрок берет карту, если она заводила, добавляет к заклинанию, иначе сброс ( так 4 раза )
        yield break;
    }
    
    // От Феечки смерти
    // Недописанная карта
    // Нет выбора врага
    public IEnumerator  OtFeechkiSmerti()
    {
        damage = 2;
        // var target = owner.ChooseEnemyMage(); // Враг по выбору
        //DamageToTarget(damage, target);
        // while(target.isDead)
        // {
        //     //var target = owner.ChooseEnemyMage(); // Враг по выбору
        //     //DamageToTarget(damage, target);
        // }

        yield break;
    }
    
    // От д-ра Конея Дуболома
    public IEnumerator  OtDraKorneyaDyboloma()
    {
        healHp = 3; // Сколько жизней нужно вылечить
        HealToTarget(healHp, owner); // Лечение себя

        Dictionary<TestMageController, int> magesAndRolls = new Dictionary<TestMageController, int>();

        int numberDice = 1;// кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить лечение
        
        // Каждый враг бросает кубик 
        foreach(TestMageController mage in AllEnemies())
        {
            int resultDice = RollDice(numberDice);
            magesAndRolls.Add(mage, resultDice);
        }

        // Проходим по каждому врагу и смотрим на его бросок
        foreach (var mageRoll in magesAndRolls)
        {
            if( mageRoll.Value == necessaryResult )// Если враг выкинул 6
            {
                HealToTarget(healHp, mageRoll.Key);// Лечение врага
            }
        }
        
        yield break;
    }

    // Фонтан Молодости
    public IEnumerator FontanMolodosti()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        
        if      (resultDice <= 4){healHp = 0;}
        else if (resultDice <= 9){healHp = 2;}
        else                     {healHp = 4;}

        HealToTarget(healHp, owner); // Лечение владельца
        yield break;
    }
    
    // От Зачуханного Комбайна
    // Полностью ненаписанная карта
    public IEnumerator  OtZachyhannogoKombauna()
    {
        // Игрок берет карту, если ее знак есть в заклинании,
        // добавляет к заклинанию,
        // иначе сброс ( так 2 раза )
        // 
        yield break;
    }

    // Вихрь бодрости
    // Недоделанная карта
    // Нет выбора какую карту сбросить овнеру
    public IEnumerator VihrBodrosti()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        damage = 0; 

        if      (resultDice <= 4)
        {
            // owner.card.ToFold(ChooseCardInSpell());
        }
        else if (resultDice <= 9)
        {
            damage = 2;
            DamageToTargets(damage, AllEnemies());
            // owner.card.ToFold(ChooseCardInSpell(), 2);
        }
        else
        {
            damage = 2;
            DamageToTargets(damage, AllEnemies());
            // owner.card.ToFold(ChooseCardInSpell(), 2);
            yield return TestGameManager.instance.treasuresDeck.PassCardsTo(owner, 1);
        }
        
        yield break;
    }

    // Самовыпил
    public IEnumerator Samovipil()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        damage = 0;
        int damageSelf = 1;

        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 5;}

        DamageToTarget(damageSelf, owner);
        DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, positionRight)); //Правый маг
        
        yield break;
    }

    
    // Мясной фарш
    public IEnumerator MyasnouiFarsh()
    {
        int resultDice = RollDice(NumberDice(((SpellCard) card).sign)); // Бросок кубика
        damage = 0;

        int maxHp = owner.health; // Сохранение его здоровья

        // Поиск врагов с большим здоровьем чем у владельца
        var listTargets = TestGameManager.instance.aliveMages.FindAll(mage => mage.health > maxHp); 

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 4;}

        DamageToTargets(damage, listTargets); 
        
        yield break;
    }
    
}
