using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using UnityEngine;

public class MagicManager : MonoBehaviour
{
    
    
    GameObject _arcane;
    
    Transform _arcaneRangeStart;
    Transform _arcaneRangeEnd;

    Transform _arcaneSingleStart;
    Transform _arcaneSingleEnd;


    GameObject _dark;
    ParticleSystem  _darkParticles;
    GameObject _darkLight;

    
    GameObject _primal;
    ParticleSystem _primalParticles;
    Light _primalLight;


    GameObject _elemental;
    ParticleSystem _elementalParticles;

    GameObject _illusion;
    ParticleSystem _illusionParticles;

    
    void Awake()
    {
        _arcane = transform.GetChild(0).gameObject;

        Transform arcaneRange = _arcane.transform.GetChild(0);
        _arcaneRangeStart  = arcaneRange.GetChild(0);
        _arcaneRangeEnd    = arcaneRange.GetChild(1);

        Transform arcaneSingle = _arcane.transform.GetChild(1);
        _arcaneSingleStart  = arcaneSingle.GetChild(0);
        _arcaneSingleEnd    = arcaneSingle.GetChild(1);


        _dark = transform.GetChild(1).gameObject;
        _darkParticles = _dark.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        _darkParticles.Stop();
        _darkLight     = _darkParticles.transform.GetChild(0).gameObject;

        _primal = transform.GetChild(2).gameObject;
        _primalParticles = _primal.GetComponentInChildren<ParticleSystem>();
        _primalParticles.Stop();
        _primalLight     = _primalParticles.gameObject.GetComponentInChildren<Light>();

        _elemental = transform.GetChild(3).gameObject;
        _elementalParticles = _elemental.GetComponentInChildren<ParticleSystem>();
        _elementalParticles.Stop();

        _illusion = transform.GetChild(4).gameObject;
        _illusionParticles = _illusion.GetComponentInChildren<ParticleSystem>();
        _illusionParticles.Stop();
    }



    public IEnumerator Arcane(Vector3 start, Vector3 end, float duration, float moveTime = 0.1f)
    {
        ArcaneSetup(start);
        _arcane.SetActive(true);
        
        yield return ArcaneMoveTo(end, moveTime);
        
        Shake(_arcaneRangeEnd.gameObject, duration);
        Shake(_arcaneSingleEnd.gameObject, duration);

        yield return new WaitForSeconds(duration);

        _arcane.SetActive(false);
    }


    public IEnumerator Dark(Vector3 start, Vector3 end, float duration, float moveTime = 1f)
    {
        _darkParticles.Stop();

        yield return new WaitUntil(() => _darkParticles.isStopped);

        SetParticlesDuration(_darkParticles, duration);

        _dark.transform.position = start;

        _darkLight.SetActive(true);

        _darkParticles.Play();
        iTween.MoveTo(_dark, iTween.Hash("position", end, "time", moveTime, "easetype", iTween.EaseType.easeInCubic));

        yield return new WaitForSeconds(duration + 0.5f);

        _darkLight.SetActive(false);

    }


    public IEnumerator Primal(Vector3 start, Vector3 end, float duration, float moveTime = 1f)
    {
        _primalParticles.Stop();

        yield return new WaitUntil(() => _primalParticles.isStopped);

        SetParticlesDuration(_primalParticles, duration);
        
        _primal.transform.position = start;

        _primalLight.enabled = true;
        
        _primalParticles.Play();
        iTween.MoveTo(_primal, iTween.Hash("position", end, "time", moveTime, "easetype", iTween.EaseType.easeInCubic));

        yield return new WaitForSeconds(duration + 1.0f);

        _primalLight.enabled = false;
    }

    public IEnumerator Elemental(Vector3 start, Vector3 end, float duration, float moveTime = 1f)
    {
        _elementalParticles.Stop();

        yield return new WaitUntil(() => _elementalParticles.isStopped);

        SetParticlesDuration(_elementalParticles, duration);
        
        start.y += 0.5f;
        _elemental.transform.position = start;
        
        
        _elementalParticles.Play();
        iTween.MoveTo(_elemental, iTween.Hash("position", end, "time", moveTime, "easetype", iTween.EaseType.easeOutElastic));

        yield return new WaitForSeconds(duration + 1.0f);
    }

    public IEnumerator Illusion(Vector3 start, Vector3 end, float duration, float moveTime = 1f)
    {
        _illusionParticles.Stop();
        yield return new WaitUntil(() => _illusionParticles.isStopped);

        SetParticlesDuration(_illusionParticles, duration);

        start.y += 0.5f;
        _illusion.transform.position = start;

        _illusionParticles.Play();
        iTween.MoveTo(_illusion, iTween.Hash("position", end, "time", moveTime, "easetype", iTween.EaseType.linear));

        yield return new WaitForSeconds(duration + 1.5f);
    }



    void Shake(GameObject obj, float duration)
    {
        iTween.ShakePosition(obj, iTween.Hash("amount", new Vector3(1.0f, 1.0f, 1.0f), "time", duration));
    }

    void ArcaneSetup(Vector3 start)
    {
        _arcaneRangeStart.position  = start;
        _arcaneSingleStart.position = start;   
    }

    IEnumerator ArcaneMoveTo(Vector3 end, float moveTime)
    {
        iTween.MoveTo(_arcaneRangeEnd.gameObject, iTween.Hash("position", end, "time", moveTime));
        iTween.MoveTo(_arcaneSingleEnd.gameObject, iTween.Hash("position", end, "time", moveTime));
        yield return new WaitForSeconds(moveTime);
    }

    void SetParticlesDuration(ParticleSystem particles, float duration)
    {
        var main = particles.main;
        main.duration = duration;
    }

}
