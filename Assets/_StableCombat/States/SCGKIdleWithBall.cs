using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKIdleWithBall : SCGKState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.ResetAllTriggers();
        //thisChar.anim.SetTrigger("OverrideToIdle");
        thisChar.anima.Idle();
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
