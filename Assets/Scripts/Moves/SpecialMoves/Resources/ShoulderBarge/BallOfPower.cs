using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOfPower : ActiveSpecialMove {
    float lastFired;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.BallOfPower();
    }
    public override string GetName() {
        return "Ball of Power";
    }

    public override string GetDescription() {
        return "Imbues the ball with Holy Power. If the Wizard passes an imbued ball to a teammate, upon reception the teammate has increased stats (+50%) for 7s.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time < lastFired + 10) {
            return false;
        }

        if (_char.ball == null || _char.ball.holder == null || _char.ball.holder != _char) {
            return false;
        }

        if (_char.ball.GetComponent<BallOfPowerEffect>()) {
            return false;
        }
        OnActivate(_char);
        return true;
    }

}