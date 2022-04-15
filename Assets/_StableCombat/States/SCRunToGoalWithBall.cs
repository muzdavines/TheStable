using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRunToGoalWithBall : SCBallCarrierState
{
    float enterTime;
    SCSpeedBuff speedBuff;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        enterTime = Time.time;
        thisChar.agent.isStopped = false;
        //thisChar.agent.speed = thisChar.myCharacter.runspeed * .33f;
        speedBuff = thisChar.SpeedBuff(8, -0.825f);
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position + thisChar.enemyGoal.transform.forward * 5 + thisChar.enemyGoal.transform.right * (Random.value - .5f));
        canGrabBall = false;
        //Debug.Log("Check for one timer here");
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
            return;
        }
        thisChar.agent.isStopped = false;
        if (thisChar.ShouldShoot()) {
            thisChar.Shoot();
            return;
        }
        thisChar.SendTeammateOnRun();
        //if (Time.time < enterTime + Random.Range(0,2f)) { return; }
        if (thisChar.ShouldPass()) {
            var passTarget = thisChar.GetPassTarget();
            if (passTarget != null) {
                thisChar.Pass(passTarget);
            }
            return;
        }
        
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    
    public override void WillExit() {
        if (speedBuff != null) { speedBuff.EndEffect(); }
        base.WillExit();
    }
}
