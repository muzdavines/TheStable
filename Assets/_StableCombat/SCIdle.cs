using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIdle : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
        if (ball.holder == thisChar) { thisChar.IdleWithBall(); return; }
        if (thisChar.ShouldPursueBall()) { thisChar.PursueBall(); return; }
        if (thisChar.ShouldPursueBallCarrier()) { thisChar.PursueBallCarrier(); return; }


        thisChar.agent.isStopped = true;

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
