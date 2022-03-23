using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTryCatchPass : StableCombatCharState
{
    Vector3 targetPos;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = false;
        canGrabBall = true;
        targetPos = ball.passTargetPosition;
    }
    public override void Update() {
        base.Update();
        targetPos = ball.passTargetPosition;
        thisChar.agent.SetDestination(targetPos);
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
        if (ball.isHeld || Vector3.Distance(thisChar.transform.position, targetPos) < .25f) {
            thisChar.Idle();
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
