using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // сингелтон объект GameManager
    // должен быть доступен для всех GameObject в игре
    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    // шаблон мага в игре
    public GameObject magePrefab;

    // шаблон карты
    public GameObject cardPrefab;

    // GameObject поля игры
    public GameObject fieldCenter;

    // все существующие начальные колоды (от GameObject)
    [SerializeField] private DeckController _spellsDeck;
    public DeckController spellsDeck => _spellsDeck;
    [SerializeField] private DeckController _treasuresDeck;
    public DeckController treasuresDeck => _treasuresDeck;
    [SerializeField] private DeckController _deadsDeck;
    public DeckController deadsDeck => _deadsDeck;

    // список всех доступных ScriptableObject магов
    [SerializeField] private List<Mage> _mages = new List<Mage>();

    // список всех созданных магов (контроллеров магов) в игре 
    private List<MageController> _mageControllers =  new List<MageController>();
    public  List<MageController>  mageControllers => _mageControllers;


    // выбранный игроком ScriptableObject мага
    public  Mage playerMage;

    // сам маг игрока
    private int _playerMageControllerIndex;
    private MageController _playerMageController;
    public  MageController  playerMageController => _playerMageController;


    void Awake()
    {
        // логика сингелтона
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        InitializeGame();
    }

    void Start()
    {
        DrawCards();
    }

    // начальная загрузка игры
    void InitializeGame()
    {
        // создание всех магов
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
