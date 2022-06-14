using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardsToolKit;

public class ISpellCardController : ICardController
{

    // словарь для получения цвета подсветки карты в зависимости от порядка в заклинании
    public static Dictionary<Order, Color> SPELL_CARD_MAIN_COLOR = new Dictionary<Order, Color>()
    {
        { Order.SOURCE,      new Color(1.0f, 0.8427867f, 0.0f) },
        { Order.QUALITY,     new Color(0.9960785f, 0.4745098f, 0.0f) },
        { Order.DELIVERY,    new Color(0.9960785f, 0.0509804f, 0.1529412f) },
        { Order.WILDMAGIC,   new Color(0.0f, 0.60784f, 0.5843f) }
    };

    public const string POSITION_LEFT  = "left";  // Позиция слева
    public const string POSITION_RIGHT = "right"; // Позиция справа

    bool _ownerGoesFirst      = false;
    bool _discoverable        = true;               // можно ли взаимодействовать с картой

    Order _spellOrder         = Order.WILDMAGIC;    // порядок в заклинании (нужен для шальной магии)

    
    public bool ownerGoesFirst => _ownerGoesFirst;
    public Order spellOrder
    {
        get => _spellOrder;
        set => _spellOrder = value;
    }

    public SpellCard spellCard => (SpellCard) _card;
    public Order order => spellCard.order;
    public Sign  sign  => spellCard.sign;


    protected void Start()
    {
        base.Start();
        _ownerGoesFirst = _card.spell == "Neterpeliviu";
    }

    public override void SetupCard(Card card, DeckController deck = null, Sprite back = null)
    {
        // данные карты
        _card = card;
        // колода карты
        _sourceDeck = deck;
        // передняя часть карты
        _frontSpriteRenderer.sprite = card.front;
        // задняя часть карты
        if (back != null)
            _backSpriteRenderer.sprite  = back;
        // подсветка карты
        Color cardColor;
        cardColor = SPELL_CARD_MAIN_COLOR[order];
        _outlineController.SetColor(cardColor);
    }

    void AddToSpell()
    {
        // if (order == Order.WILDMAGIC)
            // StartCoroutine(owner.AddWildMagicToSpell(this)); UNCOMMENT
        // else
            // StartCoroutine(owner.AddToSpell(this, order)); UNCOMMENT
    }

    // вернуть карту в руку из заклинания
    void BackToHand()
    {
        // StartCoroutine(owner.SpellCardBackToHand(this, spellOrder)); UNCOMMENT
    }

    public override void OnMouseOverAction()
    {
        SelectCard(true);
        ShowCardInfo();
    }

    public override void OnMouseExitAction()
    {
        HideCardInfo();
        if (withOwner && _discoverable)
            SelectCard(false);
    }

    public override void OnMouseDownAction()
    {
        if (gm.isSpellCreationState && withOwner)
        {
            if (inHand)
                AddToSpell();   
            else if (inSpell)
                BackToHand();   
        }
        else if (gm.isChoosingState)
        {
            gm.StopChoosing();
        }
    }

    protected override void ShowCardInfo()
    {
        _mouseOverTime += Time.deltaTime;
        // if (_mouseOverTime >= _cardShowInfoWaitTime)
            // UIManager.instance.ShowCardInfo(this, true); UNCOMMENT
    }

    // спрятать информацию о карте
    protected override void HideCardInfo()
    {
        _mouseOverTime = 0.0f;
        // UIManager.instance.ShowCardInfo(this, false); UNCOMMENT
    }




    // Забрать карту определенного порядка  
    public IEnumerator StealCardFromSpell(Order order, List<MageController> listTargets)
    {
        List<MageController> targets = listTargets.FindAll(mage => mage.HasCardOfOrderInSpell(order));
        foreach (MageController target in targets)
            yield return target.PassSpellOfOrderTo(owner, order);
    }









    // Драконий сундук
    public IEnumerator  DrakoniuSundyk()
    {
        List<int> rolls = RollDice(NumberDice(spellCard.sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        int resultDice  = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
            damage = 3;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        // Нахождение магов без сокровища
        List<MageController> listTargets = gm.aliveMages.FindAll(mage => mage != owner && mage.treasures.Count == 0);

        yield return DamageToTargets(damage, listTargets);

        yield break;
    }

    // Договор с Дьяволом
    public IEnumerator DogovorSDuavolom()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();
                
        int damage = 0;
        if   (resultDice <= 4){damage = 1;}
        else                  {damage = 2;}

        List<MageController> listTargets = FindTargets(TargetType.HIGH_HP);
        yield return DamageToTargets(damage, listTargets);

        if (resultDice > 9)
        {
            yield return Highlight(false);
            yield return StealCardFromSpell(Order.DELIVERY, listTargets);
        }

        yield break;
    }

    // Кислотный слив
    public IEnumerator KislotnuiSliv()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей
        
        int damage;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else
        {
            damage = 4;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));

        yield return DamageToTargets(damage, listTargets);
        
        yield break;
    }

    // Экзорцизм
    public IEnumerator Exorcism()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        List<MageController> listTargets      = new List<MageController>(); // Лист целей
        List<MageController> listTargetsDeads = new List<MageController>(); // Лист целей с жетоном недобитого колдуна
        
        int damageDeads = 0; // Урон недобитым магам

        foreach(MageController mage in AllEnemies())
        {
            if(mage.medals != 0)
                listTargetsDeads.Add(mage); // сохранение магов с жетоном недобитого колдуна
            else
                listTargets.Add(mage); // без жетона                 
        }

        int damage = 0;
        if      (resultDice <= 4)
        {
            damage = 1;
            yield return DamageToTarget(damage, owner);
        }
        else if (resultDice <= 9)
        {
            damage = 2;
            damageDeads = 4;
            yield return DamageToTargets(damage, listTargets);
            yield return DamageToTargets(damageDeads, listTargetsDeads);
        }
        else
        {
            damage = 4;
            damageDeads = 8;
            yield return DamageToTargets(damage, listTargets);
            yield return DamageToTargets(damageDeads, listTargetsDeads);
        }
        
        yield break;
    }

    // Петушок
    public IEnumerator Petyshok()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();
        
        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 1;}
        else                     {damage = 7;}

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        yield break;
    }

    // Шарах Молнии
    public IEnumerator SharahMolnuu()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT));

        if (IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT).leftMage != owner)
            listTargets.Add(owner.leftMage.leftMage);

        yield return DamageToTargets(damage, listTargets);
        yield break;
    }

    // Химерический Хохот
    public IEnumerator HimericheskiuXoxot()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 4;}
        
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT)); //Правый маг
        yield break;
    }

    // Ядерный синтез
    public IEnumerator YaderniuSintez()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        List<MageController> targets = FindTargets(TargetType.HIGH_HP);

        int damageNeighbors = 0; // Урон соседям
        
        int damage = 0;
        if      (resultDice <= 4)
        {
            damage = 1;
            yield return DamageToTargets(damage, targets);
        }
        else if (resultDice <= 9)
        {
            damage = 3;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, targets);// Урон магу и его соседям справа и слева
        }
        else
        {
            damage = 5;
            damageNeighbors = 1;
            DamageToTargetsNeighbors(damage, damageNeighbors, targets);// Урон магу и его соседям справа и слева
        }
        
        yield break;
    }



    // Кулак природы
    public IEnumerator KylakPrirodi()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();
        
        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 4;}

        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.leftMage, POSITION_LEFT)); //Левый маг
        yield break;
    }

    // Шалтай Разболтай
    public IEnumerator  ShaltaiRazboltaui()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else
        {
            damage = 5;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        yield return DamageToTargets(damage, FindTargets(TargetType.LOW_HP));

        yield break;
    }

    // Удар милосердия
    public IEnumerator  YdarMiloserdiya()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 3;}

        yield return DamageToTargets(damage, FindTargets(TargetType.LOW_HP));

        yield break;
    }

    // Змеиный жор
    public IEnumerator  ZmeinuiJor()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        List<MageController> listTargets = new List<MageController>(); // Лист целей
        
        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 2;}
        else                     {damage = 2 * BuffDamageSign(sign);}// Увеличение урона на кол-во знаков травы(для этой карты)

        listTargets.Add(IsDeadTargetLeftOrRight(owner.leftMage,  POSITION_LEFT));
        listTargets.Add(IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));

        yield return DamageToTargets(damage, listTargets);

        yield break;
    }

    // Нетерпеливый
    // Начинать ход первым
    // Проверка во время определения очередности хода игроков
    public IEnumerator  Neterpeliviu()
    {
        yield return DamageToTargets(1, AllEnemies());

        yield break;
    }

















    // Котострофический
    public IEnumerator  Kotostroficheskiu()
    {
        // Словарь маг key бросок value
        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1; // кол-во кубиков
        
        foreach (MageController mage in gm.aliveMages)
        {
            
            mage.mageIcon.Highlight(true);

            List<int> rolls = RollDice(numberDice); // Бросок кубика 
            
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();

            string resultDiceText = resultDice.ToString();

            if( mage == owner )
            {
                resultDice += 2;// Если цель владелец, +2 к результату
                resultDiceText += " +2";
            }

            mage.mageIcon.ShowInfoText(resultDiceText);

            magesAndRolls.Add(mage, resultDice); 

            mage.mageIcon.Highlight(false);
        }

        var maxValue = magesAndRolls.Values.Max();// Нахождение максимального броска

        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            if( mageRoll.Value == maxValue )
            {
                mage.mageIcon.Highlight(true);
                
                // Если маг выкинул максимальное число, получает сокровище
                yield return gm.treasuresDeck.PassCardsTo(mage, 1);

                mage.mageIcon.Highlight(false);
            }
            else
            {
                // Остальные получают урон = броску
                yield return DamageToTarget(mageRoll.Value, mageRoll.Key);
            }

            mage.mageIcon.HideInfoText();
        }
        yield break;
    }

    //Мозголомный
    public IEnumerator  Mozgolomniu()
    {
        // Урон
        int damage = 3;
        
        // Выбираем случайного врага
        MageController randomEnemy = FindTargets(TargetType.RANDOM)[0];
        yield return OnRandomEnemy(randomEnemy);
        
        // Наносим урон
        yield return DamageToTarget(damage, randomEnemy);
        
        // Берем сокровище для себя и случайного врага
        yield return gm.treasuresDeck.PassCardsTo(owner, 1);
        yield return gm.treasuresDeck.PassCardsTo(randomEnemy, 1);
        
        yield break;
    }





    //Шипастый
    public IEnumerator Shipastui()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();
        
        int damage = 0;
        int healHp = 0;
        if      (resultDice <= 4){damage = 1; healHp = 0;}
        else if (resultDice <= 9){damage = 1; healHp = 1;}
        else                     {damage = 3; healHp = 3;}

        yield return HealToTarget(healHp, owner);
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT));
        
        yield break;
    }

    // Загнивающий
    // Убрать из списка сходивших магов
    // Нужна функция сбрасывания Quality у таргетов
    public IEnumerator Zagnivaushui()
    {
        // Убрать с листа сходивших магов 
        List<MageController> targets = FindTargets(TargetType.HIGH_HP).FindAll(mage => !mage.spellsAreExecuted);
        
        targets.ForEach(mage => mage.DropSpellOfOrder(Order.QUALITY));
        
        yield break;
    }

    //Каменючный
    public IEnumerator Kamenuchnui()
    {
        MageController target = owner.leftMage; // Берем левого врага у владельца

        int damage = 1;

        while (target != owner)// Пока цель не владелец
        {
            if (!target.isDead)// Если враг не умер
            {
                yield return DamageToTarget(damage, target);
                damage++;
            }
            target = target.leftMage; // Берем следующего левого мага
        }
        yield break;
    }

    //От старого Жгуна
    public IEnumerator  OtStarogoJgyna()
    {
        List<int> rolls = RollDice(1); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        if(resultDice <= 3)
        {
            yield return DamageToTarget(resultDice, owner); // Урон владельцу сколько выпало
        }
        else
        {
            yield return HealToTarget(resultDice, owner); // Лечение владельцу, сколько выпало
        }

        yield break;
    }

    // От Короля Оберона
    public IEnumerator  OtKoroluaOberona()
    {
        yield return HealToTarget(2, owner);
        
        yield break;
    }




    // Кубический
    public IEnumerator  Kybicheskui()
    {
        // Словарь маг key бросок value
        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1; // кол-во кубиков

        owner.mageIcon.Highlight(true);

        List<int> rolls       = RollDice(numberDice); // Бросок кубика 
        yield return OnDiceRoll(rolls, showBonus: false);
        int firstResultOwner  = rolls.Sum();          // Результат первого кубика владельца

        owner.mageIcon.ShowInfoText($"{firstResultOwner}");

        rolls = RollDice(numberDice);
        yield return OnDiceRoll(rolls, showBonus: false);
        int secondResultOwner = rolls.Sum(); // Результат второго кубика владельца

        owner.mageIcon.ShowInfoText($"{firstResultOwner} + {secondResultOwner}");

        owner.mageIcon.Highlight(false);

        foreach (MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);
            
            rolls = RollDice(numberDice);
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();
            magesAndRolls.Add(mage, resultDice); // Каждый маг кидает кубик

            mage.mageIcon.ShowInfoText(resultDice.ToString());

            mage.mageIcon.Highlight(false);
        }

        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            int mageDice = mageRoll.Value;


            // Если совпало с 1 кубиком, получает урон = броску
            if( mageDice == firstResultOwner || mageDice == secondResultOwner)
            {
                yield return DamageToTarget(mageDice, mage); 
            }

            mage.mageIcon.HideInfoText();
        }

        owner.mageIcon.HideInfoText();

        yield break;
    }





    // Отборный
    public IEnumerator  Otbornui()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
        yield return OnDiceRoll(rolls);
        int resultDice = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9)
        {
            damage = 2;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }
        else
        {
            damage = 5;
            yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        }

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));

        yield break;
    }

    // Качковый
    // Недописанная карта
    // Нет добавления случайно карты к своему заклинанию
    public IEnumerator  Kachkovui()
    {
        int healHp = 2;
        yield return HealToTarget(healHp, owner);

        List<MageController> allMages = gm.aliveMages.ToList();
        allMages.Sort((mage1, mage2) => mage1.health.CompareTo(mage2.health)); // Нахождение самого хилого мага

        int minHp = allMages[0].health; // Сохранение его здоровья

        if(owner.health == minHp)
        {
            // Добавить случайную карту к своему заклинанию
            List<CardController> spellsInHand = owner.GetSpellsInHand();
            int randomCardIndex = random.Next(spellsInHand.Count);
            CardController card = spellsInHand[randomCardIndex];
            yield return owner.AppendToSpell(card);
        }
        
        yield break;
    }




    // От Сера Кладомота
    public IEnumerator  OtSeraKladomota()
    {
        int numberDice = 1; // кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить сокровище

        yield return gm.treasuresDeck.PassCardsTo(owner, 1); // Карта сокровища владельцу 
        
        foreach(MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);

            List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика 
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice = rolls.Sum();


            if (resultDice == necessaryResult)
                yield return gm.treasuresDeck.PassCardsTo(mage, 1);

            mage.mageIcon.Highlight(false);
        }

        yield break;
    }





    // От Мордоеда
    // Недописанная карта
    // Скопировать решение у карты дохлый колдун
    public IEnumerator  OtMordoeda()
    {
        owner.bonusDice += 1;

        owner.mageIcon.ShowInfoText($"+1");

        yield return new WaitForSeconds(1.5f);

        owner.mageIcon.HideInfoText();

        yield break;
    }

    // От Горячей штучки
    public IEnumerator  OtGoruacheiShtychki()
    {
        int damage = 3;
        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));

        yield break;
    }

    // От профессора Ахалая
    // Дописать шальную магию и проверку на наличие ее в заклинании
    public IEnumerator  OtProfessoraAxalaya()
    {
        int damage = 3;
        yield return DamageToTargets(damage, FindTargets(TargetType.RANDOM));

        if(owner.isWildMagicInSpell)
            yield return gm.treasuresDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // Жмураган
    public IEnumerator Jmyragan()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        int resultDice  = rolls.Sum();

        int damage = 0;
        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 6;}
        

        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        
        yield break;
    }




    // От Тай Тьфуна
    // Поменять список целей на список уже сходивших
    public IEnumerator OtTauiTfyna()
    {
        int damage = 3;

        yield return DamageToTargets(damage, gm.magesWhoExecutedSpell.FindAll(mage => mage != owner));
        
        yield break;
    }




    // От Поганого Мерлина
    public IEnumerator  OtPoganogoMerlina()
    {
        int damage = gm.aliveMages.Count;
        yield return DamageToTargets(damage, FindTargets(TargetType.HIGH_HP));
        
        yield return gm.deadsDeck.PassCardsTo(owner, 1);

        yield break;
    }

    // От Брадострела
    public IEnumerator  OtBradostrela()
    {
        List<CardController> deliveries = owner.GetCardsOfOrderInSpell(Order.DELIVERY);
        
        foreach (CardController card in deliveries)
        {
            yield return owner.OneSpellCardExecution(card);
        }

        yield break;
    }




    // От д-ра Конея Дуболома
    public IEnumerator  OtDraKorneyaDyboloma()
    {
        int healHp = 3; // Сколько жизней нужно вылечить
        yield return HealToTarget(healHp, owner); // Лечение себя

        Dictionary<MageController, int> magesAndRolls = new Dictionary<MageController, int>();

        int numberDice = 1;// кол-во кубиков
        int necessaryResult = 6; // Необходимый результат чтоб получить лечение
        
        // Каждый враг бросает кубик 
        foreach(MageController mage in AllEnemies())
        {
            mage.mageIcon.Highlight(true);
            List<int> rolls = RollDice(numberDice);
            yield return OnDiceRoll(rolls, showBonus: false);
            int resultDice  = rolls.Sum();
            magesAndRolls.Add(mage, resultDice);
            mage.mageIcon.ShowInfoText(resultDice.ToString());
            mage.mageIcon.Highlight(false);
        }

        // Проходим по каждому врагу и смотрим на его бросок
        foreach (var mageRoll in magesAndRolls)
        {
            MageController mage = mageRoll.Key;
            if( mageRoll.Value == necessaryResult )// Если враг выкинул 6
            {
                yield return HealToTarget(healHp, mage);// Лечение врага
            }
            mage.mageIcon.HideInfoText();
        }
        
        yield break;
    }

    // Фонтан Молодости
    public IEnumerator FontanMolodosti()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        int resultDice  = rolls.Sum();
        
        int healHp = 0;
        if      (resultDice <= 4){healHp = 0;}
        else if (resultDice <= 9){healHp = 2;}
        else                     {healHp = 4;}

        yield return HealToTarget(healHp, owner); // Лечение владельца
        yield break;
    }




    // Самовыпил
    public IEnumerator Samovipil()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        int resultDice  = rolls.Sum();

        int damageSelf = 1;

        int damage = 0;
        if      (resultDice <= 4){damage = 2;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 5;}

        yield return DamageToTarget(damageSelf, owner);
        yield return DamageToTarget(damage, IsDeadTargetLeftOrRight(owner.rightMage, POSITION_RIGHT)); //Правый маг
        
        yield break;
    }

    // Мясной фарш
    public IEnumerator MyasnouiFarsh()
    {
        List<int> rolls = RollDice(NumberDice(sign)); // Бросок кубика
        yield return OnDiceRoll(rolls);
        int resultDice  = rolls.Sum();

        int damage = 0;

        // Поиск врагов с большим здоровьем чем у владельца
        List<MageController> listTargets = gm.aliveMages.FindAll(mage => mage.health > owner.health);

        if      (resultDice <= 4){damage = 1;}
        else if (resultDice <= 9){damage = 3;}
        else                     {damage = 4;}

        yield return DamageToTargets(damage, listTargets);
        
        yield break;
    }



}
