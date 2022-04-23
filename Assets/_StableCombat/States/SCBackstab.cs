using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBackstab : SCCombatStanceState, CannotInterrupt {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.myAttackTarget == null || thisChar.myAttackTarget.isKnockedDown) {
            thisChar.Idle();
            return;
        }
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(thisChar.myAttackTarget.position);
        GameObject effect = Resources.Load<GameObject>("SmokeEffect");
        var fx1 = GameObject.Instantiate(effect, thisChar.position, thisChar.transform.rotation);
        var fx2 = GameObject.Instantiate(effect, thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward, thisChar.transform.rotation);
        GameObject.Destroy(fx1, 10);
        GameObject.Destroy(fx2, 10);
        thisChar.transform.position = thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward;
        thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        thisChar.anima.Backstab();
        thisChar.myAttackTarget.BackstabVictim();
        thisChar.DisplaySpecialAbilityFeedback("Backstab");
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.transform.position = thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward;
        thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "BackstabDamage") {
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 1, health = 2, mind = 1, stamina = 8, isKnockdown = true });
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
