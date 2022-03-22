using UnityEngine;
using UnityEngine.Events;

public abstract class Card : MonoBehaviour
{
    
    public string cardname;
    public string description;

    public UnityEvent CardAction;

}
