using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowMagePanelController : MonoBehaviour
{
    [Header("Настройка цвета")]
    public Color healthTextColorFrom;
    public Color healthTextColorTo;
    public Color medalsTextColorFrom;
    public Color medalsTextColorTo;
    
    [Header("Настройка анимации")]
    public float colorChangeTime = 1.0f;

    [Header("Компоненты")]
    public TMP_Text healthText;
    public TMP_Text medalsText;


    void OnEnable()
    {
        StartColorChange();
    }

    void OnDisable()
    {
        iTween.Stop(gameObject);
    }

    void StartColorChange()
    {
        Hashtable parameters = GetColorChangeParameters(healthTextColorFrom, healthTextColorTo, "OnHealthColorUpdate");
        iTween.ValueTo(gameObject, parameters);

        parameters = GetColorChangeParameters(medalsTextColorFrom, medalsTextColorTo, "OnMedalsColorUpdate");
        parameters.Add("oncomplete", "ReversedColorChange"); // FOR THE LAST COLOR CHANGE
        iTween.ValueTo(gameObject, parameters);
    }

    void ReversedColorChange()
    {
        Hashtable parameters = GetColorChangeParameters(healthTextColorTo, healthTextColorFrom, "OnHealthColorUpdate");
        iTween.ValueTo(gameObject, parameters);

        parameters = GetColorChangeParameters(medalsTextColorTo, medalsTextColorFrom, "OnMedalsColorUpdate");
        parameters.Add("oncomplete", "StartColorChange"); // FOR THE LAST COLOR CHANGE
        iTween.ValueTo(gameObject, parameters);
    }

    Hashtable GetColorChangeParameters(Color from, Color to, string onupdate)
    {
        Hashtable parameters = new Hashtable();
        parameters.Add("from", from);
        parameters.Add("to",   to);
        parameters.Add("time", colorChangeTime);
        parameters.Add("onupdate", onupdate);
        return parameters;
    }

    void OnHealthColorUpdate(Color color)
    {
        healthText.color = color;
    }

    void OnMedalsColorUpdate(Color color)
    {
        medalsText.color = color;
    }

}
