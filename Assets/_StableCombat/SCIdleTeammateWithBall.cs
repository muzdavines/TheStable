using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdleTeammateWithBall : SCTeammateBallCarrierState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
    }
    public override void Update() {
        base.Update();
        thisChar.Block();
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
