using UnityEngine;

[CreateAssetMenuAttribute(menuName="Mage")]
public class Mage : ScriptableObject
{
    [SerializeField] private string _magename;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _front;

    public string mageName => _magename;
    public Sprite icon     => _icon;
    public Sprite front    => _front;
}
