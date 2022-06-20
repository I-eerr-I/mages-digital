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

        // yield return gm.treasuresDeck.PassCardsTo(gm.player, 4);
        // yield return gm.spellsDeck.PassCardsTo(gm.player, 4);

        // yield return gm.SpellCreation();


        // yield return gm.treasuresDeck.PassCardsTo(gm.player, 4);
        // yield return gm.spellsDeck.PassCardsTo(gm.mages[1], 4);

        // yield return ((EnemyController) gm.mages[1].owner).OnSpellCreationState();


        // yield return gm.SpellExecution();
        // yield return gm.RoundEnd();


        // yield return new WaitForSeconds(0.5f);
        // yield return CardEffectsManager.instance.magic.Primal(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(-6.22f, 1.0f, 0.08f), 2.0f);

        // TEST


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
