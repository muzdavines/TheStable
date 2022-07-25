using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGoalScored : StableCombatCharState, SCReviveUnit
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        foreach (ActiveSpecialMove m in thisChar.myCharacter.activeSpecialMoves) {
            if (m!=null && m.GetType() == typeof(RallyingCry)) {
                int myScore = thisChar.team == 0 ? thisChar.matchController.homeScore : thisChar.matchController.awayScore;
                int otherScore = thisChar.team == 1 ? thisChar.matchController.homeScore : thisChar.matchController.awayScore;
                if (myScore < otherScore) {
                    m.OnActivate(thisChar);
                } else {
                    (m as RallyingCry).DeactivateAll();
                }
            }
        }
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
