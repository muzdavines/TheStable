using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSwordFlurry: StableCombatCharState, CannotSpecial, CannotTarget {

    public StableCombatChar victim;
    private bool shouldFaceTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (victim == null || victim.isKnockedDown) {
            thisChar.Idle();
                return;
        }

        thisChar.agent.TotalStop();
        victim.SwordFlurryVictim(thisChar);
        thisChar._t.LookAt(victim.position);
        thisChar.RHMWeapon.gameObject.SetActive(true);
    }

    private bool attackFired = false;
    IEnumerator DelayIdle() {
        yield return new WaitForSeconds(1.5f);
        thisChar.Idle();
        thisChar.RHMWeapon.gameObject.SetActive(false);
    }
    bool notActive;
    public override void Update() {
        if (notActive) {
            return;
        }
        base.Update();
        if (victim == null || victim.isKnockedDown) {
            notActive = true;
            thisChar.StartCoroutine(DelayIdle());
            return;
        }
        if (thisChar.Distance(victim) > 1.5f) {
            thisChar.agent.SetDestination(victim.position);
            thisChar.agent.isStopped = false;
        }
        else {
            thisChar.agent.TotalStop();
            if (!attackFired) {
                attackFired = true;
                thisChar.anima.SwordFlurry();
            }
        }

        if (shouldFaceTarget && victim != null) {
            thisChar._t.LookAt(victim.position);
        }
        
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        thisChar.MeleeScanDamage(message);
        Debug.Log("#SwordFlurry#Message: "+message);
        if (message == "BeginFaceTarget") {
            shouldFaceTarget = true;
        }
        if (message == "EndFaceTarget") {
            shouldFaceTarget = false;
        }
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.RHMWeapon.gameObject.SetActive(false);
    }   
}
