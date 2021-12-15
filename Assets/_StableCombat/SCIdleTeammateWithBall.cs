using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdleTeammateWithBall : SCTeammateBallCarrierState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anim.ResetAllTriggers();
        thisChar.agent.isStopped = true;
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder.team != thisChar.team) {
            thisChar.Idle();
        }
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
