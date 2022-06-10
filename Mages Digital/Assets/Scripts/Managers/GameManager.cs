using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{


    // синглтон объект
    private static GameManager _instance;
    public  static GameManager  instance => _instance;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    [Header("Префабы")]
    [SerializeField] GameObject _cardPrefab;   // префаб карты
    [SerializeField] GameObject _medalPrefab;  // префаб медали недобитого колдуна

    [Header("Колоды")]
    [SerializeField] DeckController _spellsDeck;     // колода заклинаний
    [SerializeField] DeckController _treasuresDeck;  // колода сокровищ
    [SerializeField] DeckController _deadsDeck;      // колода недобитых магов

    [Header("Расположения")]
    [SerializeField] private Transform _fieldCenter;         // центр поля
    [SerializeField] private Transform _spellLocation;       // место расположения карт заклинаний
    [SerializeField] private Transform _sourceLocation;      // место расположения заводилы
    [SerializeField] private Transform _qualityLocation;     // место расположения наворота
    [SerializeField] private Transform _deliveryLocation;    // место расположения прихода
    [SerializeField] private Transform _spellGroupLocation;  // место расположения группы заклинаний

    [Header("Состояние игры")]
    [SerializeField] GameState _prevGameState;                      // предыдущее состояние игры
    [SerializeField] GameState _gameState = GameState.ROUND_START;  // настоящее состояние игры
    
    [Header("Маги")]
    [SerializeField] List<MageController> _mages = new List<MageController>();      // все маги в игре (вообще все, считая мертвых)
    [SerializeField] List<MageController> _magesOrder = new List<MageController>(); // порядок хода (тут только живые маги)


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // локации расположения спелов при создании заклинания
    List<Transform> _spellLocations = new List<Transform>();
    List<SpellLocationController> _spellLocationControllers = new List<SpellLocationController>();


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
        ROUND_START,
        SPELL_CREATION,
        SPELL_EXECUTION,
        ROUND_END,
        TOURNAMENT_END,
        GAME_END,
        CHOOSING
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public List<MageController>      mages => _mages;              
    public List<MageController> magesOrder => _magesOrder;
    public DeckController       spellsDeck => _spellsDeck;
    public DeckController        deadsDeck => _deadsDeck;
    public DeckController    treasuresDeck => _treasuresDeck;
    public GameState             gameState => _gameState;
    public GameState         prevGameState => _prevGameState;
    public GameObject           cardPrefab => _cardPrefab;
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
    public MageController     onlyOneAlive => aliveMages[0];   // первый выживший маг

    public bool areAllDead           => aliveMages.Count == 0;  // мертвы ли все маги
    public bool isAliveOnlyOne       => aliveMages.Count == 1;  // жив ли лишь один маг
    public bool isGameEnd            => _gameState == GameState.GAME_END;
    public bool isRoundEnd           => _gameState == GameState.ROUND_END;
    public bool isTournamentEnd      => _gameState == GameState.TOURNAMENT_END;
    public bool isSpellCreationState => _gameState == GameState.SPELL_CREATION;
    public bool isChoosingState      => _gameState == GameState.CHOOSING;


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
            _mages.Add(mageObject.GetComponent<MageController>());
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
                SetNewState(GameState.ROUND_START);
            
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

    // пересчитать очередь хода магов
    void ResetMagesOrder()
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
    List<MageController> GetSortedNCardsSpellsMages(List<MageController> mages, int n)
    {
        List<MageController> nCardSpellsMages = mages.FindAll(mage => mage.nCardsInSpell == n);
        nCardSpellsMages.Sort((mage1, mage2) => mage1.spellInitiative.CompareTo(mage2.spellInitiative));
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
