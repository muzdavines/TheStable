using UnityEngine;

public class SCShoot : SCBallCarrierState
{
    float shootAngleMin = 2.8f;

    float shotAdjustmentMod = 2.5f;
    Vector3 adjustment;
    float error;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("ShootBall");
        thisChar.transform.LookAt(thisChar.enemyGoal.transform.position);
        RaycastHit frontRay;
        adjustment = Vector3.zero;

        Vector3 goalForward = thisChar.enemyGoal.transform.TransformDirection(Vector3.forward);
        Vector3 toShooter = (thisChar.position - thisChar.enemyGoal.transform.position).normalized;
        float shootAngle = Vector3.Dot(goalForward, toShooter);
        Debug.Log("#ShootDot#" + shootAngle);

        StableCombatChar passTarget = thisChar.GetFarthestTeammateNearGoal();
        if (shootAngle < shootAngleMin) {
            if (passTarget == null) {
                thisChar.RunToGoalWithBall();
                //Debug.Log("#TODO# Change Run to goal with ball so that the player runs to a spot ahead ofthe goal");
                return;
            } else {
                thisChar.Pass(passTarget);
                return;
            }
        }
        LayerMask layerMask = LayerMask.GetMask("Character");
        if (ball.Distance(thisChar.enemyGoal.transform) > 3 && Physics.SphereCast(ball.transform.position+thisChar.transform.forward, 1f, thisChar.transform.forward, out frontRay, Mathf.Infinity, layerMask)) {
            int randID = Random.Range(0, 100000);
            Debug.Log("#ShootRay#"+randID + " "+ frontRay.collider.transform.name);
            StableCombatChar collided = frontRay.collider.GetComponent<StableCombatChar>();
            if (collided != null && collided != thisChar) {
                //view blocked, look for someone to pass to
                if (passTarget != null) {
                    thisChar.Pass(passTarget);  //found a pass target - pass it to them
                    return;
                }
                Debug.Log("#ShootRay#" + randID + " Adjust Shot");
                //Didn't find a pass target - shoot for a corner

                Vector3 upDownAdjustment = (Random.value - .5f) > 0 ? Vector3.up : Vector3.down;

                adjustment = thisChar.enemyGoal.transform.right * Mathf.Sign(Random.value - .5f) * shotAdjustmentMod + upDownAdjustment;
            }
        }

        error = 2f - thisChar.myCharacter.shooting * .01f;
        thisChar.anima.ShootBall();
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "Throw") {
            thisChar.ball.Shoot(thisChar.enemyGoal.transform.position + adjustment, error, thisChar.myCharacter.strength * .01f);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
