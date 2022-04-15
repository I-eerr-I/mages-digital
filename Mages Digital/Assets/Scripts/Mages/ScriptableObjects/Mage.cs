using UnityEngine;

[CreateAssetMenuAttribute(menuName="Mage")]
public class Mage : ScriptableObject
{
    [SerializeField] private string _magename;
    public string mageName => _magename;
    
    [SerializeField] private Sprite _icon;
}
