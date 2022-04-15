using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flechettes : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Flechettes();
    }

    public override bool Check(StableCombatChar _char) {
        if (_char.playStyle != PlayStyle.Fight) { return false; }
        if (Time.time <= lastFired + 15 || _char.Distance(_char.myAttackTarget) > 10) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}