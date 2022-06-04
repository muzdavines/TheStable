using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCAssassinate : SCCombatStanceState, CannotInterrupt, CannotTarget {
    GameObject weapon;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.ball == null) {

        }
        else {
            thisChar.ReleaseTarget();
            thisChar.myAttackTarget = ball.holder;
        }
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
        GameObject effect = Resources.Load<GameObject>("SmokeEffect");
        var fx1 = GameObject.Instantiate(effect, thisChar.position, thisChar.transform.rotation);
        //var fx2 = GameObject.Instantiate(effect, thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward, thisChar.transform.rotation);
        GameObject.Destroy(fx1, 10);
        //GameObject.Destroy(fx2, 10);
        
        thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        thisChar.anima.Assassinate();
        GameObject weaponPrefab = Resources.Load<GameObject>("AssassinateWeapon");
        weapon = GameObject.Instantiate<GameObject>(weaponPrefab);
        weapon.transform.parent = thisChar._rightHand;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.transform.localScale = Vector3.one;
        DisplayWeapon(false);
        thisChar.DisplaySpecialAbilityFeedback("Assassinate");
        thisChar.myAttackTarget.DisplaySpecialAbilityFeedback("Assassinated by " + thisChar.myCharacter.name);
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
        if (message == "AssassinateDamage") {
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