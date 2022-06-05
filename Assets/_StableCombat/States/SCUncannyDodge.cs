using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCUncannyDodge : StableCombatCharState, CannotTarget, CannotSpecial {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        FireAnimation();
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        checkForIdle = true;
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
    public void FireAnimation() {
        thisChar.DisplaySpecialAbilityFeedback("Uncanny Dodge");
        thisChar.anima.UncannyDodge();
    }
}