using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    private List<MageController> _mages = new List<MageController>();


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

    public List<MageController> mages => _mages;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameObject[] mageObjects = GameObject.FindGameObjectsWithTag("Mage");
        foreach (GameObject mageObject in mageObjects)
        {
            _mages.Add(mageObject.GetComponent<MageController>());
        }
    }
}
