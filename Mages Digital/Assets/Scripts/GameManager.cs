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

    [SerializeField] private List<Mage> _mages;             // список всех доступных ScriptableObject магов
                                                            
    private List<MageController> _mageControllers;          // список всех созданных магов (контроллеров магов) в игре 
    
    private int _playerMageControllerIndex;                 // индекс мага игрока в списке магов
    private MageController _playerMageController;           // сам маг игрока

    private State _state;                                   // настоящие состояние игры



    public GameObject magePrefab;           // шаблон мага в игре
    public GameObject cardPrefab;           // шаблон карты
    public GameObject fieldCenter;          // GameObject поля игры
    
    public Mage playerMage;                 // выбранный игроком ScriptableObject мага
    
    public  List<MageController>  mageControllers => _mageControllers;
    public  MageController   playerMageController => _playerMageController;



    void Awake()
    {
        CreateSingleton();
        SetupGame();
    }

    void Start()
    {
        
    }

    // логика сингелтона
    void CreateSingleton()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    // начальная загрузка игры
    void SetupGame()        
    {
        SetupMages();        
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
