using System;
using Random = System.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MageIconController : MonoBehaviour
{

    [SerializeField] private SpriteMask     _cracksMask;
    [SerializeField] private SpriteRenderer _healthOutline;
    [SerializeField] private TextMesh       _healthText;
    [SerializeField] private SpriteRenderer _medalsOutline;
    [SerializeField] private TextMesh       _medalsText;
    [SerializeField] private SpriteRenderer _iconOutline;
    [SerializeField] private SpriteRenderer _icon;

    private MageController _mage;
    private SerializedObject _halo;


    private float _mouseOverTime = 0.0f;
    private bool  _discoverable  = true;

    private int _mouseDownClicked = 0;
    private int _damageHitClicks  = 25;
    private int _deathClicks      = 50;
    private List<Action> _mouseDownActions = new List<Action>();
    
    public Random random = new Random();

    public float mageShowInfoWaitTime;

    [Header("Основной цвет")]
    public Color mainColor;
    public Color mainHoverColor;
    public Color iconColor;
    public Color iconHoverColor;
    
    [Header("Настройки Halo")]
    public Color haloColor;
    // public Color haloHoverColor;
    public float haloRange;
    public float haloHoverRange;

    [Header("Внешний вид после смерти")]
    public Color haloDeathColor;
    // public Color haloDeathHoverColor;
    public Color mainDeathColor;
    public Color mainDeathHoverColor;
    public Color iconDeathColor;
    public Color iconDeathHoverColor;


    void Awake()
    {
        _mage       = gameObject.GetComponentInParent<MageController>();
        _halo       = new SerializedObject(gameObject.GetComponent("Halo"));

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

    void Shake(Vector3 strength, float time = 0.5f, string what = "position")
    {
        if (what == "position")
            iTween.ShakePosition(gameObject, strength, time);
        else if (what == "rotation")
            iTween.ShakeRotation(gameObject, strength, time);
    }

    IEnumerator OnMouseDown()
    {
        if (_discoverable)
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

    public void ShowMageInfo()
    {
        _mouseOverTime += Time.deltaTime;
        if (_mouseOverTime >= mageShowInfoWaitTime)
            UIManager.instance.ShowMageInfo(_mage, true);
    }

    public void HideMageInfo()
    {
        _mouseOverTime = 0.0f;
        UIManager.instance.ShowMageInfo(_mage, false);
    }

    public void OnDeath()
    {
        iTween.ShakePosition(gameObject, new Vector3(1.0f,1.0f,1.0f), 1.0f);
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
