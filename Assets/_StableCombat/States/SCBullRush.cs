using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBullRush : SCBallCarrierState, CannotTarget, ShouldHoldBall {
    SCSpeedBuff speedBuff;
    float startTime;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        speedBuff = thisChar.SpeedBuff(5, 5);
        startTime = Time.time;
        thisChar.ReleaseAttackers();
        GameObject effect = GameObject.Instantiate(Resources.Load<GameObject>("ClosingSpeedEffect"));
        thisChar.DisplaySpecialAbilityFeedback("Bull Rush");
        effect.transform.parent = thisChar.transform;
        effect.transform.localPosition = Vector3.zero;
        GameObject.Destroy(effect, 8f);
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
            if (speedBuff != null) { speedBuff.EndEffect(); }
            return;
        }
        foreach (StableCombatChar c in thisChar.coach.otherTeam) {
            if (c.isKnockedDown || c.isCannotTarget) { continue; }
            if (c.Distance(thisChar) < 2.5f) {
                c.GetTackled();
            }
        }
        thisChar.agent.SetDestination(thisChar.enemyGoal.transform.position + thisChar.enemyGoal.transform.forward * 5);
        thisChar.agent.isStopped = false;
        if (ShouldShoot()) {
            thisChar.Shoot();
        }
        if (Time.time >= startTime + 5) {
            thisChar.Idle();
        }
        ///if in range, try tackle. Might need messaging here, or just fire it. for now just do 50/50, pass tackle, fail animate a fall and stop the tackler for a bit
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        
        if (message == "TryTackle") {
            return new SCResolution() { success = false, tackleType = TackleType.Tackle };
        }
        else return null;
    }

    public override void WillExit() {
        if (speedBuff != null) { speedBuff.EndEffect(); }
        base.WillExit();
    }
   
}