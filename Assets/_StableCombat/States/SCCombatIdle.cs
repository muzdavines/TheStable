using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatIdle : SCCombatStanceState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        switch (thisChar.combatFocus) {
            case CombatFocus.Melee:
                thisChar.attackRange = 1.2f;
                break;
            case CombatFocus.Ranged:
                thisChar.attackRange = 50f;
                break;
        }
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        StableCombatChar thisTarget;
        if (thisChar.myAttackTarget == null) {
             thisTarget = GetCombatTarget();
        } else { thisTarget = thisChar.myAttackTarget; }

        if (thisTarget != null) {
            thisChar.myAttackTarget = thisTarget;
            thisChar._t.LookAt(thisTarget.position);
            if (thisChar.IsCoolingDown()) {
                //is cooling down and target not null
                if (thisChar.MyTargetIsInAttackRange()) {
                    if (thisChar.Distance(thisChar.myAttackTarget)<.8f) {
                        thisChar.agent.SetDestination(thisChar.position + thisChar._t.forward * -1.5f);
                        thisChar.agent.isStopped = false;
                    }
                }
                else {
                    thisChar.CombatPursueTarget();
                }
            }
            else {
                //is not cooling down and target not null
                if (thisChar.MyTargetIsInAttackRange()) {
                    thisChar.CombatAttack();
                }
                else {
                    thisChar.CombatPursueTarget();
                }
            }
        } else {
            //target is null - figure out what else to do. maybe look to heal or lay a trap, or exit combat. Or look at other allies and see if something reveals itself
            //or perhaps look for a POI
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
