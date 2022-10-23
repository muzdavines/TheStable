using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulSteal : ActiveSpecialMove {
    float lastFired;
    StableCombatChar enemy;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.SoulSteal(enemy);
    }

    public override bool Check(StableCombatChar _char) {

        if (!_char.fieldSport || Time.time <= lastFired + 20) { return false; }
        enemy = _char.GetNearestEnemy(20);
        Debug.Log("#SoulSteal#" + (enemy ? enemy.name : "null"));
        if (enemy) {
            OnActivate(_char);
            return true;
        }
        return false;
    }

}
