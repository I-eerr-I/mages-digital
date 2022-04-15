using System.Collections;
using UnityEngine;

public class BeginGameState : GameState
{

    public BeginGameState(GameManager manager) : base(manager) {}

    public override IEnumerator Start()
    {
        yield break;
        // GameEventSystem.instance.onBeginGame?.Invoke();
        
    }
}
