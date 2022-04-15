using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance;                   // сингелтон объект
    public  static GameManager  instance => _instance;             

    [SerializeField] private DeckController _spellsDeck;    // колода заклинаний
    [SerializeField] private DeckController _treasuresDeck; // колода сокровищ
    [SerializeField] private DeckController _deadsDeck;     // колода мертвых магов

    [SerializeField] private List<Mage> _mages    = new List<Mage>();             // список всех доступных ScriptableObject магов
                                                            
    private List<MageController> _mageControllers = new List<MageController>();  // список всех созданных магов (контроллеров магов) в игре 
    
    private int _playerMageControllerIndex;                 // индекс мага игрока в списке магов
    private MageController _playerMageController;           // сам маг игрока

    private GameState _state;                               // настоящее состояние игры



    public GameObject magePrefab;           // шаблон мага в игре
    public GameObject cardPrefab;           // шаблон карты
    public GameObject fieldCenter;          // GameObject поля игры
    
    public Mage playerMage;                 // выбранный игроком ScriptableObject мага
    
    public  List<MageController>  mageControllers => _mageControllers;
    public  MageController   playerMageController => _playerMageController;
    public  int playerMageControllerIndex => _playerMageControllerIndex;
    public  List<Mage> mages => _mages;


    void Awake()
    {
        CreateSingleton();
        SetupGame();
    }

    void Start()
    {
        SetState(new BeginGameState(this));
    }

    // установить новое состояние игры
    public void SetState(GameState state)
    {
        _state = state;
        StartCoroutine(_state.Start());
    }

    // начальная загрузка игры
    void SetupGame()
    {
        SetupMages(); 
    }

    // логика сингелтона
    void CreateSingleton()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }


    // загрузить нужных магов и сохранить мага игрока
    void SetupMages()       
    {
        int index = 0;      
        foreach (Mage mage in _mages)
        {
            GameObject newMage = Instantiate(magePrefab, fieldCenter.transform);
            MageController newMageController = newMage.GetComponent<MageController>();
            newMageController.mage = mage;
            _mageControllers.Add(newMageController);
            if (mage == playerMage) 
            {
                _playerMageControllerIndex = index;
                _playerMageController = newMageController;
                newMage.tag = "Player";
            }
            index++;
        }
    }

    // каждому магу взять карты до 8 штук
    void DrawCards()        
    {
        List<MageController> magesToDrawCard = new List<MageController>(_mageControllers);
        Predicate<MageController> predicate = mage => mage.hand.spellsCount >= 8 && !mage.isDead;
        while (_spellsDeck.Count > 0 && magesToDrawCard.Count > 0)
        {
            magesToDrawCard.RemoveAll(predicate);
            foreach(MageController mage in magesToDrawCard)
            {
                mage.TakeCard(_spellsDeck);
            }
        }
    }

}
