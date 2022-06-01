using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public  static GameManager  instance => _instance;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }
}
