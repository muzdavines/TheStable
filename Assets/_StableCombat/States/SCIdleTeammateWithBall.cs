using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdleTeammateWithBall : SCTeammateBallCarrierState
{
    Vector3 goalOffset = Vector3.zero;
    
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        if (thisChar.enemyGoal.Distance(thisChar) < 40 || thisChar.fieldPosition.IsForward()) {
            thisChar.GoNearEnemyGoal();
        } else {
            thisChar.Block();
        }
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {

        Debug.Log("#Message#SCIdleTeammatewithBall " + message);
            return base.ReceiveMessage(sender, message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
