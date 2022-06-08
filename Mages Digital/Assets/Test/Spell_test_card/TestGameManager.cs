using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : MonoBehaviour
{
    public enum GameState
    {
        ROUND_START,
        SPELL_CREATION,
        SPELL_EXECUTION,
        CHOOSING
    }

    private static TestGameManager _instance;
    public  static TestGameManager  instance => _instance;

    private GameState _prevGameState;
    private GameState _gameState = GameState.ROUND_START;

    private List<TestMageController> _mages = new List<TestMageController>();

    private List<Transform> _spellLocations = new List<Transform>();
    private List<SpellLocationController> _spellLocationControllers = new List<SpellLocationController>();


    public Transform  fieldCenter;


    [Header("Prefabs")]
    public GameObject cardPrefab;

    [Header("Decks")]
    public TestDeckController spellsDeck;
    public TestDeckController treasuresDeck;
    public TestDeckController deadsDeck;

    [Header("Spell Creation")]
    public Transform sourceLocation;
    public Transform qualityLocation;
    public Transform deliveryLocation;

    public GameState gameState => _gameState;
    public List<TestMageController> mages => _mages;
    public List<Transform> spellLocations => _spellLocations;
    public List<SpellLocationController> spellLocationControllers => _spellLocationControllers;

    [SerializeField] private List<MageController> _magesOrder = new List<MageController>();//CHANGE


    public List<MageController> magesOrder => _magesOrder; //CHANGE

    public bool isSpellCreationState => _gameState == GameState.SPELL_CREATION;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        _spellLocations.Add(sourceLocation);
        _spellLocations.Add(qualityLocation);
        _spellLocations.Add(deliveryLocation);
        // foreach (Transform spellLocation in _spellLocations)
        //     _spellLocationControllers.Add(spellLocation.gameObject.GetComponent<SpellLocationController>());
        
    }

    void Start()
    {
        _prevGameState = _gameState;

        GameObject[] mageObjects = GameObject.FindGameObjectsWithTag("Mage");
        foreach (GameObject mageObject in mageObjects)
        {
            _mages.Add(mageObject.GetComponent<TestMageController>());
        }
    }

    public IEnumerator CardDraw()
    {
        foreach (TestMageController mage in _mages)
        {
            yield return spellsDeck.PassCardsTo(mage, 8);
        }
    }

    public IEnumerator SpellCreation()
    {
        SetNewState(GameState.SPELL_CREATION);
        _spellLocationControllers.ForEach(x => x.FadeInOutline());

        // XXX to slow (c)
        yield return new WaitUntil(() => _mages.FindAll(x => x.owner.readyToExecute).Count == _mages.FindAll(x => !x.isDead).Count);
    }

    public IEnumerator SpellExecution()
    {
        SetNewState(GameState.SPELL_EXECUTION);
        _spellLocationControllers.ForEach(x => x.FadeOutOutline());

        yield break;
    }

    public void SetChoosingState()
    {
        SetNewState(GameState.CHOOSING);
    }

    public void ReturnToPrevState()
    {
        SetNewState(_prevGameState);
    }

    void SetNewState(GameState newState)
    {
        _prevGameState = _gameState;
        _gameState = newState;
    }
}
