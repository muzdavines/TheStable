using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGoToPosition : StableCombatCharState {
    Transform myPosition;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        canGrabBall = true;
        myPosition = thisChar.GetFieldPosition();
    }
    public override void Update() {
        base.Update();
        if (ball.isHeld) { thisChar.Idle(); return; }
        else {
            if (thisChar.ShouldPursueBall()) {
                thisChar.PursueBall();
            }
        }
        thisChar.agent.SetDestination(myPosition.position);
        if (Vector3.Distance(thisChar.transform.position, myPosition.position) < 1.5f) {
            thisChar.CombatIdle();
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}