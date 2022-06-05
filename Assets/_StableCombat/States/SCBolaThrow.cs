using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBolaThrow : SCCombatStanceState, CannotTarget {
    GameObject weapon;
    public StableCombatChar target;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (target == null) { Debug.Log("#BolaThrow#Idle"); thisChar.Idle(); }
        thisChar.ReleaseAttackers();
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(target.position);
        GameObject effect = Resources.Load<GameObject>("SmokeEffect");
        var fx1 = GameObject.Instantiate(effect, thisChar.position, thisChar.transform.rotation);
        //var fx2 = GameObject.Instantiate(effect, thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward, thisChar.transform.rotation);
        GameObject.Destroy(fx1, 10);
        //GameObject.Destroy(fx2, 10);

        thisChar.transform.LookAt(target.transform);
        thisChar.anima.BolaThrow();
        GameObject weaponPrefab = Resources.Load<GameObject>("BolaWeapon");
        weapon = GameObject.Instantiate<GameObject>(weaponPrefab);
        weapon.transform.parent = thisChar._rightHand;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.transform.localScale = Vector3.one;
        DisplayWeapon(false);
        thisChar.DisplaySpecialAbilityFeedback("Bola Throw");
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!damageDelivered) {
            thisChar.transform.LookAt(target.transform);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "BolaDamage") {
            damageDelivered = true;
            target.DisplaySpecialAbilityFeedback("Bola Knockdown from " + thisChar.myCharacter.name);
            target.TakeDamage(new StableDamage() { balance = 10, health = 0, mind = 0, stamina = 0, isKnockdown = true }, thisChar);
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