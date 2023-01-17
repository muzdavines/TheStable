using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kneecapper : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Kneecap();
    }

    public override string GetName() {
        return "Kneecapper";
    }

    public override string GetDescription() {
        return "Delivers a devastating blow to an opponent's knee, knocking them down and injuring them for a time. The affected enemy suffers from reduced movement while injured.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15) { return false; }
        if (_char.isKnockedDown) { return false; }
        Ball ball = _char.ball;
        if (ball?.holder == null || ball.holder.team == _char.team || ball.Distance(_char) > 10) {
            return false;
        }
        OnActivate(_char);
        return true;
    }
}