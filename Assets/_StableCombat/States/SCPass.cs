using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPass : SCBallCarrierState
{
    public StableCombatChar passTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (passTarget == null) { thisChar.RunToGoalWithBall(); return; }
        //thisChar.anim.SetTrigger("PassBall");
        thisChar.anima.PassBall();
        thisChar.transform.LookAt(passTarget.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.transform.LookAt(passTarget.transform);
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "PassBall") {
            ball.PassTo(passTarget);
            passTarget.TryCatchPass();
        }
        
    }

    public override void WillExit() {
        base.WillExit();
    }
}
