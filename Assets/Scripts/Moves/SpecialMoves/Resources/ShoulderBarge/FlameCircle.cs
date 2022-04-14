using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCircle : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.FlameCircle();
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15 || _char.Distance(_char.myAttackTarget) > 5) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}
