using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatStanceState : StableCombatCharState
{
    public float timeOut;
    public bool shouldFaceTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        timeOut = Time.time + 8f;
    }
    public virtual bool CheckExitCombatStance() {
        return false;
    }

    public override void Update() {
        base.Update();
        if (shouldFaceTarget && thisChar.myAttackTarget !=null) {
            thisChar._t.LookAt(thisChar.myAttackTarget.position);
        }
        if (Time.time >= timeOut) {
            thisChar.Idle();
        }
    }

    public StableCombatChar GetCombatTarget() {
        float closest = Mathf.Infinity;
        StableCombatChar returnChar = null;
        foreach (Collider c in Physics.OverlapSphere(thisChar.position, thisChar.aggroRadius)) {
            var tempChar = c.GetComponent<StableCombatChar>();
            if (tempChar == null || tempChar.team == thisChar.team || tempChar.isKnockedDown || tempChar.isCannotTarget || tempChar.Distance(thisChar) > closest) {
                continue;
            }
            returnChar = tempChar;
            closest = tempChar.Distance(thisChar);
        }
        return returnChar;
    }
    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "BeginFaceTarget") {
            shouldFaceTarget = true;
        }
        if (message == "EndFaceTarget") {
            shouldFaceTarget = false;
        }
    }
}

public static class SCCombatHelers {

}
