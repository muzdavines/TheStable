using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKSwat : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.GKSwat();
        ball.GetSwatted(thisChar.myGoal);
        thisChar.agent.isStopped = true;
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