using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStartState : GameState
{
    public RoundStartState(GameManager gameManager) : base(gameManager) {}

    public override IEnumerator Start()
    {
        foreach (MageController mage in _manager.mageControllers)
        {
            _manager.StartCoroutine(mage.OnRoundStart());
            yield return new WaitWhile(() => !mage.isReady);
        }
        _manager.SetState(new SpellsCreationState(_manager));
    }
}
