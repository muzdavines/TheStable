using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKDefendNet : SCGKState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        //thisChar.anim.SetTrigger("OverrideToIdle");
        thisChar.anima.Idle();
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = false;
        float ballDist = thisChar.ball.Distance(thisChar);
        if (ballDist>30) {
            return;
        }
        if (ball.holder == null) {
            if (ballDist < 10) {
                thisChar.GKPursueBall();
                return;
            }
        }
        if (Time.frameCount % 10 != 0) { return; }
        Vector3 a = thisChar.myGoal.transform.position;
        Vector3 b = ball.transform.position;

        Vector3 targetPos = a + (b - a).normalized * 3f;
        thisChar.agent.SetDestination(targetPos);
        //thisChar.transform.LookAt(b);
       // thisChar.transform.rotation = Quaternion.Euler(0, thisChar.transform.rotation.y, 0);
        
        if (ball.holder == null || ball.holder.team == thisChar.team) {
            thisChar.Idle();
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
