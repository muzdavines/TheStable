using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatPursueTarget : SCCombatStanceState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
        if (thisChar.myAttackTarget == null) {
            thisChar.CombatIdle();
            return;
        }
        if (Time.frameCount % 20 != 0) {
            return;
        }
        if (thisChar.fieldSport && ball.Distance(thisChar) < 10f) {
            //thisChar.Idle();
            return;
        }
        Vector3 attackTargetPos = thisChar.myAttackTarget._t.position;
        if (Vector3.Distance(thisChar._t.position, attackTargetPos) < thisChar.attackRange) {
            thisChar.CombatIdle();
            return;
        }
        thisChar.agent.SetDestination(attackTargetPos);
        thisChar.agent.isStopped = false;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
