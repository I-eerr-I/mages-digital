using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : MonoBehaviour
{

    [SerializeField] protected int _health = 20;      // здоровье мага
    [SerializeField] protected TextMesh _healthText;  // текст здоровья мага
    [SerializeField] protected bool _isReady = false; // готовность мага к началу раунда


    public Mage mage;  // данные мага
    
    public bool isDead => _health <= 0; // мертв ли маг
    public int health
    {
        get => _health;
        set 
        {
            _health = value;
            OnHealthChange();
        }
    }
    public bool isReady => _isReady;

    public MageController leftMage  = null;
    public MageController rightMage = null;


    void Start()
    {
        OnHealthChange(); // инициализировать текст со здоровьев мага
    }


    // реакция на изменение жизней мага
    public void OnHealthChange()
    {
        _healthText.text = health.ToString();
    }

    // сделать не готовым
    public void Unready()
    {
        _isReady = false;
    }


}
