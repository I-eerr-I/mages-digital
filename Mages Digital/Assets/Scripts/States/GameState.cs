using System.Collections;
using UnityEngine;

public abstract class GameState
{

    protected readonly GameManager _manager;

    public GameState(GameManager gameManager)
    {
        _manager = gameManager;
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }

}
