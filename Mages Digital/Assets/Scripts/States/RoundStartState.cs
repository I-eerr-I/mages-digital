using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStartState : GameState
{
    public RoundStartState() : base() {}

    public override IEnumerator Start()
    {
        Debug.Log("Round Start State"); // TEST
        _gameManager.SetupNewRound();
        _gameManager.StartCoroutine(_uiManager.FadeInAndOutInfoText("Round " + _gameManager.roundNumber));
        yield return new WaitForSeconds(2.0f);
        foreach (MageController mage in _gameManager.mageControllers)
        {
            yield return mage.OnRoundStart();
            yield return new WaitWhile(() => !mage.isReady);
        }
        _gameManager.SetState(new SpellsCreationState());
    }
}
