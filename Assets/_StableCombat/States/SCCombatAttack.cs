using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatAttack : SCCombatStanceState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.FireBaseAttackMoves();
        thisChar.agent.isStopped = true;
        thisChar._t.LookAt(thisChar.myAttackTarget.position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        thisChar.MeleeScanDamage(message);
        if (message == "EndAttack") {
            //thisChar.anima.FireNextBaseAttackMove();
        }
        if (message == "RangedShot") {

        }
        if (message == "FaceTarget") {
            thisChar._t.LookAt(thisChar.myAttackTarget.position);
        }
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.lastAttack = Time.time;
        thisChar.MeleeScanDamage("EndAll");
    }
}
