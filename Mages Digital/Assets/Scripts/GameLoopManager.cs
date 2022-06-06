using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    private static GameLoopManager _instance;
    public  static GameLoopManager instance => _instance;

    public GameManager gm => GameManager.instance;

    IEnumerator Start()
    {

        yield return new WaitForSeconds(2.0f);
        yield return gm.CardDraw();
        
        // TEST
        foreach (MageController mage in gm.mages)
        {
            yield return gm.treasuresDeck.PassCardsTo(mage, 8);
        }   
        // TEST

        yield return gm.SpellCreation();

        yield return gm.SpellExecution();
    }
}
