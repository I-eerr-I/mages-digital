// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class GameManager : MonoBehaviour
// {

//     // private static GameManager _instance;                   // сингелтон объект
//     // public  static GameManager  instance => _instance;      


//     // [Header("Rounds")]
//     // [SerializeField] private int _roundNumber = 0;


//     // [Header("Decks")]
//     // [SerializeField] private DeckController _spellsDeck;    // колода заклинаний
//     // [SerializeField] private DeckController _treasuresDeck; // колода сокровищ
//     // [SerializeField] private DeckController _deadsDeck;     // колода мертвых магов

    
//     // [Header("Mages")]
//     // [SerializeField] private List<Mage> _mages      = new List<Mage>();     // список всех доступных ScriptableObject магов
//     // [SerializeField] private List<Mage> _enemyMages = new List<Mage>();     // список вражеских магов
                                                            
//     // private List<MageController>  _mageControllers  = new List<MageController>();  // список всех созданных магов (контроллеров магов) в игре 
//     // private List<EnemyController> _enemyControllers = new List<EnemyController>();
    
//     // private PlayerController _playerController;           // сам маг игрока

//     // private GameState _state;                               // настоящее состояние игры


//     // public Mage playerMage;                 // выбранный игроком ScriptableObject мага

//     // [Header("Prefabs and GameObjects")]
//     // public GameObject playerPrefab;
//     // public GameObject enemyPrefab;          // шаблон врага в игре
//     // public GameObject cardPrefab;           // шаблон карты
//     // public GameObject fieldCenter;          // GameObject поля игры
//     // public Transform  playerHandLocation;   // локация руки игрока
    
    
//     // public List<Mage> mages      => _mages;
//     // public List<Mage> enemyMages => _enemyMages;
//     // public DeckController spellsDeck    =>    _spellsDeck;
//     // public DeckController treasuresDeck => _treasuresDeck;
//     // public DeckController deadsDeck     =>     _deadsDeck;
//     // public List<MageController>  mageControllers  => _mageControllers;
//     // public List<EnemyController> enemyControllers => _enemyControllers;
//     // public PlayerController  playerController     => _playerController;
//     // public int roundNumber => _roundNumber;


//     // void Awake()
//     // {
//     //     CreateSingleton();
//     // }

//     // void Start()
//     // {
//     //     SetState(new TournamentStartState());
//     // }

//     // // установить новое состояние игры
//     // public void SetState(GameState state)
//     // {
//     //     _state = state;
//     //     StartCoroutine(_state.Start());
//     // }

//     // public void TransitionToState(GameState state)
//     // {
//     //     if (_state != null) StartCoroutine(_state.End(state));
//     // }

//     // public void SetPlayerMage(Mage mage)
//     // {
//     //     playerMage = mage;
//     //     _enemyMages = new List<Mage>(_mages.FindAll((mage) => mage != playerMage));
//     // }

//     // // начальная загрузка игры
//     // public void SetupGame()
//     // {
//     //     SetupMages(); 
//     // }

//     // public void SetupNewRound()
//     // {
//     //     _roundNumber++;
//     //     spellsDeck.Shuffle();
//     //     treasuresDeck.Shuffle();
//     //     deadsDeck.Shuffle();
//     // }

//     // // логика сингелтона
//     // void CreateSingleton()
//     // {
//     //     if (_instance == null) _instance = this;
//     //     else Destroy(gameObject);
//     // }


//     // // загрузить нужных магов и сохранить мага игрока
//     // void SetupMages()       
//     // {
//     //     int index = 1;
//     //     _playerController = CreateMage<PlayerController>(playerPrefab, playerMage, 0.0f);
//     //     _mageControllers.Add(_playerController);
//     //     _enemyMages.ForEach((Mage mage) => 
//     //     {
//     //             EnemyController enemyMage = CreateMage<EnemyController>(enemyPrefab, mage, index * 360.0f / (_enemyMages.Count + 1));
//     //             MageController previousMage = _mageControllers[index-1];
//     //             enemyMage.rightMage   = previousMage;
//     //             previousMage.leftMage = enemyMage;
//     //             _mageControllers.Add(enemyMage);
//     //             _enemyControllers.Add(enemyMage);
//     //             index++;
//     //     });
//     //     _mageControllers[_mageControllers.Count - 1].leftMage = _playerController;
//     // }

//     // T CreateMage<T>(GameObject prefab, Mage mage, float angle) where T : MageController
//     // {
//     //     GameObject newObject = Instantiate(prefab, fieldCenter.transform);
//     //     PositionMage(newObject, angle);
//     //     T mageController     = newObject.GetComponent<T>();
//     //     mageController.mage  = mage;
//     //     return mageController;
//     // }

//     // // поставить мага в нужное место относительно центра поля
//     // void PositionMage(GameObject mage, float angle)
//     // {
//     //     mage.transform.RotateAround(fieldCenter.transform.position, Vector3.forward, angle);
//     //     mage.transform.eulerAngles = new Vector3(0, 0, 0);
//     // }

//     // // каждому магу взять карты до 8 штук
//     // // void DrawCards()        
//     // // {
//     // //     List<MageController> magesToDrawCard = new List<MageController>(_mageControllers);
//     // //     Predicate<MageController> predicate = mage => mage.hand.spellsCount >= 8 && !mage.isDead;
//     // //     while (_spellsDeck.Count > 0 && magesToDrawCard.Count > 0)
//     // //     {
//     // //         magesToDrawCard.RemoveAll(predicate);
//     // //         foreach(MageController mage in magesToDrawCard)
//     // //         {
//     // //             mage.TakeCard(_spellsDeck);
//     // //         }
//     // //     }
//     // // }

// }
