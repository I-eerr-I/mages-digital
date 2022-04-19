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
        _gameManager.SetupGame();
        yield return _uiManager.FadeOutMagesChoiceMenu();
        yield return new WaitForSeconds(2.0f);
        _gameManager.SetState(state);
    }
}
