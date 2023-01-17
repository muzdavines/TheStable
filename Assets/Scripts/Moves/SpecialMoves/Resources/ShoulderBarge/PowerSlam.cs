using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSlam : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.PowerSlam();
    }
    public override string GetName() {
        return "Power Slam";
    }

    public override string GetDescription() {
        return "Slams the ground, producing a shockwave that knocks down all enemies in range.";
    }
    public override bool Check(StableCombatChar _char) {
        //if (_char.playStyle != PlayStyle.Fight) { return false; }
        if (_char.myAttackTarget == null) { return false; }
        if (Time.time <= lastFired + 30 || _char.Distance(_char.myAttackTarget) > 5) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}
