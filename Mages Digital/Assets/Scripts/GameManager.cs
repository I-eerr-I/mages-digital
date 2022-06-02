using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    public GameObject cardPrefab;
    public Transform  fieldCenter;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }
}
