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
        //     yield return GameManager.instance.treasuresDeck.PassCardsTo(mage, 4);
        // }
        // TEST

        // yield return new WaitForSeconds(0.5f);
        // yield return CardEffectsManager.instance.RollDice(new List<int>() {1, 2, 3, 4, 5, 6});



        // UNCOMMENT
        while (!gm.isGameEnd)
        {
            
            yield return gm.TournamentStart();

            while (!gm.isTournamentEnd)
            {
                yield return new WaitForSeconds(2.0f);
                
                yield return gm.RoundStart();

                yield return gm.SpellCreation();

                yield return gm.SpellExecution();

                yield return gm.RoundEnd();
            }

            yield return gm.TournamentEnd();            
        }
    }

    
}
