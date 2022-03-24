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
        enemyGoal = thisChar.enemyGoal;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = false;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position+ thisChar.enemyGoal.transform.forward*5);
        if (enemyGoal.Distance(thisChar) < 6) {
            thisChar.Idle();
        }
        if (!thisChar.ball.isHeld || (thisChar.ball.TeamHolding() != thisChar.team)) {
           // thisChar.Idle();
        }
    }

    public override void AnimEventReceiver(string message) {
        
    }

    public override void WillExit() {
        base.WillExit();
    }
}
