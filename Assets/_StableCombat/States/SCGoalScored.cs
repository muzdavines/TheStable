using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGoalScored : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        thisChar.anima.GoalScored();
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