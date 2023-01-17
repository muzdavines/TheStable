using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingSpeed : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.ClosingSpeed();
    }
    public override string GetName() {
        return "Closing Speed";
    }

    public override string GetDescription() {
        return "Boosts speed while chasing a ball carrier, delivering a critical tackle if the hero gets in range.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15) { return false; }
        if (_char.isKnockedDown) { return false; }
        Ball ball = _char.ball;
        if (ball?.holder == null || ball.holder.team == _char.team || ball.Distance(_char) > 10 || ball.Distance(_char) < 4) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}