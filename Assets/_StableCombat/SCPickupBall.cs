using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPickupBall : StableCombatCharState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        Debug.Log("CHANGE TO PICKUP ANIMATION");
        if (ball.PickupBall(thisChar)) {
            thisChar.agent.isStopped = true;
            thisChar.IdleWithBall();
        }
        canGrabBall = false;
        
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        CheckIdle(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
