using CardsToolKit;
using UnityEngine;
using UnityEngine.Events;

public abstract class Card : ScriptableObject
{

    [SerializeField] protected Sprite   _front;
    [SerializeField] protected string   _cardName;
    [SerializeField] protected string   _description;
    [SerializeField] protected CardType _cardType;
    // [SerializeField] protected Spell    _spell;
    [SerializeField] protected string _spell; 
    
    public Sprite   front       => _front;
    public string   cardName    => _cardName;
    public string   description => _description;
    public CardType cardType    => _cardType;
    // public Spell    spell       => _spell;
    public string   spell       => _spell;
}
