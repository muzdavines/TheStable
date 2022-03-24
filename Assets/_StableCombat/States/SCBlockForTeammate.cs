using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBlockForTeammate : SCTeammateBallCarrierState {

    StableCombatChar blockTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = true;
        checkForIdle = false;
    }
    public override void Update() {
        base.Update();
        if (blockTarget == null || blockTarget.isKnockedDown) {
            blockTarget = FindEnemyToBlock();
        } else {
            thisChar.agent.SetDestination(blockTarget.position);
            thisChar.agent.isStopped = false;
            if (thisChar.Distance(blockTarget) <= 1f) {
                var resolution = thisChar.state.SendMessage(blockTarget, "TryBlock");
                bool didBlock = (resolution != null && resolution.success);
                if (didBlock) { thisChar.Tackle(); return; } else { thisChar.MissTackle(); return; }
            }
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
    StableCombatChar FindEnemyToBlock() {
        StableCombatChar toBlock = null;
        float closest = 100000;
        foreach (StableCombatChar check in thisChar.coach.otherTeam) {
            if (check.state.GetType() == typeof(SCKnockdown) || check.state.GetType() == typeof(SCKnockdown)) { continue; }
            if (thisChar.enemyGoal.Distance(thisChar) < thisChar.enemyGoal.Distance(check)) { continue; }
            float thisDist = Vector3.Distance(thisChar.position, check.position);
            if (thisDist < closest) {
                closest = thisDist;
                toBlock = check;
            }
        }
        return toBlock;
    }
}
