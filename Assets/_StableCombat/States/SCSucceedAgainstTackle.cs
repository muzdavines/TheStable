using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SCSucceedAgainstTackle : SCBallCarrierState
{
    public StableCombatChar tackler;
    public float angle;
    public abstract void FireAnimation();
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        angle = Vector3.Angle(tackler.transform.position, thisChar.transform.position - tackler.transform.position);
        Debug.Log("#TODO# Fix this angle to normalized");
        FireAnimation();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
        canGrabBall = false;
        checkForIdle = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

   

    public override void WillExit() {
        thisChar.anim.applyRootMotion = false;
        base.WillExit();
    }
}
