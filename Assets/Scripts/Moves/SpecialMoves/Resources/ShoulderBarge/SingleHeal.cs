using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleHeal : ActiveSpecialMove, HealingMove {
    float lastFired;
    StableCombatChar target;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.SingleHeal(target);
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 5) { return false; }
        if (_char.isKnockedDown) { return false; }
        var chars = GameObject.FindObjectsOfType<StableCombatChar>();

        foreach (var scc in chars) {
            if(scc == _char)
            {
                continue;
            }
            if (_char.team != scc.team) {
                continue;
            }
            if (scc.isKnockedDown) {
                continue;
            }

            if (scc.stamina / scc.maxStamina > .5f && scc.mind / scc.maxMind > .5f &&
                scc.balance / scc.maxBalance > .5f) {
                continue;
            }
            if (_char.Distance(scc) <= 30) {
                target = scc;
                break;
            }
        }
        
        if (target != null) {
            Debug.Log("#SingleHeal#Activate");
            OnActivate(_char);
            return true;
        }
        else {
            Debug.Log("#SingleHeal#No Target Found");
            return false;
        }

        return false;
    }
}