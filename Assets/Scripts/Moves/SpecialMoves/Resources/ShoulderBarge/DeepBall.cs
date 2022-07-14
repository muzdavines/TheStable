using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepBall : ActiveSpecialMove {
    float lastFired;
    StableCombatChar passTarget;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.DeepBall(passTarget);
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 15) { return false; }
        Ball ball = _char.ball;
        if (ball?.holder != _char) { return false; }
        
        var target = _char.GetPassTarget(PassTargetLogic.DeepBall);
        if (target==null) {
            return false;
        }
        passTarget = target;
        //send target to passstate
        OnActivate(_char);
        return true;
    }
}