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
    Transform  _darkStart;
    Transform  _darkEnd;


    
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
        Transform darkObject = _dark.transform.GetChild(0);
        _darkStart = darkObject.GetChild(0);
        _darkEnd   = darkObject.GetChild(1);
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

    public IEnumerator Dark(Vector3 start, Vector3 end, float duration, float moveTime = 0.1f)
    {
        DarkSetup(start, end);

        _dark.SetActive(true);

        Shake(_darkEnd.gameObject, duration);

        yield return new WaitForSeconds(duration);

        _dark.SetActive(false);
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

    void DarkSetup(Vector3 start, Vector3 end)
    {
        _darkStart.position = start;
        _darkEnd.position   = end;
    }

}
