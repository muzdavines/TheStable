using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BallHawk : ActiveSpecialMove {
    float lastFired;
    
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.BallHawk();
    }
    public override string GetName() {
        return "Ball Hawk";
    }

    public override string GetDescription() {
        return "Any balls passed to opponents near the Wizard are chased down and intercepted. 5s cooldown.";
    }

    public override bool Check(StableCombatChar _char) {
        if (!_char.fieldSport) {
            return false;}
        if (Time.time <= lastFired + 5) { return false; }
        if (_char.isKnockedDown) { return false; }

        Ball ball = _char.ball;
        if (ball.holder?.team == _char.team) {
            return false;
        }

        if (ball.lastHolder?.team == _char.team) {
            return false;
        }
        if (ball.passTargetPosition == Vector3.zero) {
            return false;
        }

        if (Vector3.Distance(ball.passTargetPosition, _char.position) < 20) {
            Debug.Log("#BallHawk#Fire");
            OnActivate(_char);
        }

        return true;
    }

}