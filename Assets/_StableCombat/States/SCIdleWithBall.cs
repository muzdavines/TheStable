using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdleWithBall : SCBallCarrierState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ShouldShoot()) {
            thisChar.Shoot();
            return;
        }
        thisChar.RunToGoalWithBall();
    }
    public override void Update() {
        base.Update();
        if (ShouldShoot()) {
            thisChar.Shoot();
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
