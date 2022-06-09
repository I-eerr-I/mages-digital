using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // состояния игры
    public enum GameState
    {
        ROUND_START,
        SPELL_CREATION,
        SPELL_EXECUTION,
        ROUND_END,
        TOURNAMENT_END,
        GAME_END,
        CHOOSING
    }

    // синглтон объект
    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    // локации расположения спелов при создании заклинания
    private List<Transform> _spellLocations = new List<Transform>();
    private List<SpellLocationController> _spellLocationControllers = new List<SpellLocationController>();

    [Header("Состояние игры")]
    [SerializeField] private GameState _prevGameState;                      // предыдущее состояние игры
    [SerializeField] private GameState _gameState = GameState.ROUND_START;  // настоящее состояние игры
    

    [Header("Маги")]
    [SerializeField] private List<MageController> _mages = new List<MageController>(); // все маги в игре (вообще все, считая мертвых)
    
    [Header("Порядок хода магов")]
    [SerializeField] private List<MageController> _magesOrder = new List<MageController>(); // порядок хода (тут только живые маги)

    [Header("Префабы")]
    [SerializeField] private GameObject _cardPrefab; // префаб карты

    [Header("Колоды")]
    [SerializeField] private DeckController _spellsDeck;     // колода заклинаний
    [SerializeField] private DeckController _treasuresDeck;  // колода сокровищ
    [SerializeField] private DeckController _deadsDeck;      // колода недобитых магов


    [Header("Расположения")]
    [SerializeField] private Transform _fieldCenter;         // центр поля
    [SerializeField] private Transform _spellLocation;       // место расположения карт заклинаний
    [SerializeField] private Transform _sourceLocation;      // место расположения заводилы
    [SerializeField] private Transform _qualityLocation;     // место расположения наворота
    [SerializeField] private Transform _deliveryLocation;    // место расположения прихода
    [SerializeField] private Transform _spellGroupLocation;  // место расположения группы заклинаний



    public List<MageController> mages => _mages;              
    public List<Transform> spellLocations => _spellLocations;
    public List<MageController> magesOrder => _magesOrder;
    public List<SpellLocationController> spellLocationControllers => _spellLocationControllers;
    
    public GameState gameState => _gameState;
    public GameState prevGameState => _prevGameState;
    
    public GameObject cardPrefab => _cardPrefab;
    
    public DeckController spellsDeck => _spellsDeck;
    public DeckController treasuresDeck => _treasuresDeck;
    public DeckController deadsDeck => _deadsDeck;
    
    public Transform fieldCenter => _fieldCenter;
    public Transform spellLocation => _spellLocation;
    public Transform sourceLocation => _sourceLocation;
    public Transform qualityLocation => _qualityLocation;
    public Transform deliveryLocation => _deliveryLocation;
    public Transform spellGroupLocation => _spellGroupLocation;

    public List<MageController> aliveMages => _mages.FindAll(mage => !mage.isDead); // живые маги
    public List<MageController> magesWhoExecutedSpell => _mages.FindAll(mage => mage.spellsAreExecuted);
    public MageController onlyOneAlive     => aliveMages[0];
    public bool areAllDead           => aliveMages.Count == 0;
    public bool isAliveOnlyOne       => aliveMages.Count == 1;
    public bool isGameEnd            => _gameState == GameState.GAME_END;
    public bool isRoundEnd           => _gameState == GameState.ROUND_END;
    public bool isTournamentEnd      => _gameState == GameState.TOURNAMENT_END;
    public bool isSpellCreationState => _gameState == GameState.SPELL_CREATION;
    public bool isChoosingState      => _gameState == GameState.CHOOSING;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        _spellLocations.Add(sourceLocation);
        _spellLocations.Add(qualityLocation);
        _spellLocations.Add(deliveryLocation);
        foreach (Transform spellLocation in _spellLocations)
            _spellLocationControllers.Add(spellLocation.gameObject.GetComponent<SpellLocationController>());
        
    }

    void Start()
    {
        _prevGameState = _gameState;

        GameObject[] mageObjects = GameObject.FindGameObjectsWithTag("Mage");
        foreach (GameObject mageObject in mageObjects)
        {
            _mages.Add(mageObject.GetComponent<MageController>());
        }
    }

    // раздать нужно количество карт магам
    public IEnumerator CardDraw()
    {
        foreach (MageController mage in aliveMages)
        {
            yield return spellsDeck.PassCardsTo(mage, mage.nCardsToDraw);
        }
    }

    // этап создания заклинаний
    public IEnumerator SpellCreation()
    {
        SetNewState(GameState.SPELL_CREATION);
        _spellLocationControllers.ForEach(x => x.FadeInOutline());

        // XXX too slow (c)
        yield return new WaitUntil(() => aliveMages.FindAll(x => x.readyToExecute).Count == aliveMages.Count);

        _spellLocationControllers.ForEach(x => x.FadeOutOutline());
    }

    // этап выполнения заклинаний
    public IEnumerator SpellExecution()
    {
        SetNewState(GameState.SPELL_EXECUTION);
        
        ResetMagesOrder();

        // ANIMATE
        // показать инициативу заклинаний магов
        print("SHOWING INITIATIVES");
        _magesOrder.ForEach(mage => print($"{mage.mage.mageName} : {mage.spellInitiative}"));
        yield return new WaitForSeconds(2.0f);
        // ANIMATE

        foreach(MageController mage in _magesOrder)
        {
            if (!mage.isDead)
                yield return mage.ExecuteSpells();
        }

        SetNewState(GameState.ROUND_END);

        yield break;
    }

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

    public IEnumerator TournamentEnd()
    {
        if (!areAllDead)
        {
            MageController roundWinner = onlyOneAlive;
            roundWinner.TournamentWon();
            
            if (roundWinner.isGameWinner)
                SetNewState(GameState.GAME_END);
            else
                SetNewState(GameState.ROUND_START);
            
            if (isGameEnd)
                print($"Winner: {roundWinner.mage.mageName}");
        }
        else
        {
            SetNewState(GameState.ROUND_START);
        }

        foreach (MageController mage in _mages)
            mage.ResetMage();

        yield break;
    }

    // пересчитать очередь хода магов
    public void ResetMagesOrder()
    {
        _magesOrder.Clear();

        List<MageController> oneCardSpellsMages   = GetSortedNCardsSpellsMages(aliveMages, 1);
        List<MageController> twoCardSpellsMages   = GetSortedNCardsSpellsMages(aliveMages, 2);
        List<MageController> threeCardSpellsMages = GetSortedNCardsSpellsMages(aliveMages, 3);

        _magesOrder.AddRange(oneCardSpellsMages);
        _magesOrder.AddRange(twoCardSpellsMages);
        _magesOrder.AddRange(threeCardSpellsMages);
    }

    // вернуть магов с количеством карт заклинаний в спеле равным n
    public List<MageController> GetSortedNCardsSpellsMages(List<MageController> mages, int n)
    {
        List<MageController> nCardSpellsMages = mages.FindAll(mage => mage.nCardsInSpell == n);
        nCardSpellsMages.Sort((mage1, mage2) => mage1.spellInitiative.CompareTo(mage2.spellInitiative));
        return nCardSpellsMages;
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

    // установить новое состояние
    void SetNewState(GameState newState)
    {
        _prevGameState = _gameState;
        _gameState = newState;
    }


}
