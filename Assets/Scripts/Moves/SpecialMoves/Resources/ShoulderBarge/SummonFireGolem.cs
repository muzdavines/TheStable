using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonFireGolem : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.SummonFireGolem();
    }

    public override bool Check(StableCombatChar _char) {
        if (_char.ball?.holder == null || _char.ball.holder.team == _char.team) { return false; }
        if (_char.myGoal.Distance(_char) > 30) { return false; }
        if (Time.time <= lastFired + Random.Range(45,65)) {
            return false;
        }
        foreach (var familiar in FindObjectsOfType<FireGolemFamiliar>()) {
            if (familiar.owner == _char) {
                lastFired = Time.time;
                return false;
            }
        }
        OnActivate(_char);
        return true;
    }
}