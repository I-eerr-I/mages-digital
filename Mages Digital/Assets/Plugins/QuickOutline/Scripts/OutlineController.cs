using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OutlineController : MonoBehaviour
{

    [SerializeField] private Color _color;

    private Outline          _outline;
    private Light            _light;
    private SerializedObject _halo;

    public bool withLight = true;
    public bool withHalo  = true;

    public bool state     = false;

    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _light   = gameObject.GetComponent<Light>();
        _halo    = new SerializedObject(gameObject.GetComponent("Halo"));
    }

    void Start()
    {
        SetState(state);
        SetColor(_color);
    }

    public void SetProperties(bool light, bool halo)
    {
        withLight = true;
        withHalo  = true;
        SetState(false);
        withLight = light;
        withHalo  = halo;
    }

    public void SetColor(Color color)
    {
        _color = color;
        if (_outline != null) _outline.OutlineColor = _color;
        if (_light != null)   _light.color = _color;
        if (_halo != null)    
        {
            _halo.FindProperty("m_Color").colorValue = _color;
            _halo.ApplyModifiedProperties();
        }
    }

    void SetState(bool state)
    {
        if (_outline != null) _outline.enabled = state;
        if (_light != null && withLight) _light.enabled = state;
        if (_halo != null && withHalo)    
        {
            _halo.FindProperty("m_Enabled").boolValue = state;
            _halo.ApplyModifiedProperties();
        }
    }

    void OnMouseOver()
    {
        SetState(true);
    }

    void OnMouseExit()
    {
        SetState(false);
    }

}
