using UnityEngine;

[CreateAssetMenuAttribute(menuName="Mage")]
public class Mage : ScriptableObject
{
    [SerializeField] private string _magename;
    [SerializeField] private Sprite _icon;

    public string mageName => _magename;
    public Sprite icon     => _icon;
}
