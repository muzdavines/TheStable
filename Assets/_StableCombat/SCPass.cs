using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPass : StableCombatCharState
{
    StableCombatChar passTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        passTarget = thisChar.GetNearestTeammate();
        if (passTarget == null) { thisChar.RunToGoalWithBall(); return; }
        thisChar.anim.SetTrigger("PassBall");
        thisChar.agent.isStopped = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "PassBall") {
            ball.PassTo(passTarget);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
