using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatIdle : SCCombatStanceState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        Debug.Log("#SCCombatStance#Combat Idle Enter "+thisChar.myCharacter.name);
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        if (thisChar.fieldSport) {
            if (thisChar.fieldPosition == Position.GK) { thisChar.GKIdle(); return; }
            if (thisChar == ball.holder) {
                thisChar.RunToGoalWithBall();
                return;
            }
            if (ball.Distance(thisChar) < 20) {
                thisChar.Idle();
                return;
            }
        }
        
        if (thisChar.myAttackTarget != null && thisChar.myAttackTarget.isKnockedDown) {
            thisChar.myAttackTarget = null;
            thisChar.Idle();
            return;
        }
        
        switch (thisChar.combatFocus) {
            case CombatFocus.Melee:
                thisChar.attackRange = 1.2f;
                thisChar.MeleeWeaponsOn();
                break;
            case CombatFocus.Ranged:
                thisChar.attackRange = 50f;
                thisChar.RangedWeaponsOn();
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
            if (thisChar.myAttackTarget?.state.GetType() == typeof(SCCombatDowned)) {
                thisChar.myAttackTarget = null;
                thisChar.Idle();
            }
            thisChar.myAttackTarget = thisTarget;
            thisChar._t.LookAt(thisTarget.position);
            if (thisChar.IsCoolingDown()) {
                //is cooling down and target not null
                if (thisChar.MyTargetIsInAttackRange()) {
                    if (thisChar.Distance(thisChar.myAttackTarget)<(thisChar.combatFocus == CombatFocus.Melee ? .8f : 5f)) {
                        thisChar.agent.SetDestination(thisChar.position + thisChar._t.forward * (thisChar.combatFocus == CombatFocus.Melee ? -1.5f : -20f));
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
