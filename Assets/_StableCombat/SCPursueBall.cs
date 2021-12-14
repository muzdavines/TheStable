using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPursueBall : StableCombatCharState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
    }
    public override void Update() {
        base.Update();
        if (ball.isHeld) { thisChar.Idle(); return; }
        thisChar.agent.SetDestination(ball.transform.position);
        if (ball.Distance(thisChar) < .6f) {
            thisChar.PickupBall();
            return;
        }
    }
    
    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
