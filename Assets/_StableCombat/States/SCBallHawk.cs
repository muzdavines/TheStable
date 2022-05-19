using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBallHawk : StableCombatCharState {
    SCSpeedBuff speedBuff;
    public StableCombatChar catchTarget;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        speedBuff = thisChar.SpeedBuff(10, 8);
        GameObject effect = GameObject.Instantiate(Resources.Load<GameObject>("ClosingSpeedEffect"));
        thisChar.DisplaySpecialAbilityFeedback("Ball Hawk");
        thisChar.agent.velocity = Vector3.zero;
        effect.transform.parent = thisChar.transform;
        effect.transform.localPosition = Vector3.zero;
        ballHawked = false;
        GameObject.Destroy(effect, 8f);
    }

    private bool ballHawked;

    public override void Update() {
        base.Update();
        if (ballHawked) {
            return;
        }

        if (ball.passTargetPosition == Vector3.zero) {
            thisChar.Idle();
            return;
        }
        if (Vector3.Distance(thisChar.position, ball.transform.position) <= 4f) {
            thisChar.agent.isStopped = true;
            thisChar.agent.velocity = Vector3.zero;
            var cols = Physics.OverlapSphere(thisChar.position, 5);
            foreach (var col in cols) {
                StableCombatChar c = col.GetComponent<StableCombatChar>();
                if (c != null && c.team != thisChar.team) {
                    c.FailStrip();
                }
            }

            ballHawked = true;
            thisChar.PickupBall(true);
            return;
            thisChar.ball.holder.TakeDamage(new StableDamage() { balance = 5, health = 1 }, thisChar, false);
            thisChar.ball.holder.GetTackled();
            thisChar.myCharacter.xp += Game.XPTackle;
            if (speedBuff != null) {
                speedBuff.EndEffect();
            }

            return;
        }

        thisChar.agent.SetDestination(ball.passTargetPosition);
        thisChar.agent.isStopped = false;
        ///if in range, try tackle. Might need messaging here, or just fire it. for now just do 50/50, pass tackle, fail animate a fall and stop the tackler for a bit
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        if (speedBuff != null) {
            speedBuff.EndEffect();
        }

        base.WillExit();
    }

    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        return null;
    }
}