using UnityEngine;

[CreateAssetMenuAttribute(menuName="Spells Collection")]
public class SpellsCollection : ScriptableObject
{
    public void Damage()
    {
        Debug.Log("Test spell");
    }

    public void DiceRoll()
    {
        
    }

        
}
