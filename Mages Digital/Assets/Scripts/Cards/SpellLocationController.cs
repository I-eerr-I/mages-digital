using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class SpellLocationController : MonoBehaviour
{


    [SerializeField] Color _color;
    [SerializeField] Order _order;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    Vector3 _defaultPosition;
    Color _lightDefaultColor;
    
    float _outlineFadeInTime        = 2.0f;
    float _outlineFadeOutTime       = 1.0f; 
    float _lightFadeTime            = 0.5f;
    float _lightMaxRange            = 5.0f;
    float _outlineMaxWidth          = 10.0f;
    float _choosingY                = 2.0f;
    float _movingUpTime             = 0.1f; 
    float _movingDownTime           = 0.05f; 
    Order _chosenOrder              = Order.WILDMAGIC;
    iTween.EaseType _movingEaseType = iTween.EaseType.spring;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    Outline      _outline;
    Light        _light;
    BoxCollider  _collider;
    MeshRenderer _meshRenderer;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public Order chosenOrder => _chosenOrder;
    public Order       order => _order;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    public bool isOrderChosen => _chosenOrder != Order.WILDMAGIC;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _light   = gameObject.GetComponent<Light>();

        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _collider     = gameObject.GetComponent<BoxCollider>();

        _defaultPosition = transform.position;
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


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    IEnumerator MoveUp()
    {
        iTween.MoveTo(gameObject, iTween.Hash("y", _choosingY, "time", _movingUpTime, "easetype", _movingEaseType));
        yield return new WaitForSeconds(_movingUpTime);
    }


    IEnumerator MoveDown()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", _defaultPosition, "time", _movingDownTime, "easetype", _movingEaseType));
        yield return new WaitForSeconds(_movingDownTime);
    }


    public IEnumerator StartChoice()
    {
        yield return MoveUp();
        FadeInLight();
        _chosenOrder = Order.WILDMAGIC;
        _collider.enabled = true;
    }


    public IEnumerator EndChoice()
    {
        yield return MoveDown();
        FadeOutLight();
        _collider.enabled = false;
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void FadeInLight()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _light.range, "to", _lightMaxRange, "time", _lightFadeTime, "onupdate", "ChangeRange", "onstart", "EnableLight"));
    }


    void FadeOutLight()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _light.range, "to", 0.0f, "time", _lightFadeTime, "onupdate", "ChangeRange", "onstart", "DisableLight"));
    }


    public void FadeInOutline()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", _outlineMaxWidth, "time", _outlineFadeInTime, "onupdate", "ChangeWidth", "onstart", "EnableMesh"));
    }


    public void FadeOutOutline()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", 0.0f, "time", _outlineFadeOutTime, "onupdate", "ChangeWidth", "oncomplete", "DisableMesh"));
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
