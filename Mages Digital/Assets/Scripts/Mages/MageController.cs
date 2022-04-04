using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{
    [SerializeField] private Mage _mage;
    public Mage mage
    {
        get => _mage;
    }

    [SerializeField] private HandController _hand;
    public HandController hand
    {
        get => _hand;
    }

    [SerializeField] private int _health;
    public int health
    {
        get => _health;
    }
}
