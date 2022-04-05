using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatAttack : SCCombatStanceState
{
    public float timeOut;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        timeOut = Time.time + 8f;
        switch (thisChar.combatFocus) {
            case CombatFocus.Melee:
                thisChar.anima.FireBaseMeleeAttackMoves();
                thisChar.MeleeWeaponsOn();
                break;
            case CombatFocus.Ranged:
                thisChar.anima.FireBaseRangedAttackMoves();
                thisChar.RangedWeaponsOn();
                break;
        }
       
        thisChar.agent.isStopped = true;
        thisChar._t.LookAt(thisChar.myAttackTarget.position);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        if (Time.time >= timeOut) { thisChar.MeleeScanDamage("EndAll"); thisChar.Idle(); }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        thisChar.MeleeScanDamage(message);
        if (message == "EndAttack") {
            //thisChar.anima.FireNextBaseAttackMove();
        }
        if (message == "FireWeaponRH" || message == "FireWeaponLH") {
            Debug.Log("#SCAttack#FireWeapon");
            FireProjectile(message);
        }
        if (message == "FaceTarget") {
            thisChar._t.LookAt(thisChar.myAttackTarget.position);
        }
    }

    public void FireProjectile(string message) {
        if (message == "FireWeaponRH") {
            thisChar.RHRWeapon.FireProjectile(thisChar.anima.currentRangedMove);
        } else {
            thisChar.LHRWeapon.FireProjectile(thisChar.anima.currentRangedMove);
        }
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.MeleeScanDamage("EndAll");
    }
}
