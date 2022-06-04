using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCKneecapAttack : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("Tackle");
        thisChar.anima.Kneecap();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
        canGrabBall = false;
        checkForIdle = true;

    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        thisChar.anim.applyRootMotion = false;
        thisChar.SetTackleCooldown();
        base.WillExit();
    }
}