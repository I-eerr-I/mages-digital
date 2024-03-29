using Enum = System.Enum;
using Array = System.Array;
using Random = System.Random;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class CardEffectsManager : MonoBehaviour
{

    public Random random = new Random();

    private static CardEffectsManager _instance;
    public  static CardEffectsManager  instance => _instance;    


    [Header("Игральные кубики")]
    [SerializeField] GameObject       _die;
    [SerializeField] Transform        _dieLocation;
    
    [Header("Эффекты магии")]
    [SerializeField] MagicManager _magic;


    Dictionary<int, Vector3> DIE_TO_ROTATION = new Dictionary<int, Vector3>()
    {
        { 1, new Vector3(   0.0f,   0.0f, -90.0f) },
        { 2, new Vector3(   0.0f,   0.0f,   0.0f) },
        { 3, new Vector3( -90.0f,   0.0f,   0.0f) },
        { 4, new Vector3(  90.0f,   0.0f,   0.0f) },
        { 5, new Vector3( 180.0f,   0.0f,   0.0f) },
        { 6, new Vector3(   0.0f,   0.0f,  90.0f) }
    };

    public MagicManager magic => _magic;


    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator HighlightEnemiesOfMage(MageController mage, int nTimes)
    {
        for (int i = 0; i < nTimes; i++)
        {
            List<MageController> mages = GameManager.instance.aliveMages.OrderBy(mage => random.Next()).ToList();
            foreach (MageController enemy in mages)
            {
                if (enemy == mage)
                    continue;
                yield return enemy.mageIcon.HighlightForSomeTime(0.1f);
            }   
        }
    }

    public IEnumerator Attack(Vector3 from, Vector3 to, CardController attackSource, float duration = 1.0f)
    {
        Array values = Enum.GetValues(typeof(Sign));
        
        Sign sign = (Sign) values.GetValue(random.Next(values.Length));

        if (attackSource.card.cardType == CardType.SPELL)
        {
            sign = attackSource.GetSpellCard().sign;
        }
        switch (sign)
        {
            case Sign.ARCANE:
                yield return _magic.Arcane(from, to, duration);
                break;
            
            case Sign.DARK:
                yield return _magic.Dark(from, to, duration);
                break;

            case Sign.PRIMAL:
                yield return _magic.Primal(from, to, duration);
                break;
            
            case Sign.ELEMENTAL:
                yield return _magic.Elemental(from, to, duration);
                break;

            case Sign.ILLUSION:
                yield return _magic.Illusion(from, to, duration);
                break;

            default:
                yield return _magic.Arcane(from, to, duration);
                break;
        }
    }


    public IEnumerator RollDice(List<int> rolls)
    {
        int nDice = rolls.Count;
        List<GameObject> dice = new List<GameObject>();

        float deltaX = 0.75f;

        float moveDownTime   = 1.5f;
        float shakeTime      = 1.0f;
        float shakeTimeDelta = 0.2f;
        float waitTime       = 2.5f;


        float x = -(deltaX * (nDice-1)) / 2;
        float y = 10.0f;
        float z = 0.0f;

        for (int i = 0; i < nDice; i++)
        {
            GameObject die = Instantiate(_die, _dieLocation);
            dice.Add(die);

            die.transform.position = new Vector3(x, y, z);
            iTween.MoveTo(die, iTween.Hash("y", 4.0f, "time", moveDownTime));

            x += deltaX;

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(moveDownTime);

        for (int i = 0; i < nDice; i++)
        {
            Hashtable completeParams = new Hashtable();
            completeParams.Add("die",  dice[i]);
            completeParams.Add("roll", rolls[i]);

            Hashtable parameters     = new Hashtable();
            parameters.Add("time", shakeTime);
            parameters.Add("amount", new Vector3(360f, 360f, 360f));
            parameters.Add("oncomplete", "OnRollComplete");
            parameters.Add("oncompleteparams", completeParams);
            parameters.Add("oncompletetarget", gameObject);
            
            iTween.ShakeRotation(dice[i], parameters);
            
            shakeTime += shakeTimeDelta;
        }

        yield return new WaitForSeconds(shakeTimeDelta + 0.05f + waitTime);
        
        foreach (GameObject die in new List<GameObject>(dice))
            iTween.MoveTo(die, iTween.Hash("y", y, "time", 1.0f, "oncomplete", "OnRollFlyOut", "oncompleteparams", die, "oncompletetarget", gameObject));
    }

    public void OnRollComplete(object parameters)
    {
        Hashtable hashtable = (Hashtable) parameters;
        GameObject die   = (GameObject) hashtable["die"];
        Vector3 rotation = DIE_TO_ROTATION[ (int) hashtable["roll"]];
        iTween.RotateTo(die, iTween.Hash("rotation", rotation, "time", 0.5f));
    }

    public void OnRollFlyOut(object die)
    {
        Destroy( (GameObject) die);
    }



}
