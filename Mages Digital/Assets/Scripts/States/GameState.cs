using System.Collections;
using UnityEngine;

public abstract class GameState
{

    protected readonly GameManager _gameManager;
    protected readonly UIManager   _uiManager;

    public GameState()
    {
        _gameManager = GameManager.instance;
        _uiManager   = UIManager.instance; 
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }

    public virtual IEnumerator End(GameState state)
    {
        yield break;
    }

}
