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
