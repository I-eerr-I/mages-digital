using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MageOrderIconController : MonoBehaviour
{
    

    [SerializeField] Color _idleIconColor;
    [SerializeField] Color _highlightedIconColor;

    SpriteRenderer   _icon;
    SerializedObject _halo;


    void Awake()
    {
        GameObject iconObject = transform.GetChild(0).gameObject;
        _icon = iconObject.GetComponent<SpriteRenderer>();
        _halo = new SerializedObject(iconObject.GetComponent("Halo"));

        Highlight(false);
    }

    public void SetIcon(Sprite icon)
    {
        _icon.sprite = icon;
    }

    public void Highlight(bool highlight)
    {
        EnableHalo(highlight);

        Color iconColor = highlight ? _highlightedIconColor : _idleIconColor;
        _icon.color = iconColor;
    }

    public void FlyOut()
    {
        iTween.MoveTo(gameObject, iTween.Hash("y", 20.0f, "time", 1.0f, "oncomplete", "DestroyObject"));
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }

    void EnableHalo(bool enable)
    {
        _halo.FindProperty("m_Enabled").boolValue = enable;
        _halo.ApplyModifiedProperties();
    }

}
