using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPistolShot : SCCombatStanceState, CannotInterrupt, CannotTarget {
    GameObject weapon;
    StableCombatChar target;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);

        thisChar.ReleaseTarget();
        thisChar.myAttackTarget = thisChar.GetNearestEnemy(0, 20);

        if (thisChar.myAttackTarget == null || thisChar.myAttackTarget.isKnockedDown || thisChar.myAttackTarget.team == thisChar.team) {
            thisChar.Idle();
            return;
        }
        thisChar.ReleaseAttackers();
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(thisChar.myAttackTarget.position);
        
        //GameObject.Destroy(fx2, 10);

        thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        Debug.Log("#PistolShot#Animation Called");
        thisChar.anima.PistolShot();
        GameObject weaponPrefab = Resources.Load<GameObject>("PistolWeapon");
        weapon = GameObject.Instantiate<GameObject>(weaponPrefab);
        weapon.transform.parent = thisChar._rightHand;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.transform.localScale = Vector3.one;
        DisplayWeapon(false);
        thisChar.DisplaySpecialAbilityFeedback("Pirate Pistol");
        thisChar.myAttackTarget.DisplaySpecialAbilityFeedback("Pistol Shot by " + thisChar.myCharacter.name);
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!damageDelivered) {

            thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "PistolShotDamage") {
            damageDelivered = true;
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 100, health = 5, mind = 100, stamina = 100, isKnockdown = true }, thisChar);
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 100, health = 5, mind = 100, stamina = 100, isKnockdown = true }, thisChar);
        }
        if (message == "DisplayWeapon") {
            DisplayWeapon(true);
            
        }
        if (message == "HideWeapon") {
            DisplayWeapon(false);
            GameObject.Destroy(weapon);
        }

        if (message == "DisplayEffect") {
            GameObject effect = Resources.Load<GameObject>("PistolShotSmokeEffect");
            var fx1 = GameObject.Instantiate(effect);
            fx1.transform.position = weapon.transform.position;
            fx1.transform.rotation = Quaternion.Euler(new Vector3(-90,0,0));
            GameObject.Destroy(fx1, 10);
        }
    }
    void DisplayWeapon(bool display) {
        weapon.SetActive(display);
    }

    public override void WillExit() {
        base.WillExit();
        if (weapon != null) {
            GameObject.Destroy(weapon);
        }
    }
}