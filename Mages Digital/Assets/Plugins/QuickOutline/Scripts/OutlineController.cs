using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OutlineController : MonoBehaviour
{

    [SerializeField] private Color _color;

    private Outline   _outline;
    private Light     _light;
    private SerializedObject _halo;

    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _light   = gameObject.GetComponent<Light>();
        _halo    = new SerializedObject(gameObject.GetComponent("Halo"));
    }

    void Start()
    {
        if (_light != null)   _light.enabled   = false;
        if (_outline != null) _outline.enabled = false;
        if (_halo != null)    
        {
            _halo.FindProperty("m_Enabled").boolValue = false;
            _halo.ApplyModifiedProperties();
        }
        SetColor(_color);
    }

    public void SetColor(Color color)
    {
        _color = color;
        if (_light != null)   _light.color = _color;
        if (_outline != null) _outline.OutlineColor = _color;
        if (_halo != null)    
        {
            _halo.FindProperty("m_Color").colorValue = _color;
            _halo.ApplyModifiedProperties();
        }
    }

    void OnMouseOver()
    {
        if (_light != null)   _light.enabled   = true;
        if (_outline != null) _outline.enabled = true;
        if (_halo != null)    
        {
            _halo.FindProperty("m_Enabled").boolValue = true;
            _halo.ApplyModifiedProperties();
        }
    }

    void OnMouseExit()
    {
        if (_light != null)   _light.enabled   = false;
        if (_outline != null) _outline.enabled = false;
        if (_halo != null)    
        {
            _halo.FindProperty("m_Enabled").boolValue = false;
            _halo.ApplyModifiedProperties();
        }
    }

}
