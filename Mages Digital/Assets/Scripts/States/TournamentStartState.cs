using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentStartState : GameState
{

    public TournamentStartState() : base() {}

    public override IEnumerator Start()
    {
        yield return _uiManager.FadeInMagesChoiceMenu();
        yield return _uiManager.CreateMageChoiceMenuButtons();
    }

    public override IEnumerator End(GameState state)
    {
        yield return _uiManager.FadeOutMagesChoiceMenu();
        yield return new WaitForSeconds(0.5f);
        _gameManager.SetupGame();
        yield return new WaitForSeconds(0.5f);
        _gameManager.SetState(state);
    }
}
