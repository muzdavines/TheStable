using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonVoidspawn : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.SummonVoidspawn();
    }

    public override bool Check(StableCombatChar _char) {
       if (Time.time < lastFired + 25) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}