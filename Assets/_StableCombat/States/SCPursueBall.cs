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

        // Rigidbody body = ball.body;
        Vector3 adjust = ball.body.velocity * Time.deltaTime * ball.body.velocity.magnitude;
        adjust.Scale(new Vector3(1, 0, 1));
        thisChar.agent.SetDestination(ball.transform.position + adjust);
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
