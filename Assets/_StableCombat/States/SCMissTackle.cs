using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCMissTackle : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        //thisChar.anim.SetTrigger("MissTackle");
        thisChar.anima.MissTackle();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        thisChar.anim.applyRootMotion = false;
        base.WillExit();
    }
}