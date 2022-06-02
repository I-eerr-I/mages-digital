using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private Outline _outline;
    private Light   _light;

    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _light   = gameObject.GetComponent<Light>();
    }

    void Start()
    {
        if (_outline) _outline.enabled = false;
        if (_light)   _light.enabled   = false;
    }

    void OnMouseOver()
    {
        if (_outline) _outline.enabled = true;
        if (_light)   _light.enabled   = true;
    }

    void OnMouseExit()
    {
        if (_outline) _outline.enabled = false;
        if (_light)   _light.enabled   = false;
    }

}
