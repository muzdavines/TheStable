using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKIdle : SCGKState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        //thisChar.anim.SetTrigger("OverrideToIdle");
        thisChar.anima.Idle();
        thisChar.agent.SetDestination(thisChar.GetFieldPosition().position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        if (ball.holder == thisChar) {
            thisChar.GKClearBall();
            return;
        }
        if (ball.holder == null) {
            //Check if GK needs to dive for ball
            //if (DiveForBall)
            if (false)
            {
            thisChar.GKDiveForBall();
                return;
            }

            if (ball.Distance(thisChar.transform)<10){
            thisChar.GKPursueBall();
            return;
            }
            else {
                //ball is loose but far away
                return;
            }
        }
        if (ball.holder.team != thisChar.team){
            //other team def has the ball
            thisChar.GKDefendNet();
            return;
        }
        //our team has the ball
        thisChar.anima.GKBored();
        thisChar.agent.SetDestination(thisChar.GetFieldPosition().position);
        thisChar.agent.isStopped = false;

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
