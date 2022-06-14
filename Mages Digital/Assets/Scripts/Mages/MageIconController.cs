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
    
    [Header("Объекты")]
    [SerializeField] SpriteMask _cracksMask;
    [SerializeField] GameObject _healthObject;
    [SerializeField] GameObject _medalsObject;
    [SerializeField] GameObject _iconObject;    
    [SerializeField] GameObject _initiativeObject;
    [SerializeField] GameObject _infoObject;
    [SerializeField] Transform  _attackReactions;
    [SerializeField] Transform  _otherReactions;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    bool  _discoverable     = true;
    bool  _highlighted      = false;
    float _mouseOverTime    = 0.0f;
    int   _mouseDownClicked = 0;
    int   _damageHitClicks  = 25;
    int   _deathClicks      = 50;

    bool _choosingEnemyState = false;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    List<Action> _mouseDownActions = new List<Action>();


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    MageController   _mage;
    SerializedObject _halo;


    ParticleSystem _arcaneReaction;
    ParticleSystem _darkReaction;
    ParticleSystem _primalReaction;
    ParticleSystem _elementalReaction;
    ParticleSystem _illusionReaction;

    ParticleSystem _runicReaction;
    ParticleSystem _healReaction;

    SpriteRenderer _healthOutline;
    TextMeshPro    _healthText;

    SpriteRenderer _medalsOutline;
    TextMeshPro    _medalsText;

    SpriteRenderer _iconOutline;
    SpriteRenderer _icon;

    TextMeshPro    _initiativeText;
    ParticleSystem _initiativeParticles;

    TextMeshPro _infoText;
    

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


        _healthOutline = _healthObject.GetComponentInChildren<SpriteRenderer>();
        _healthText    = _healthObject.GetComponentInChildren<TextMeshPro>();

        _medalsOutline = _medalsObject.GetComponentInChildren<SpriteRenderer>();
        _medalsText    = _medalsObject.GetComponentInChildren<TextMeshPro>();
        
        _icon        = _iconObject.GetComponent<SpriteRenderer>();
        _iconOutline = _iconObject.transform.GetChild(0).GetComponent<SpriteRenderer>();

        _initiativeText      = _initiativeObject.GetComponentInChildren<TextMeshPro>();
        _initiativeText.gameObject.SetActive(false);
        _initiativeParticles = _initiativeObject.GetComponentInChildren<ParticleSystem>();

        _infoText = _infoObject.GetComponentInChildren<TextMeshPro>();
        _infoText.gameObject.SetActive(false);

        _arcaneReaction    = _attackReactions.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        _darkReaction      = _attackReactions.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        _elementalReaction = _attackReactions.GetChild(2).gameObject.GetComponent<ParticleSystem>();
        _illusionReaction  = _attackReactions.GetChild(3).gameObject.GetComponent<ParticleSystem>();


        _runicReaction = _otherReactions.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        _healReaction  = _otherReactions.GetChild(1).gameObject.GetComponent<ParticleSystem>();

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
        if (_discoverable && !_highlighted && _mage.owner is PlayerController)
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

        if (_choosingEnemyState)
        {
            GameManager.instance.player.chosenMage = _mage;
        }

        // TEST !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // if (_mage.owner is EnemyController)
        //     _mage.TakeDamage(5);
        // TEST !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }


    void OnMouseOver()
    {
        if (!_highlighted)
        {
            if (!_mage.isDead)
                OnHover();
            else
                OnHoverDead();
        }
        ShowMageInfo();
    }


    void OnMouseExit()
    {
        if (_mouseDownClicked < _damageHitClicks)
            _mouseDownClicked = 0;
        else
            _mouseDownClicked = _damageHitClicks + 1;
        if (!_highlighted)
        {
            if (!_mage.isDead)
                OnHoverExit();
            else
                OnHoverExitDead();
        }
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
        _initiativeParticles.Play();

        ShowInitiativeText(_mage.spellInitiative.ToString());
    }

    public void HideInitiative()
    {
        _initiativeText.gameObject.SetActive(false);
    }

    public void ShowInitiativeText(string text)
    {
        _initiativeText.gameObject.SetActive(true);
        _initiativeText.text = text;
    }

    public void ShowInfoText(string text)
    {
        _infoText.gameObject.SetActive(true);
        _infoText.text = text;
    }

    public void HideInfoText()
    {
        _infoText.text = "";
        _infoText.gameObject.SetActive(false);
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

    public void Highlight(bool highlight)
    {
        _highlighted = highlight;
        if (highlight)
        {
            SetUndiscoverable();
            SetIconSettings(iconHoverColor);
            SetOutlineSettings(Color.white);
            SetHaloSettings(Color.white, haloHoverRange);
        }
        else
        {
            SetDiscoverable();
            SetIconSettings(iconColor);
            SetOutlineSettings(mainColor);
            SetHaloSettings(haloColor, haloRange);
        }
    }

    public IEnumerator HighlightForSomeTime(float time)
    {
        Highlight(true);
        yield return new WaitForSeconds(time);
        Highlight(false);
    }

    public IEnumerator OnTakeDamage(CardController damageSource = null)
    {
        if (damageSource != null)
        {
            yield return React(damageSource);
        }
        else
        {
            Shake();
        }
    }

    public IEnumerator OnHeal()
    {
        _healReaction.Stop();
        _healReaction.Play();
        yield break;
    }

    public IEnumerator OnChangeOrder()
    {
        _runicReaction.Stop();
        _runicReaction.Play();
        yield break;
    }

    public void OnChoosingEnemyState()
    {
        _choosingEnemyState = true;
    }

    public void OnChoosingEnemyStateEnd()
    {
        _choosingEnemyState = false;
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


    IEnumerator React(CardController damageSource)
    {
        Card card = damageSource.card;

        Sign sign = Sign.ARCANE;

        if (card.cardType == CardType.SPELL)
            sign = damageSource.GetSpellCard().sign;

        ParticleSystem reaction = null;
        Action iconReaction = null;
        switch (sign)
        {
            case Sign.ARCANE:
                reaction = _arcaneReaction;
                iconReaction = () =>
                {
                    Shake();
                    IconColorReaction(Color.black);
                };
                break;

            case Sign.DARK:
                reaction = _darkReaction;
                iconReaction = Shake;
                break;
            
            case Sign.PRIMAL:
                reaction = null;
                iconReaction = () => 
                {
                    _cracksMask.enabled = true;
                    Shake();
                    SetCrackCutoff(1.0f);
                    IconColorReaction(Color.green);
                    CracksOff(5.0f);
                };
                break;
            
            case Sign.ELEMENTAL:
                reaction = _elementalReaction;
                iconReaction = () =>
                {
                    Shake();
                    IconColorReaction(Color.red);
                };
                break;
            
            case Sign.ILLUSION:
                reaction = _illusionReaction;
                iconReaction = () =>
                {
                    ShakeScale();
                    iTween.RotateTo(gameObject, iTween.Hash("z", 360*20, "time", 3.0f, "islocal", true, "easetype", iTween.EaseType.easeInCirc));
                };
                break;
        }

        reaction?.Stop();
        yield return new WaitForSeconds(GetReactionWaitTime(sign));
        reaction?.Play();
        iconReaction?.Invoke();
        yield return new WaitForSeconds(0.15f);
    }

    float GetReactionWaitTime(Sign sign)
    {
        switch (sign)
        {
            case Sign.ARCANE:    
                return 0.01f;
            case Sign.DARK:    
                return 1.00f;
            case Sign.PRIMAL:    
                return 1.00f;
            case Sign.ELEMENTAL:    
                return 0.05f;
            case Sign.ILLUSION:    
                return 1.50f;
        }
        return 0.01f;
    }

    void IconColorReaction(Color color)
    {
        Color prevColor = _icon.color;
        _icon.color = color;
        SetUndiscoverable();
        iTween.ValueTo(gameObject, iTween.Hash("from", _icon.color, "to", prevColor, "time", 5.0f, "onupdate", "SetIconColor", "oncomplete", "SetDiscoverable"));
    }

    void Shake()
    {
        iTween.ShakePosition(gameObject, new Vector3(UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f)), UnityRandom.Range(0.5f, 1.0f));
    }

    void ShakeScale()
    {
        iTween.ShakeScale(gameObject, new Vector3(UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f),UnityRandom.Range(0.1f, 0.5f)), UnityRandom.Range(0.5f, 1.0f));
    }

    void CracksOn(float duration = 10)
    {
        _cracksMask.enabled = true;
        SetCrackCutoff(1.0f);
        iTween.ValueTo(gameObject, iTween.Hash("from", 1.0f, "to", 0.5f, "time", duration, "onupdate", "SetCrackCutoff"));
    }


    void CracksOff(float duration = 0.5f)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", _cracksMask.alphaCutoff, "to", 1.0f, "time", duration, "onupdate", "SetCrackCutoff", "oncomplete", "DisableCraks"));
    }


    void DisableCraks()
    {
        _cracksMask.enabled = false;
    }


    void SetCrackCutoff(float cutoff)
    {
        _cracksMask.alphaCutoff = cutoff;
    }

    void SetIconColor(Color color)
    {
        _icon.color = color;
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
