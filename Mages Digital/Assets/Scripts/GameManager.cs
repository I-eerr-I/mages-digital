using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        ROUND_START,
        SPELL_CREATION
    }

    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    private GameState _gameState = GameState.ROUND_START;

    private List<MageController> _mages = new List<MageController>();

    private List<SpellLocationController> _spellLocationControllers = new List<SpellLocationController>();


    public Transform  fieldCenter;


    [Header("Prefabs")]
    public GameObject cardPrefab;

    [Header("Decks")]
    public DeckController spellsDeck;
    public DeckController treasuresDeck;
    public DeckController deadsDeck;

    [Header("Spell Creation")]
    public Transform sourceLocation;
    public Transform qualityLocation;
    public Transform deliveryLocation;

    public GameState gameState => _gameState;
    public List<MageController> mages => _mages;
    public List<SpellLocationController> spellLocationControllers => _spellLocationControllers;

    public bool isSpellCreationState => _gameState == GameState.SPELL_CREATION;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        _spellLocationControllers.Add(sourceLocation.gameObject.GetComponent<SpellLocationController>());
        _spellLocationControllers.Add(qualityLocation.gameObject.GetComponent<SpellLocationController>());
        _spellLocationControllers.Add(deliveryLocation.gameObject.GetComponent<SpellLocationController>());
    }

    void Start()
    {
        GameObject[] mageObjects = GameObject.FindGameObjectsWithTag("Mage");
        foreach (GameObject mageObject in mageObjects)
        {
            _mages.Add(mageObject.GetComponent<MageController>());
        }
    }

    public IEnumerator CardDraw()
    {
        foreach (MageController mage in _mages)
        {
            yield return spellsDeck.PassCardsTo(mage, 8);
        }
    }

    public IEnumerator SpellCreationStart()
    {
        _gameState = GameState.SPELL_CREATION;
        foreach (SpellLocationController slc in _spellLocationControllers)
            slc.FadeIn();
        yield break;
    }
}
