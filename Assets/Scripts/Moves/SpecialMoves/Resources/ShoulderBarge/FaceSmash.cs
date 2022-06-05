using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSmash : ActiveSpecialMove {
    float lastFired;
    StableCombatChar enemy;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.FaceSmash(enemy);
    }

    public override bool Check(StableCombatChar _char) {

        if (Time.time <= lastFired + 15) { return false; }
        if (_char.isKnockedDown) { return false; }

        Ball ball = _char.ball;
        if (ball.holder == null || ball.holder != _char) {
            return false;
        }
        StableCombatChar _enemy = _char.GetNearestEnemy(0,5f);
        if (_enemy.isKnockedDown || _enemy.isCannotTarget) { return false; }
        if (_enemy != null) {
            enemy = _enemy;
            OnActivate(_char);
        }
        return true;
    }

}