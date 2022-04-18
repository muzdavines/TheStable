using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTryCatchPass : StableCombatCharState
{
    

    Vector3 targetPos;
    float entryTime;
    float arrivedTime = Mathf.Infinity;
    float lastBallDist;
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
        if (CheckOneTimer()) {
            return;
        }
        if (Vector3.Distance(thisChar.transform.position, targetPos) < .1f) {

            var thisballDist = ball.Distance(thisChar);
            thisChar.agent.isStopped = true;
            thisChar.agent.velocity = Vector3.zero;
            arrivedTime = Time.time;
            thisChar.transform.LookAt(Vector3.Project(ball.transform.position, Vector3.up));
            if (thisballDist > lastBallDist || thisballDist < 1.5f) {
                thisChar.PursueBall();
                return;
            }
            lastBallDist = thisballDist;
        } else {
            thisChar.agent.isStopped = false;

        }
        
        
        if (Time.time > entryTime + 5f || Time.time > arrivedTime + 2f)
        {
            thisChar.Idle();
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
