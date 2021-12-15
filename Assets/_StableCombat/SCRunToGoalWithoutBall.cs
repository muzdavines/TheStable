using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRunToGoalWithoutBall : SCTeammateBallCarrierState
{
    Goal enemyGoal;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        canGrabBall = true;
        thisChar.agent.speed = 5f;
        enemyGoal = thisChar.enemyGoal;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position);
        if (enemyGoal.Distance(thisChar) < 4) {
            thisChar.Idle();
        }
        if (!thisChar.ball.isHeld) {
            thisChar.Idle();
        }
    }

    public override void AnimEventReceiver(string message) {
        
    }

    public override void WillExit() {
        base.WillExit();
    }
}
