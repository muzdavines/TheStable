using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullRush : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.BullRush();
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15) { return false; }
        if (_char.isKnockedDown) { return false; }
        if (_char.state.GetType() == typeof(SCPass)) { return false; }
        Ball ball = _char.ball;
        if (ball?.holder == null || ball.holder != _char || ball.Distance(_char.enemyGoal.transform.position) < 10 || _char.GetNearestEnemy().Distance(_char) > 7) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}