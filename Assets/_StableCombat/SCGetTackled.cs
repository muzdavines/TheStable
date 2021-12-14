using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGetTackled : StableCombatCharState {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.agent.destination = thisChar.transform.position;
        thisChar.anim.SetTrigger("Knockdown");
        ball.GetDropped();
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
