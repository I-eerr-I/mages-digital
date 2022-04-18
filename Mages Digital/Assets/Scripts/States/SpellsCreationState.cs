using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsCreationState : GameState
{
    public SpellsCreationState(GameManager gameManager) : base(gameManager) {}


    public override IEnumerator Start()
    {
        _manager.mageControllers.ForEach((mage) => mage.Unready());
        List<MageController> aliveMages = _manager.mageControllers.FindAll((mage) => !mage.isDead);
        yield return new WaitWhile(() => aliveMages.FindAll((mage) => !mage.isReady).Count > 0);
    }

}
