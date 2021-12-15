using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPickupBall : StableCombatCharState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ball.PickupBall(thisChar)) {
            thisChar.anim.SetTrigger("Catch");
            thisChar.agent.isStopped = true;
            thisChar.IdleWithBall();
        }
        canGrabBall = false;
        checkForIdle = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
