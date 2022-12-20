using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolaThrow : ActiveSpecialMove {
    float lastFired;
    StableCombatChar target;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.BolaThrow(target);
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 10) { return false; }
        if (_char.isKnockedDown) { return false; }
        Ball ball = _char.ball;
        
        if (ball?.holder == null || ball.holder.team != _char.team || ball.Distance (_char) > 30 || ball.holder == _char) {
            return false;
        }
        target = ball.holder.GetNearestEnemy(0,15);
        if (target != null) {
            Debug.Log("#BolaThrow#Activate");
            OnActivate(_char);
        } else {
            Debug.Log("#BolaThrow#No Target Found");
            return false;
        }
        return true;
    }
}