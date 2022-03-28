using UnityEngine;

[CreateAssetMenuAttribute(menuName="Spells Collection")]
public class SpellsCollection : ScriptableObject
{
    public void TestSpell()
    {
        Debug.Log("Test spell");
    }    
}
