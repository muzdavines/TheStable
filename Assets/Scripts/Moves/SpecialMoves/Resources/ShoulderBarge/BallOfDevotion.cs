using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOfDevotion : ActiveSpecialMove {
    float lastFired;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.BallOfDevotion();
    }

    public override string GetName() {
        return "Ball of Devotion";
    }

    public override string GetDescription() {
        return "Imbues the ball with Power attuned to the Wizard's team. If an enemy is the next player to pick up the ball, the ball emits an explosion that deals 50 Mind damage and knocks the holder down.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time < lastFired + 10) {
            return false;
        }

        if (_char.ball == null || _char.ball.holder == null || _char.ball.holder != _char) {
            Debug.Log("#BallOfDevotion#Not Holder");
            return false;
        }

        if (_char.ball.GetComponent<BallOfDevotionEffect>()) {
            Debug.Log("#BallOfDevotion#Ball Has Effect");
            return false;
        }

        Debug.Log("#BallOfDevotion#Activate");
        OnActivate(_char);
        return true;
    }

}