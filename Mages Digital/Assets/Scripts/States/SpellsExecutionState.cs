using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsExecutionState : GameState
{
    public SpellsExecutionState() : base() {}

    public override IEnumerator Start()
    {
        yield return _uiManager.FadeInAndOutInfoText("Executing spells!");
        yield return _seManager.SetupSpellExecution();
        
    }
}
