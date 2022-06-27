using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCExecute : SCCombatStanceState, CannotInterrupt, CannotTarget {

    public StableCombatChar victim;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (victim == null || victim.isKnockedDown) {
            thisChar.Idle();
            return;
        }
        thisChar.ReleaseAttackers();
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(victim.position);
        thisChar.transform.position = victim.position - victim.transform.forward;
        thisChar.transform.LookAt(victim.transform);
        thisChar.anima.Execute();
        victim.ExecuteVictim();
        thisChar.DisplaySpecialAbilityFeedback("Execute");
        thisChar.RHMWeapon.gameObject.SetActive(true);
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!damageDelivered) {
            thisChar.transform.position = victim.position - victim.transform.forward;
            thisChar.transform.LookAt(victim.transform);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "ExecuteDamage") {
            victim.TakeDamage(new StableDamage() { balance = 0, health = 0, mind = 0, stamina = 0, isKnockdown = false }, thisChar, false);
        }
        if (message == "ExecuteDamageFinal") {
            damageDelivered = true;
            victim.TakeDamage(new StableDamage() { balance = 1000, health = 1000, mind = 1000, stamina = 1000, isKnockdown = false }, thisChar, false);
            victim.TakeDamage(new StableDamage() { balance = 1000, health = 1000, mind = 1000, stamina = 1000, isKnockdown = true }, thisChar);
        }
    }
   
    public override void WillExit() {
        base.WillExit();
        thisChar.RHMWeapon.gameObject.SetActive(false);
    }
}
