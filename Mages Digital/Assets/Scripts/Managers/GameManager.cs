using Random = System.Random;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{


    // синглтон объект
    private static GameManager _instance;
    public  static GameManager  instance => _instance;


    public Random random = new Random();


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    [Header("Префабы")]
    [SerializeField] GameObject _spellCardPrefab;    // префаб карты
    [SerializeField] GameObject _bonusCardPrefab;
    [SerializeField] GameObject _medalPrefab;   // префаб медали недобитого колдуна
    [SerializeField] GameObject _mageOrderIcon; // иконка для отображения хода мага

    [Header("Колоды")]
    [SerializeField] DeckController _spellsDeck;     // колода заклинаний
    [SerializeField] DeckController _treasuresDeck;  // колода сокровищ
    [SerializeField] DeckController _deadsDeck;      // колода недобитых магов

    [Header("Расположения")]
    [SerializeField] Transform _fieldCenter;         // центр поля
    [SerializeField] Transform _spellLocation;       // место расположения карт заклинаний
    [SerializeField] Transform _sourceLocation;      // место расположения заводилы
    [SerializeField] Transform _qualityLocation;     // место расположения наворота
    [SerializeField] Transform _deliveryLocation;    // место расположения прихода
    [SerializeField] Transform _spellGroupLocation;  // место расположения группы заклинаний
    [SerializeField] Transform _magesOrderLocation;  // место расположения порядка хода магов
    

    [Header("Состояние игры")]
    [SerializeField] GameState _prevGameState;                      // предыдущее состояние игры
    [SerializeField] GameState _gameState = GameState.ROUND_START;  // настоящее состояние игры
    
    [Header("Маги")]
    [SerializeField] Mage _playerMage;
    [SerializeField] MageController _player;
    [SerializeField] List<MageController> _mages = new List<MageController>();      // все маги в игре (вообще все, считая мертвых)
    [SerializeField] List<MageController> _magesOrder = new List<MageController>(); // порядок хода (тут только живые маги)


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // локации расположения спелов при создании заклинания
    List<Transform> _spellLocations = new List<Transform>();
    List<SpellLocationController> _spellLocationControllers = new List<SpellLocationController>();

    List<MageOrderIconController> _magesOrderIcons = new List<MageOrderIconController>();


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // параметры анимации передвижения медали недобитого мага
    float   _medalStartMovingTime  = 2.5f;
    float   _medalToMageMovingTime = 1.0f;
    Vector3 _medalStartPosition    = new Vector3(0.0f, 10.0f, 0.0f);
    Vector3 _medalStartDestination = new Vector3(0.0f, 1.0f, 0.0f);


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // состояния игры
    public enum GameState
    {
        TOURNAMENT_START,
        
        ROUND_START,
        
        SPELL_CREATION,
        CHOOSING,
        SPELL_EXECUTION,
        
        ROUND_END,

        TOURNAMENT_END,

        GAME_END,
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public MageController           player => _player;
    public List<MageController>      mages => _mages;              
    public List<MageController> magesOrder => _magesOrder;
    public DeckController       spellsDeck => _spellsDeck;
    public DeckController        deadsDeck => _deadsDeck;
    public DeckController    treasuresDeck => _treasuresDeck;
    public GameState             gameState => _gameState;
    public GameState         prevGameState => _prevGameState;
    public GameObject      spellCardPrefab => _spellCardPrefab;
    public GameObject      bonusCardPrefab => _bonusCardPrefab;
    public Transform           fieldCenter => _fieldCenter;
    public Transform         spellLocation => _spellLocation;
    public Transform        sourceLocation => _sourceLocation;
    public Transform       qualityLocation => _qualityLocation;
    public Transform      deliveryLocation => _deliveryLocation;
    public Transform    spellGroupLocation => _spellGroupLocation;
    
    public List<Transform> spellLocations => _spellLocations;
    public List<SpellLocationController> spellLocationControllers => _spellLocationControllers;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
    public List<MageController> magesWhoExecutedSpell => _mages.FindAll(mage => mage.spellsAreExecuted);  // походившие маги
    public List<MageController> aliveMages => _mages.FindAll(mage => !mage.isDead);   // живые маги
    public List<MageController> deadMages  => _mages.FindAll(mage => mage.isDead);    // мертвые маги
    public List<MageController> magesWithDeads => _mages.FindAll(mage => mage.deads.Count > 0); // маги с картами дохлых колдунов
    public MageController     onlyOneAlive => aliveMages[0];   // первый выживший маг

    public bool areAllDead            => aliveMages.Count == 0;  // мертвы ли все маги
    public bool isAliveOnlyOne        => aliveMages.Count == 1;  // жив ли лишь один маг
    public bool isGameEnd             => _gameState == GameState.GAME_END;
    public bool isRoundEnd            => _gameState == GameState.ROUND_END;
    public bool isTournamentEnd       => _gameState == GameState.TOURNAMENT_END;
    public bool isSpellCreationState  => _gameState == GameState.SPELL_CREATION;
    public bool isSpellExecutionState => _gameState == GameState.SPELL_EXECUTION;
    public bool isChoosingState       => _gameState == GameState.CHOOSING;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        _spellLocations.Add(_sourceLocation);
        _spellLocations.Add(_qualityLocation);
        _spellLocations.Add(_deliveryLocation);
        foreach (Transform spellLocation in _spellLocations)
            _spellLocationControllers.Add(spellLocation.gameObject.GetComponent<SpellLocationController>());
        
    }


    void Start()
    {
        _prevGameState = _gameState;

        GameObject[] mageObjects = GameObject.FindGameObjectsWithTag("Mage");
        foreach (GameObject mageObject in mageObjects)
        {
            MageController mage = mageObject.GetComponent<MageController>();
            _mages.Add(mage);
            if (mage.mage == _playerMage)
                _player = mage;
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public IEnumerator TournamentStart()
    {
        SetNewState(GameState.TOURNAMENT_START);

        // выполняются карты дохлых колдунов
        List<MageController> currentMagesWithDeads = magesWithDeads.ToList();
        foreach (MageController mageWithDeads in currentMagesWithDeads)
            yield return mageWithDeads.ExecuteDeads();
        
    }


    public IEnumerator RoundStart()
    {
        SetNewState(GameState.ROUND_START);

        _mages.ForEach(mage => mage.OnRoundStart());

        // мертвые маги берут одну карту дохлого мага
        yield return PassCardsToMages(deadMages.ToList(), deadsDeck, nCards: 1, autoNCardsToDraw: false);

        // раздать карты
        yield return PassCardsToMages(aliveMages.ToList(), spellsDeck, autoNCardsToDraw: true);

    }

    // этап создания заклинаний
    public IEnumerator SpellCreation()
    {
        SetNewState(GameState.SPELL_CREATION);
        
        _spellLocationControllers.ForEach(x => x.FadeInOutline());

        // XXX too slow (c)
        List<MageController> currentAliveMages = aliveMages.ToList();
        yield return new WaitUntil(() => currentAliveMages.FindAll(x => x.readyToExecute).Count == currentAliveMages.Count);

        _spellLocationControllers.ForEach(x => x.FadeOutOutline());
    }

    // этап выполнения заклинаний
    public IEnumerator SpellExecution()
    {
        SetNewState(GameState.SPELL_EXECUTION);
        
        ResetMagesOrder();

        yield return ShowOrder();

        for (int i = 0; i < _magesOrder.Count; i++)
        {
            MageController mage = _magesOrder[i];
            if (!mage.isDead && mage.nCardsInSpell > 0)
            {
                _magesOrderIcons.ForEach(icon => icon.Highlight(false));
                _magesOrderIcons[i].Highlight(true);
                yield return mage.ExecuteSpells();
            }
        }

        _magesOrderIcons.ForEach(icon => icon.FlyOut());

        SetNewState(GameState.ROUND_END);

        yield break;
    }

    // этап конца раунда (когда все выполнили свои заклинания)
    public IEnumerator RoundEnd()
    {
        foreach (MageController mage in aliveMages)
            mage.DropSpell();

        if (isAliveOnlyOne || areAllDead)
            SetNewState(GameState.TOURNAMENT_END);
        else
            SetNewState(GameState.ROUND_START);
        
        yield break;
    }

    // этап конца турнамента (когда остался лишь один выживший или погибли все)
    public IEnumerator TournamentEnd()
    {
        if (!areAllDead)
        {
            MageController tournamentWinner = onlyOneAlive;

            yield return PassMedalToWinner(tournamentWinner);

            tournamentWinner.OnTournamentWon();
            
            if (tournamentWinner.isGameWinner)
                SetNewState(GameState.GAME_END);
            else
                SetNewState(GameState.TOURNAMENT_START);
            
            if (isGameEnd)
                print($"Winner: {tournamentWinner.mage.mageName}");
        }
        else
        {
            SetNewState(GameState.ROUND_START);
        }

        foreach (MageController mage in _mages)
            mage.ResetMage();

        yield break;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    IEnumerator ShowOrder()
    {
        _magesOrderIcons.Clear();

        int magesInOrder = _magesOrder.Count;

        float deltaX = 1.5f;
        float y = 1.0f;
        float z = 5.5f;
        float moveTime  = 1.0f;
        float deltaTime = 1.0f;

        float x = -(deltaX * (magesInOrder-1)) / 2;
        

        for (int i = 0; i < magesInOrder; i++)
        {
            MageController mage = _magesOrder[i];
            
            mage.mageIcon.ShowInitiative();
            yield return new WaitForSeconds(0.5f);

            GameObject orderIcon = Instantiate(_mageOrderIcon, _magesOrderLocation);

            MageOrderIconController orderIconController = orderIcon.GetComponent<MageOrderIconController>();
            orderIconController.SetIcon(mage.mage.icon);
            _magesOrderIcons.Add(orderIconController);

            orderIcon.transform.position = mage.mageIcon.transform.position;

            iTween.MoveTo(orderIcon, iTween.Hash("x", x, "y", y, "z", z, "time", moveTime));

            yield return new WaitForSeconds(moveTime + deltaTime);

            x += deltaX;
        }

        yield return new WaitForSeconds(1.0f);

        _magesOrder.ForEach(mage => mage.mageIcon.HideInitiative());
    }


    // передать медаль победителю турнира
    IEnumerator PassMedalToWinner(MageController winner)
    {
        GameObject medal = Instantiate(_medalPrefab, _fieldCenter);
        medal.transform.position = _medalStartPosition;

        iTween.MoveTo(medal, iTween.Hash("position", _medalStartDestination, "time", _medalStartMovingTime));
        yield return new WaitForSeconds(_medalStartMovingTime);
        
        iTween.MoveTo(medal, iTween.Hash("position", winner.mageIcon.transform.position, "time", _medalToMageMovingTime, "easetype", iTween.EaseType.easeInExpo));
        yield return new WaitForSeconds(_medalToMageMovingTime);
        
        Destroy(medal);
    }

    IEnumerator PassCardsToMages(List<MageController> mages, DeckController deck, int nCards = 0, bool autoNCardsToDraw = true)
    {
        int amount = mages.Count;
        if (amount > 0)
        {
            int n = 0;
            foreach (MageController mage in mages.GetRange(0, amount-1))
            {
                n = autoNCardsToDraw ? mage.nCardsToDraw : nCards;
                yield return deck.PassCardsTo(mage, n, autoHide: false);
            }

            MageController lastMage = mages[amount - 1];
            n = autoNCardsToDraw ? lastMage.nCardsToDraw : nCards;
            yield return deck.PassCardsTo(lastMage, n);
        }
    }

    // пересчитать очередь хода магов
    void ResetMagesOrder()
    {
        _magesOrder.Clear();

        List<MageController> currentAliveMages    = aliveMages.ToList();
        List<MageController> oneCardSpellsMages   = GetSortedNCardsSpellsMages(currentAliveMages, 1);
        List<MageController> twoCardSpellsMages   = GetSortedNCardsSpellsMages(currentAliveMages, 2);
        List<MageController> threeCardSpellsMages = GetSortedNCardsSpellsMages(currentAliveMages, 3);

        _magesOrder.AddRange(oneCardSpellsMages);
        _magesOrder.AddRange(twoCardSpellsMages);
        _magesOrder.AddRange(threeCardSpellsMages);

        int currentNMages = _magesOrder.Count;
        for (int i = 0; i < currentNMages; i++)
        {
            MageController mage = _magesOrder[i];


            if (mage.nonNullSpell.Count(card => card.ownerGoesFirst) > 0)
            {
                mage.OnChangeOrder();
                _magesOrder.RemoveAt(i);
                _magesOrder.Insert(0, mage);
            }
        }

    }

    // вернуть магов с количеством карт заклинаний в спеле равным n
    List<MageController> GetSortedNCardsSpellsMages(List<MageController> mages, int n)
    {
        List<MageController> nCardSpellsMages = mages.FindAll(mage => mage.nCardsInSpell == n).OrderBy(mage => random.Next()).ToList();
        nCardSpellsMages.Sort((mage1, mage2) => -mage1.spellInitiative.CompareTo(mage2.spellInitiative));
        return nCardSpellsMages;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // установить новое состояние
    void SetNewState(GameState newState)
    {
        _prevGameState = _gameState;
        _gameState = newState;
    }

    // законичить состояние выбора игрока
    public void StopChoosing()
    {
        ReturnToPrevState();
    }

    // установить состояние выбора игроком
    public void SetChoosingState()
    {
        SetNewState(GameState.CHOOSING);
    }

    // вернуться к предыдущему состоянию
    public void ReturnToPrevState()
    {
        SetNewState(_prevGameState);
    }


}
