using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCKnockdown : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = true;
        //thisChar.anim.SetTrigger("Knockdown");
        thisChar.anima.Knockdown();
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
