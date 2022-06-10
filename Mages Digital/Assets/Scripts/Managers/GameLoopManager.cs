using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{


    private static GameLoopManager _instance;
    public  static GameLoopManager instance => _instance;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    
    public GameManager gm => GameManager.instance;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    IEnumerator Start()
    {
        // TEST
        // yield return new WaitForSeconds(2.0f);
        // foreach (MageController mage in GameManager.instance.mages)
        // {
        //     yield return mage.owner.ShowSpellToAll();
        //     yield return new WaitForSeconds(1.0f);
        //     yield return mage.owner.HideSpellFromAll();
        // }
        // TEST

        // UNCOMMENT
        while (!gm.isGameEnd)
        {
            while (!gm.isTournamentEnd)
            {
                yield return new WaitForSeconds(2.0f);
                
                yield return gm.CardDraw();

                yield return gm.SpellCreation();

                yield return gm.SpellExecution();

                yield return gm.RoundEnd();
            }
            yield return gm.TournamentEnd();            
        }
    }

    
}
