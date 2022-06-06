using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPursueBall : StableCombatCharState
{
    float startTime;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        canGrabBall = true;
        startTime = Time.time;
    }
    public override void Update() {
        base.Update();
        if (ball.isHeld) { thisChar.Idle(); return; }
        if (CheckOneTimer()) {
            return;
        }
        
        Rigidbody body = ball.body;
        thisChar.agent.SetDestination(ball.transform.position + (body.velocity.magnitude > 8 && ball.passTargetPosition == Vector3.zero ? ball.body.velocity : Vector3.zero));
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
