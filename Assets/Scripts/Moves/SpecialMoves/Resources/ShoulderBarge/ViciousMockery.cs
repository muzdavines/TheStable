using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViciousMockery : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.ViciousMockery(_char.ball.holder);
    }

    public override bool Check(StableCombatChar _char) {
        if (!_char.fieldSport || Time.time <= lastFired + 30) {
            return false;
        }
        Ball ball = _char.ball;
        if (ball.isHeld == false || ball == null || ball.holder == null || ball.holder.team == _char.team || ball.Distance(_char) > 20) {
            return false;
        }
        Debug.Log("#ViciousMockery#Activate");
        OnActivate(_char);
        return true;
    }
}

