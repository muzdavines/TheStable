using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBackstabVictim : StableCombatCharState, CannotInterrupt {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.anima.BackstabVictim();
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}