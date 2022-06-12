using System;
using Random = System.Random;
using UnityRandom = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using TMPro;
using UnityEngine;
using UnityEditor;

public class MageIconController : MonoBehaviour
{


    public Random random = new Random();


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    [SerializeField] float _mageShowInfoWaitTime;
    
    [Header("Трещины")]
    [SerializeField] SpriteMask     _cracksMask;
    
    [Header("Здоровье")]
    [SerializeField] SpriteRenderer _healthOutline;
    [SerializeField] TextMeshPro    _healthText;
    
    [Header("Медали")]
    [SerializeField] SpriteRenderer _medalsOutline;
    [SerializeField] TextMeshPro    _medalsText;
    
    [Header("Иконка")]
    [SerializeField] SpriteRenderer _iconOutline;
    [SerializeField] SpriteRenderer _icon;
    
    [Header("Инициатива")]
    [SerializeField] GameObject  _initiativeObject;
    [SerializeField] TextMeshPro _initiativeText;

    [Header("Реакция на атаку")]
    [SerializeField] GameObject _lightningSmoke;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    bool  _discoverable     = true;
    float _mouseOverTime    = 0.0f;
    int   _mouseDownClicked = 0;
    int   _damageHitClicks  = 25;
    int   _deathClicks      = 50;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    List<Action> _mouseDownActions = new List<Action>();


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    MageController   _mage;
    SerializedObject _halo;
    

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    [Header("Основной цвет")]
    public Color mainColor;
    public Color mainHoverColor;
    public Color iconColor;
    public Color iconHoverColor;
    
    [Header("Настройки Halo")]
    public Color haloColor;    
    public float haloRange;
    public float haloHoverRange;

    [Header("Внешний вид после смерти")]
    public Color haloDeathColor;
    public Color mainDeathColor;
    public Color mainDeathHoverColor;
    public Color iconDeathColor;
    public Color iconDeathHoverColor;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        _mage       = gameObject.GetComponentInParent<MageController>();
        _halo       = new SerializedObject(gameObject.GetComponent("Halo"));

        InitializeMouseDownActions();
    }


    void Start()
    {
        _cracksMask.enabled = false;
    
        if (_mage.mage != null)
            _icon.sprite = _mage.mage.icon;

        OnHoverExit();
    }


    void Update()
    {
        _healthText.text = _mage.health.ToString();
        _medalsText.text = _mage.medals.ToString();
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void InitializeMouseDownActions()
    {
        _mouseDownActions.Add(() => 
        {
            Hashtable parameters = new Hashtable();
            parameters.Add("amount", new Vector3((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()));
            parameters.Add("time", (float) random.NextDouble());
            parameters.Add("oncomplete", "SetDiscoverable");
            iTween.ShakePosition(gameObject, parameters);
        });
        _mouseDownActions.Add(() => 
        {
            Hashtable parameters = new Hashtable();
            parameters.Add("amount", new Vector3((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()));
            parameters.Add("time", (float) random.NextDouble());
            parameters.Add("oncomplete", "SetDiscoverable");
            iTween.ShakeScale(gameObject, parameters);
        });
        _mouseDownActions.Add(() => 
        {
            haloColor = new Color((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            mainColor = new Color((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            mainHoverColor = new Color((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            _discoverable = true;
        });
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    IEnumerator OnMouseDown()
    {
        if (_discoverable && !_mage.ownerIsBot)
        {
            _mouseDownClicked++;

            _discoverable = false;
            int index = random.Next(_mouseDownActions.Count);
            _mouseDownActions[index].Invoke();
            yield return new WaitForSeconds(1.0f);


            if (_mouseDownClicked == _damageHitClicks)
            {
                _mage.TakeDamage(5);
            }
            else if (_mouseDownClicked == _deathClicks)
            {
                _mage.TakeDamage(25);
            }
        }

        // TEST !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (_mage.ownerIsBot)
            _mage.TakeDamage(5);
        // TEST !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }


    void OnMouseOver()
    {
        if (!_mage.isDead)
            OnHover();
        else
            OnHoverDead();
        ShowMageInfo();
    }


    void OnMouseExit()
    {
        if (_mouseDownClicked < _damageHitClicks)
            _mouseDownClicked = 0;
        else
            _mouseDownClicked = _damageHitClicks + 1;
        if (!_mage.isDead)
            OnHoverExit();
        else
            OnHoverExitDead();
        HideMageInfo();
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnHover()
    {
        SetIconSettings(iconHoverColor);
        SetOutlineSettings(mainHoverColor);
        SetHaloSettings(haloColor, haloHoverRange);
    }


    void OnHoverExit()
    {
        SetIconSettings(iconColor);
        SetOutlineSettings(mainColor);
        SetHaloSettings(haloColor, haloRange);
    }


    void OnHoverDead()
    {
        SetIconSettings(iconDeathHoverColor);
        SetOutlineSettings(mainDeathHoverColor);
        SetHaloSettings(haloDeathColor, haloHoverRange);
    }


    void OnHoverExitDead()
    {
        SetIconSettings(iconDeathColor);
        SetOutlineSettings(mainDeathColor);
        SetHaloSettings(haloDeathColor, haloRange);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public void ShowInitiative()
    {
        _initiativeObject.SetActive(true);
        _initiativeText.text = _mage.spellInitiative.ToString();
    }


    public void HideInitiative()
    {
        _initiativeObject.SetActive(false);
    }

    public void ShowMageInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= _mageShowInfoWaitTime)
            UIManager.instance.ShowMageInfo(_mage, true);
    }


    public void HideMageInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowMageInfo(_mage, false);
    }


    public IEnumerator OnTakeDamage(CardController damageSource = null)
    {
        iTween.ShakePosition(gameObject, new Vector3(UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f)), UnityRandom.Range(0.5f, 1.0f));
        
        if (damageSource != null)
        {
            GameObject fxObject = GetFXObject(damageSource);
            fxObject.SetActive(false);
            yield return new WaitForSeconds(0.15f);
            fxObject.SetActive(true);
        }
    }

    public void OnDeath()
    {
        SetOutlineSettings(mainDeathColor);
        SetHaloSettings(haloDeathColor, haloRange);
        SetIconSettings(iconDeathColor);
        CracksOn();
    }


    public void OnReset()
    {
        SetIconSettings(Color.white);
        SetOutlineSettings(mainColor);
        SetHaloSettings(haloColor, haloRange);
        CracksOff();
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    GameObject GetFXObject(CardController damageSource)
    {
        Card card = damageSource.card;

        Sign sign = Sign.ARCANE;

        if (card.cardType == CardType.SPELL)
            sign = damageSource.GetSpellCard().sign;

        switch (sign)
        {
            case Sign.ARCANE:
                return _lightningSmoke;

            default: 
                return _lightningSmoke;
        }
    }


    void CracksOn()
    {
        _cracksMask.enabled = true;
        SetCrackCutoff(1.0f);
        iTween.ValueTo(gameObject, iTween.Hash("from", 1.0f, "to", 0.5f, "time", 10.0f, "onupdate", "SetCrackCutoff"));
    }


    void CracksOff()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _cracksMask.alphaCutoff, "to", 1.0f, "time", 0.5f, "onupdate", "SetCrackCutoff", "oncomplete", "DisableCraks"));
    }


    void DisableCraks()
    {
        _cracksMask.enabled = false;
    }


    void SetCrackCutoff(float cutoff)
    {
        _cracksMask.alphaCutoff = cutoff;
    }


    void SetHaloSettings(Color color, float range)
    {
        _halo.FindProperty("m_Color").colorValue = color;
        _halo.FindProperty("m_Size").floatValue  = range;
        _halo.ApplyModifiedProperties();
    }


    void SetHaloRange(float range)
    {
        _halo.FindProperty("m_Size").floatValue  = range;
        _halo.ApplyModifiedProperties();
    }


    void SetOutlineSettings(Color color)
    {
        _healthOutline.color = color;
        _medalsOutline.color = color;
        _iconOutline.color   = color;
    }


    void SetIconSettings(Color color)
    {
        _icon.color = color;
    }


    void UpdateSettings()
    {
        SetIconSettings(iconColor);
        SetOutlineSettings(mainColor);
        SetHaloSettings(haloColor, haloRange);
    }


    public void SetDiscoverable()
    {
        _discoverable = true;
    }


    public void SetUndiscoverable()
    {
        _discoverable = false;
    }
    

}
