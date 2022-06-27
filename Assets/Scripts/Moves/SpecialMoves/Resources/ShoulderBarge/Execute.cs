using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Execute : ActiveSpecialMove {
    float lastFired;
    StableCombatChar victim;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        Debug.Log("#Execute#Activate Execute");
        lastFired = Time.time;
        _char.Execute(victim);
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 20) {
            return false;
        }
        foreach (StableCombatChar c in _char.coach.otherTeam) {
            if (c.isKnockedDown) { continue; }
            if (c.isCannotTarget) { continue; }
            if (c.stamina / c.maxStamina > .5f && c.mind / c.maxMind > .5f && c.balance / c.maxBalance > .5f) { continue; }
            if (c.Distance(_char) > 6) { continue; }
            victim = c;
            OnActivate(_char);
            return true;
        }
        return false;
    }
}
