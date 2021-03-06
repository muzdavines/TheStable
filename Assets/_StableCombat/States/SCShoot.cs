using UnityEngine;

public class SCShoot : SCBallCarrierState
{
   

    float shotAdjustmentMod = 2.5f;
    Vector3 adjustment;
    float error;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("ShootBall");
        thisChar.transform.LookAt(thisChar.enemyGoal.transform.position);
        RaycastHit frontRay;
        adjustment = Vector3.zero;
        LayerMask layerMask = LayerMask.GetMask("Character");
        if (ball.Distance(thisChar.enemyGoal.transform) > 3 && Physics.SphereCast(thisChar.transform.position + new Vector3(0,1,0) + thisChar.transform.forward, 1f, thisChar.transform.forward, out frontRay, Mathf.Infinity, layerMask)) {
            if (Random.Range(0, 100) > 50) {
                int randID = Random.Range(0, 100000);
                Debug.Log("#ShootRay#" + randID + " " + frontRay.collider.transform.name);
                StableCombatChar collided = frontRay.collider.GetComponent<StableCombatChar>();
                if (collided != null && collided != thisChar) {
                    //view blocked, aim for a corner
                    Vector3 upDownAdjustment = (Random.value - .5f) > 0 ? Vector3.up : Vector3.down;
                    adjustment = thisChar.enemyGoal.transform.right * Mathf.Sign(Random.value - .5f) * shotAdjustmentMod + upDownAdjustment;
                }
            }
        }

        error = Mathf.Clamp(1f - thisChar.myCharacter.shooting * .01f, 0, Mathf.Infinity);
        thisChar.anima.ShootBall();
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "Throw") {
            Vector3 shootTarget = thisChar.enemyGoal.transform.position;
            if (Random.Range(0,100)<thisChar.myCharacter.shooting) {
                thisChar.DisplayShotAccuracy(100);
                shootTarget = thisChar.enemyGoal.topRight.position;
                error = 0;
            }
            else {
                shootTarget += adjustment;
            }
            thisChar.ball.Shoot(shootTarget, error, 1);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
