using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsExecutionState : GameState
{
    public SpellsExecutionState() : base() {}

    public override IEnumerator Start()
    {
        Debug.Log("SPELLS EXECUTION STATE!"); // TEST
        yield break;
    }
}
