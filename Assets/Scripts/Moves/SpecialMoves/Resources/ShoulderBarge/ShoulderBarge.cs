using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderBarge : ActiveSpecialMove
{
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.ShoulderBarge();
    }

    public override string GetName() {
        return "Shoulder Barge";
    }

    public override string GetDescription() {
        return "Boosts speed while carrying the ball, making the hero unable to be tackled while making a run towards the goal";
    }

    public override bool Check(StableCombatChar _char) {
        if (_char.playStyle != PlayStyle.Fight) { return false; }
        if (Time.time <= lastFired + 15 || _char.Distance(_char.myAttackTarget) > 5) { 
            return false;
        }
        OnActivate(_char);
        return true;
    }
}
