using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdleTeammateWithBall : SCTeammateBallCarrierState
{
    Vector3 goalOffset = Vector3.zero;
    
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        if (thisChar.enemyGoal.Distance(thisChar) < 200) {
            goalOffset = new Vector3(Random.Range(-6, 6), 0, Random.Range(-2, 5));
        }
    }
    public override void Update() {
        base.Update();
        if (goalOffset != Vector3.zero) {
            thisChar.agent.isStopped = false;
            thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position + (thisChar.enemyGoal.transform.forward * 10) + goalOffset);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {

        Debug.Log("#Message#SCIdleTeammatewithBall " + message);
            return base.ReceiveMessage(sender, message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
