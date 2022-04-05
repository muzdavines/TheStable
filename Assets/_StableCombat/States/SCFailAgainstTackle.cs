using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SCFailAgainstTackle : SCBallCarrierState {

    public StableCombatChar tackler;
    public float angle;
    public abstract void FireAnimation();
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        checkForIdle = true;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.agent.destination = thisChar.transform.position;
        
        if (ball.holder != null && ball.holder == thisChar) {
            ball.GetDropped();
        }
        FireAnimation();
        thisChar.SetTackleCooldown();
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
