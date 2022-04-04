using UnityEngine;

[CreateAssetMenuAttribute(menuName="Mage")]
public class Mage : ScriptableObject
{
    [SerializeField] private string _magename;
    public string magename
    {
        get => _magename;
    }

    [SerializeField] private Sprite _icon;
    public Sprite icon
    {
        get => _icon;
    }
}
