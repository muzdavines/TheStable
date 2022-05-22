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
        Vector3 ballZero = new Vector3(ball.transform.position.x, 0, ball.transform.position.z);
        Vector3 charZero = new Vector3(thisChar.position.x, 0, thisChar.position.z);
        var thisballDist = Vector3.Distance(ballZero, charZero);
        if (Vector3.Distance(thisChar.transform.position, targetPos) < .1f) {
            thisChar.agent.isStopped = true;
            thisChar.agent.velocity = Vector3.zero;
            thisChar.transform.LookAt(Vector3.Project(ball.transform.position, Vector3.up));
            if (thisballDist > lastBallDist) {
                thisChar.PursueBall();
                return;
            }
            lastBallDist = thisballDist;
        } else {
            thisChar.agent.isStopped = false;
        }

        if (Time.time > entryTime + 2 && ball.passTargetPosition == Vector3.zero) {
            thisChar.Idle();
        }
        
        if (ball.holder!=null && ball.holder.team != thisChar.team)
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
