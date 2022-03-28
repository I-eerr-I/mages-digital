using CardsToolKit;
using UnityEngine;
using UnityEngine.Events;

public abstract class Card : ScriptableObject
{
    [SerializeField] private string _cardname;
    public  string cardname
    {
        get => _cardname;
    }

    [SerializeField] private string _description;
    public  string description
    {
        get => _description;
    }

    [SerializeField] private Sprite _front;
    public  Sprite front
    {
        get => _front;
    }

    [SerializeField] private Sprite _back;
    public  Sprite  back
    {
        get => _back;
    }

    [SerializeField] public Spell spell;
    
}
