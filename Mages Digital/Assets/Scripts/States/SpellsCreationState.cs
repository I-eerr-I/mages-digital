using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsCreationState : GameState
{
    public SpellsCreationState() : base() {}


    // XXX медленное решение (ожидание готовности живых магов)
    // лучше использовать событие у магов
    public override IEnumerator Start()
    {
        Debug.Log("Spells Creation State"); // TEST
        yield return new WaitForSeconds(0.5f);
        _gameManager.StartCoroutine(_uiManager.FadeInAndOutInfoText("Create spells now!"));
        _gameManager.mageControllers.ForEach((mage) => mage.Unready());
        
        foreach (EnemyController enemyMage in _gameManager.enemyControllers)
        {
            if (!enemyMage.isDead)
                enemyMage.CreateRandomSpell();
        }
        // TEST
        _gameManager.playerController.CreateRandomSpell();
        // TEST
        List<MageController> aliveMages = _gameManager.mageControllers.FindAll((mage) => !mage.isDead);
        yield return new WaitWhile(() => aliveMages.FindAll((mage) => !mage.isReady).Count > 0); 
        _gameManager.SetState(new SpellsExecutionState());
    }

}
