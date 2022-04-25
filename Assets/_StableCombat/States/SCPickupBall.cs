using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPickupBall : StableCombatCharState
{
    bool stopToPickup;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ball.PickupBall(thisChar)) {
            //thisChar.anim.SetTrigger("Catch");
            thisChar.anima.CatchBall();
            thisChar.agent.SetDestination(thisChar._t.forward * 15);
            if (thisChar.agent.velocity.magnitude > 0) {
                stopToPickup = false;
            } else {
                stopToPickup = true;
            }
            thisChar.agent.isStopped = false;
            //thisChar.IdleWithBall();
            canGrabBall = false;
            foreach (var c in Physics.OverlapSphere(thisChar.position, 3)) {
                var nearChar = c.GetComponent<StableCombatChar>();
                if (nearChar!=null && nearChar.team != thisChar.team) {
                    Debug.Log("#Backoff# Fired");
                    nearChar.BackOffCarrier(true);
                }
            }
        }
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = false;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
