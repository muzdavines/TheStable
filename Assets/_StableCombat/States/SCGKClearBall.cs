using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKClearBall : SCGKState {
    float shotAdjustmentMod = 10f;
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
            RaycastHit frontRay;
            Vector3 adjustment = Vector3.zero;

            if (Physics.Raycast(thisChar.position, thisChar.transform.forward, out frontRay)) {
                if (frontRay.collider.GetComponent<StableCombatChar>() != null) {
                    adjustment = thisChar.enemyGoal.transform.right * Mathf.Sign(Random.value - .5f) * shotAdjustmentMod;
                }
            }

            float error = 2 - thisChar.myCharacter.shooting * .01f;
            thisChar.ball.Shoot(thisChar.enemyGoal.transform.position + adjustment + new Vector3(0,30,0), error, 1);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}