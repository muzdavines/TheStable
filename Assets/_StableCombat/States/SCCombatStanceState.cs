using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatStanceState : StableCombatCharState
{
    public virtual bool CheckExitCombatStance() {
        return false;
    }

    public override void Update() {
        base.Update();
    }

    public StableCombatChar GetCombatTarget() {
        foreach (Collider c in Physics.OverlapSphere(thisChar.position, thisChar.aggroRadius)) {
            var tempChar = c.GetComponent<StableCombatChar>();
            if (tempChar == null || tempChar.team == thisChar.team || tempChar.state.GetType() == typeof(SCCombatDowned)) {
                continue;
            }
            return tempChar;
        }
        return null;
    }
}

public static class SCCombatHelers {

}
