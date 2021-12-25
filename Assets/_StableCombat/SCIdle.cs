using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdle : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anim.ResetAllTriggers();
        thisChar.anim.SetTrigger("OverrideToIdle");
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        if (ball.holder == null) {
            if (thisChar.ShouldPursueBall()) {
                thisChar.PursueBall();
                return;
            } else {
                //nobody has the ball, but I shouldn't get it
                //go to zone on the field
                //or maybe look for someone to fight

                return;
            }
        }
          
        if (ball.holder == thisChar) {
            thisChar.IdleWithBall();
            return;
        }
        if (ball.holder.team == thisChar.team) {
            thisChar.IdleTeammateWithBall();
        } else {
            //enemy must have ball at this point
            if (thisChar.ShouldPursueBallCarrier()) {
                thisChar.PursueBallCarrier();
                return;
            } else {
                //enemy has ball but I shouldn't pursue
                //should I defend my zone or beat someone up
            }
        }
        thisChar.agent.isStopped = true;

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
