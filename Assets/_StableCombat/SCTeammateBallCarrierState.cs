using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTeammateBallCarrierState : StableCombatCharState
{
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        SCResolution res = new SCResolution();
        if (message == "RunToOpposingGoal") {
            thisChar.RunToGoalWithoutBall();
        }
        return res;
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null || thisChar.ball.holder.team != thisChar.team) {
            thisChar.Idle();
        }
    }
}
