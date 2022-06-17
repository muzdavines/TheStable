using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPass : SCBallCarrierState, CannotSpecial
{
    public StableCombatChar passTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (passTarget == null) { thisChar.RunToGoalWithBall(); return; }
        //thisChar.anim.SetTrigger("PassBall");
        thisChar.anima.PassBall();
        thisChar.transform.LookAt(passTarget.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.transform.LookAt(passTarget.transform);
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "PassBall") {
            float dist = Vector3.Distance(ball.transform.position, passTarget.position);
            int passing = thisChar.myCharacter.passing;
            float need = (passing / (dist * 10 / 8)) * 50;
            float roll = Random.Range(0,100);
            bool fail = roll > need;
            if (fail) {
                thisChar.DisplaySpecialAbilityFeedback("BAD PASS! " + (int)roll+ " v. " + (int)need);
            }
            ball.PassTo(passTarget,  Mathf.Min(.3f + thisChar.myCharacter.strength * .1f, 1f), fail);
            passTarget.TryCatchPass();
        }
        
    }

    public override void WillExit() {
        base.WillExit();
    }
}
