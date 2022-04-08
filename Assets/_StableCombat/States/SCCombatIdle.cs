using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatIdle : SCCombatStanceState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        Debug.Log("#SCCombatStance#Combat Idle Enter "+thisChar.myCharacter.name);
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        if (thisChar.fieldSport) {
            if (thisChar.fieldPosition == Position.GK) { thisChar.GKIdle(); return; }
            
            /*if (ball.Distance(thisChar) < 20) {
                thisChar.Idle();
                return;
            }*/
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
        if (thisChar.combatEngagementStatus == CombatEngagementStatus.Defender) {
            if (ball?.holder == thisChar) { ball.GetDropped(); }
            ProcessCombat();
            //this whole section deals with a character that was pulled into combat. 
            //We should check if the attacker is down
            //check if the hero can disengage
            //if not, then do combat normally.  Put all of that into a separate method so aggressors and defenders can use it with separate calls

            return;
        }

        //from here on out it's all aggressor logic
        if (thisChar == ball?.holder) {
            thisChar.RunToGoalWithBall();
            return;
        }
        if (thisChar.myAttackTarget == null) {
             thisTarget = GetCombatTarget();
        } else { thisTarget = thisChar.myAttackTarget; }

        if (thisTarget != null) {
            if (thisChar.myAttackTarget?.state.GetType() == typeof(SCCombatDowned)) {
                thisChar.myAttackTarget = null;
                thisChar.Idle();
                return;
            }
            thisChar.myAttackTarget = thisTarget;
            thisChar._t.LookAt(thisTarget.position);
            ProcessCombat();
        } else {
            //target is null - figure out what else to do. maybe look to heal or lay a trap, or exit combat. Or look at other allies and see if something reveals itself
            //or perhaps look for a POI
        }
    }

    void ProcessCombat() {
        if (thisChar.IsCoolingDown()) {
            //is cooling down and target not null
            if (thisChar.MyTargetIsInAttackRange()) {
                if (thisChar.Distance(thisChar.myAttackTarget) < (thisChar.combatFocus == CombatFocus.Melee ? .8f : 5f)) {
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
                if (thisChar.combatEngagementStatus == CombatEngagementStatus.Aggressor && thisChar.myAttackTarget.combatEngagementStatus == CombatEngagementStatus.None) {
                    thisChar.myAttackTarget.DefendCombat(thisChar);
                }
                //need logic here to pull people into combat
                //consider if they are ball carrier or not - drop ball on pull
                //consider whether they dodge or avoid

                thisChar.CombatAttack();
            }
            else {
                thisChar.CombatPursueTarget();
            }
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
        canGrabBall = false;
    }
}
