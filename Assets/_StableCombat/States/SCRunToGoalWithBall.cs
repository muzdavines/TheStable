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
        runTime = 0;
        runTolerance = thisChar.myCharacter.archetype == Character.Archetype.Mercenary ? 2f : 0;
        //Debug.Log("Check for one timer here");
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
            return;
        }
        thisChar.agent.isStopped = false;
        if (ShouldShoot()) {
            thisChar.Shoot();
            return;
        }
        if (ShouldRun()) {
            return;
        }
        if (thisChar.enemyGoal.Distance(thisChar) < 30) {
            //we are near the enemy goal but haven't shot
            PassTargetLogic logic = PassTargetLogic.Rogue;
            logic |= PassTargetLogic.NearGoal;

            var rogueTarget = thisChar.GetPassTarget(logic);
            if (rogueTarget != null) {
                Debug.Log("#PassLogic#Rogue Found");
                thisChar.Pass(rogueTarget);
                return;
            }

        }
        if (thisChar.enemyGoal.Distance(thisChar)> 60) {
            PassTargetLogic logic = PassTargetLogic.Wizard;
            logic |= PassTargetLogic.BackwardOK;
            var wizardTarget = thisChar.GetPassTarget(logic);
            if (wizardTarget != null) {
                Debug.Log("#PassLogic#Wizard Found");
                thisChar.Pass(wizardTarget);
            }
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
