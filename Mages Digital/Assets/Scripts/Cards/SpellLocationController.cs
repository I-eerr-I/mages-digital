using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class SpellLocationController : MonoBehaviour
{

    [SerializeField] private Color _lightDefaultColor;
    [SerializeField] private Color _color;
    [SerializeField] private float _outlineFadeTime = 2.0f;
    [SerializeField] private float _lightFadeTime   = 0.5f;
    [SerializeField] private float _lightMaxRange   = 5.0f;
    [SerializeField] private float _outlineMaxWidth = 10.0f;

    private Order _chosenOrder = Order.WILDMAGIC;

    private Outline _outline;
    private Light   _light;

    private MeshRenderer _meshRenderer;
    private BoxCollider  _collider;


    public Order order;

    public Order chosenOrder => _chosenOrder;
    
    public bool isOrderChosen => _chosenOrder != Order.WILDMAGIC;
    

    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _light   = gameObject.GetComponent<Light>();

        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _collider     = gameObject.GetComponent<BoxCollider>();
    }

    void Start()
    {
        _outline.OutlineColor = _color;
        _outline.OutlineWidth = 0.0f;
        
        _light.color   = _lightDefaultColor;
        _light.range   = 0.0f;
        _light.enabled = false;

        _meshRenderer.enabled = false;
        _collider.enabled = false;
    }

    public void StartChoice()
    {
        FadeInLight();
        _chosenOrder = Order.WILDMAGIC;
        _collider.enabled = true;
    }

    public void EndChoice()
    {
        FadeOutLight();
        _collider.enabled = false;
    }

    void OnMouseOver()
    {
        _light.color = _color;
    }

    void OnMouseExit()
    {
        _light.color = _lightDefaultColor;
    }

    void OnMouseDown()
    {
        _chosenOrder = order;
    }

    public void FadeInLight()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _light.range, "to", _lightMaxRange, "time", _lightFadeTime, "onupdate", "ChangeRange", "onstart", "EnableLight"));
    }

    public void FadeOutLight()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _light.range, "to", 0.0f, "time", _lightFadeTime, "onupdate", "ChangeRange", "onstart", "DisableLight"));
    }

    public void FadeInOutline()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", _outlineMaxWidth, "time", _outlineFadeTime, "onupdate", "ChangeWidth", "onstart", "EnableMesh"));
    }

    public void FadeOutOutline()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", 0.0f, "time", _outlineFadeTime, "onupdate", "ChangeWidth", "oncomplete", "DisableMesh"));
    }

    public void EnableMesh()
    {
        _meshRenderer.enabled = true;
    }

    public void DisableMesh()
    {
        _meshRenderer.enabled = false;
    }

    public void EnableLight()
    {
        _light.enabled = true;
    }

    public void DisableLight()
    {
        _light.enabled = false;
    }

    public void ChangeWidth(float value)
    {
        _outline.OutlineWidth = value;
    }

    public void ChangeRange(float value)
    {
        _light.range = value;
    }
}
