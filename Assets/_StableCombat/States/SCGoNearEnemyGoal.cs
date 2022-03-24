using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGoNearEnemyGoal : SCTeammateBallCarrierState
{
    Vector3 goalOffset = Vector3.zero;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.Idle();
        thisChar.agent.isStopped = false;
        goalOffset = new Vector3(Random.Range(-6, 6), 0, Random.Range(-2, 5));
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = false;
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position + (thisChar.enemyGoal.transform.forward * 10) + goalOffset);
        if (thisChar.enemyGoal.Distance(thisChar) < 20) {
            if (ball.Distance(thisChar) < 3) {
                thisChar.OneTimerToGoal();
                return;
            }
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
