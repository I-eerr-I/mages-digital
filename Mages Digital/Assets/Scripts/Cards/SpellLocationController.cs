using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLocationController : MonoBehaviour
{
    public Color color;
    public float fadeTime = 2.0f;

    private Outline _outline;
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        _outline = gameObject.GetComponentInChildren<Outline>();
        _meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
    }

    void Start()
    {
        _outline.OutlineColor = color;
        _meshRenderer.enabled = false;
    }

    public void FadeIn()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", 10.0f, "time", fadeTime, "onupdate", "ChangeWidth", "onstart", "EnableMesh"));
    }

    public void FadeOut()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _outline.OutlineWidth, "to", 0.0f, "time", fadeTime, "onupdate", "ChangeWidth", "oncomplete", "DisableMesh"));
    }

    public void EnableMesh()
    {
        _meshRenderer.enabled = true;
    }

    public void DisableMesh()
    {
        _meshRenderer.enabled = false;
    }

    public void ChangeWidth(float value)
    {
        _outline.OutlineWidth = value;
    }
}
