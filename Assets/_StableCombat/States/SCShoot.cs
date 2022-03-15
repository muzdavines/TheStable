using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShoot : SCBallCarrierState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("ShootBall");
        thisChar.anima.ShootBall();
        thisChar.agent.isStopped = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "Throw") {
            thisChar.transform.LookAt(thisChar.enemyGoal.transform.position);
            float error = 1 - thisChar.myCharacter.shooting * .01f;
            thisChar.ball.Shoot(thisChar.enemyGoal, error, 1);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
