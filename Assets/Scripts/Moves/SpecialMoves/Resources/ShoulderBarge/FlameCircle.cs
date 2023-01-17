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
    public override string GetName() {
        return "Flame Circle";
    }

    public override string GetDescription() {
        return "Casts a circle of fire around the hero, damaging all enemies within its range while the circle is active.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15 || _char.myAttackTarget == null || _char.Distance(_char.myAttackTarget) > 5) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}
