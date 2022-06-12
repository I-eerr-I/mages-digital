using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    
    
    GameObject _range;
    LightningBoltScript _rangeScript;
    Transform _rangeStart;
    Transform _rangeEnd;

    GameObject _single;
    LightningBoltScript _singleScript;
    Transform _singleStart;
    Transform _singleEnd;

    
    void Awake()
    {
        _range = transform.GetChild(0).gameObject;
        _rangeScript = _range.GetComponent<LightningBoltScript>();
        _rangeStart  = _range.transform.GetChild(0);
        _rangeEnd    = _range.transform.GetChild(1);

        _single = transform.GetChild(1).gameObject;
        _singleScript = _single.GetComponent<LightningBoltScript>();
        _singleStart  = _single.transform.GetChild(0);
        _singleEnd    = _single.transform.GetChild(1);
    }


    void Start()
    {
        Enable(false);
    }


    public IEnumerator Generate(Vector3 start, Vector3 end, float duration, float moveTime = 0.1f)
    {
        Setup(start);
        Enable(true);
        
        yield return MoveTo(end, moveTime);
        
        iTween.ShakePosition(_rangeEnd.gameObject, iTween.Hash("amount", new Vector3(1.0f, 1.0f, 1.0f), "time", duration));
        iTween.ShakePosition(_singleEnd.gameObject, iTween.Hash("amount", new Vector3(1.0f, 1.0f, 1.0f), "time", duration));

        yield return new WaitForSeconds(duration);

        Enable(false);
    }

    void Setup(Vector3 start)
    {
        _rangeStart.position  = start;
        _singleStart.position = start;   
    }

    IEnumerator MoveTo(Vector3 end, float moveTime)
    {
        iTween.MoveTo(_rangeEnd.gameObject, iTween.Hash("position", end, "time", moveTime));
        iTween.MoveTo(_singleEnd.gameObject, iTween.Hash("position", end, "time", moveTime));
        yield return new WaitForSeconds(moveTime);
    }

    void Enable(bool enable)
    {
        _range.SetActive(enable);
        _single.SetActive(enable);
    }

}
