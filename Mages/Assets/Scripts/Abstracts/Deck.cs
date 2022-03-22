using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Deck<T> : MonoBehaviour
{

    public static Deck<T> instance = null;

    public List<T> deck;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }
    }

}
