using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatStanceState : StableCombatCharState
{
    public float timeOut;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        timeOut = Time.time + 8f;
    }
    public virtual bool CheckExitCombatStance() {
        return false;
    }

    public override void Update() {
        base.Update();
        if (Time.time >= timeOut) {
            thisChar.Idle();
        }
    }

    public StableCombatChar GetCombatTarget() {
        float closest = Mathf.Infinity;
        StableCombatChar returnChar = null;
        foreach (Collider c in Physics.OverlapSphere(thisChar.position, thisChar.aggroRadius)) {
            var tempChar = c.GetComponent<StableCombatChar>();
            if (tempChar == null || tempChar.team == thisChar.team || tempChar.state.GetType() == typeof(SCCombatDowned) || tempChar.Distance(thisChar) > closest) {
                continue;
            }
            returnChar = tempChar;
            closest = tempChar.Distance(thisChar);
        }
        return returnChar;
    }
}

public static class SCCombatHelers {

}
