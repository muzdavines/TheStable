using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShoot : SCBallCarrierState
{
    float shotAdjustmentMod = 3f;
    Vector3 adjustment;
    float error;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("ShootBall");
        thisChar.transform.LookAt(thisChar.enemyGoal.transform.position);
        RaycastHit frontRay;
        adjustment = Vector3.zero;
        if (Physics.Raycast(thisChar.position, thisChar.transform.forward, out frontRay)) {
            if (frontRay.collider.GetComponent<StableCombatChar>() != null) {
                StableCombatChar passTarget = thisChar.GetFarthestTeammateNearGoal();  //view blocked, look for someone to pass to
                if (passTarget != null) {
                    thisChar.Pass(passTarget);  //found a pass target - pass it to them
                    return;
                }
                //Didn't find a pass target - shoot for a corner
                adjustment = thisChar.enemyGoal.transform.right * Mathf.Sign(Random.value - .5f) * shotAdjustmentMod;
            }
        }

        error = 2 - thisChar.myCharacter.shooting * .01f;
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
            thisChar.ball.Shoot(thisChar.enemyGoal.transform.position + adjustment , error, 1);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
