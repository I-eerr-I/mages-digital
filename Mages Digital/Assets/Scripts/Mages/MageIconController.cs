using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MageIconController : MonoBehaviour
{

    [SerializeField] private SpriteRenderer _healthOutline;
    [SerializeField] private SpriteRenderer _iconOutline;
    [SerializeField] private SpriteRenderer _icon;

    
    private MageController _mage;
    private SerializedObject _halo;
    private TextMesh _healthText;
    


    public Color mainColor;
    
    [Header("Настройки Halo")]
    public Color haloColor;
    public Color hoverColor;
    public float haloRange;
    public float hoverRange;


    void Awake()
    {
        _mage       = gameObject.GetComponentInParent<MageController>();
        _halo       = new SerializedObject(gameObject.GetComponent("Halo"));
        _healthText = gameObject.GetComponentInChildren<TextMesh>();
    }

    void Start()
    {
        if (_mage.mage != null)
            _icon.sprite = _mage.mage.icon;
        UpdateSettings();
    }

    void Update()
    {
        _healthText.text = _mage.health.ToString();
    }

    void OnMouseOver()
    {
        SetHaloSettings(hoverColor, hoverRange);
    }

    void OnMouseExit()
    {
        SetHaloSettings(haloColor, haloRange);
    }

    void SetHaloSettings(Color color, float range)
    {
        _halo.FindProperty("m_Color").colorValue = color;
        _halo.FindProperty("m_Size").floatValue  = range;
        _halo.ApplyModifiedProperties();
    }


    void UpdateSettings()
    {
        _healthOutline.color = mainColor;
        _iconOutline.color   = mainColor;

        SetHaloSettings(haloColor, haloRange);
    }

}
