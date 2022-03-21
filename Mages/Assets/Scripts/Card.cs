using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : MonoBehaviour
{

    public string title;
    public string description;

    public abstract void Action();
}
