using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFlurry : ActiveSpecialMove {
    float lastFired;
    private StableCombatChar target;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.SwordFlurry(target);
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time < lastFired + 20) {
            return false;
        }

        foreach (StableCombatChar c in _char.coach.otherTeam) {
            if (c == null || c.isKnockedDown || c.isCannotTarget || _char.ball.holder == c || c.Distance(_char) > 10) {
                continue;
            }
            else {
                target = c;
                Debug.Log("#SwordFlurry#Activate");
                OnActivate(_char);
                return true;
            }
        }

        return false;

    }
}