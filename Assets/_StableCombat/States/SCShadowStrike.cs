using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShadowStrike : SCCombatStanceState, CannotInterrupt, CannotTarget {
    public  StableCombatChar target;
    public ShadowStrike myShadowStrike;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.myAttackTarget == null || thisChar.myAttackTarget.isKnockedDown) {
            thisChar.Idle();
            return;
        }
        thisChar.ReleaseAttackers();
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
        thisChar.anima.ShadowStrike();
        thisChar.myAttackTarget.ShadowStrikeVictim();
        thisChar.DisplaySpecialAbilityFeedback("Shadow Strike");
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "ShadowStrikeDamage1") {
           
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 50, health = 0, mind = 0, stamina = 0, isKnockdown = false }, thisChar, false);
           Debug.Log("#ShadowStrike#Damage1"); 
        }

        if (message == "ShadowStrikeDamage2") {
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 0, health = 1000, mind = 0, stamina = 0, isKnockdown = false }, thisChar, false);
            Debug.Log("#ShadowStrike#Damage2 "+thisChar.myAttackTarget.health);
            if (thisChar.myAttackTarget.health <= 0) {
                if (myShadowStrike != null) {
                    myShadowStrike.lastFired = 0;
                }
            }
        }

        if (message == "FaceTarget") {
            thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        }
    }
   
    public override void WillExit() {
        base.WillExit();
        
    }
}
