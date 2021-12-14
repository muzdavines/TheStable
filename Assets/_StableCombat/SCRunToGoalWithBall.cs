using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRunToGoalWithBall : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        thisChar.agent.speed = 4.6f;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = false;
        if (thisChar.ShouldShoot()) {
            thisChar.Shoot();
            return;
        }
        if (thisChar.ShouldPass()) {
            thisChar.Pass();
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        thisChar.agent.speed = 5f;
        base.WillExit();
    }
}
