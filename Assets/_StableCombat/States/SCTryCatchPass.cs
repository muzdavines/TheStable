using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTryCatchPass : StableCombatCharState
{
    Vector3 targetPos;
    float entryTime;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        canGrabBall = true;
        targetPos = ball.passTargetPosition;
        entryTime = Time.time;
    }
    public override void Update() {
        base.Update();
        targetPos = ball.passTargetPosition;
        thisChar.agent.SetDestination(targetPos);
        if (Vector3.Distance(thisChar.transform.position, targetPos) < .25f) {
            thisChar.agent.isStopped = true;
        } else {
            thisChar.agent.isStopped = false;
        }
        if (thisChar.enemyGoal.Distance(thisChar) < 20) {
            if (ball.Distance(thisChar) < 3) {
                thisChar.OneTimerToGoal();
                return;
            }
        }
        //check distance to goal for onetimer
        //if close then check distance of ball
        //if ball is close go to one-Time state

        //create one-timer state with BallCollision override to launch into net
        //if (Time.time >= entryTime + .5f && (ball.isHeld || Vector3.Distance(thisChar.transform.position, targetPos) < .25f)) {
          //  thisChar.Idle();
        //}
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
