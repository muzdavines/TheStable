using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRunToGoalWithBall : SCBallCarrierState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        thisChar.agent.speed = 4.2f;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position);
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
            return;
        }
        thisChar.agent.isStopped = false;
        if (thisChar.ShouldShoot()) {
            thisChar.Shoot();
            return;
        }
        thisChar.SendTeammateOnRun();
        if (thisChar.ShouldPass()) {
            var passTarget = thisChar.GetPassTarget();
            if (passTarget != null) {
                thisChar.Pass(passTarget);
            }
            return;
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
