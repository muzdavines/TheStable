using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCViciousMockery : SCCombatStanceState, CannotInterrupt, CannotTarget {
    GameObject weapon;
   public  StableCombatChar target;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);

        thisChar.ReleaseTarget();
        

        if (target == null || target.isKnockedDown || target.team == thisChar.team) {
            thisChar.Idle();
            return;
        }
        thisChar.ReleaseAttackers();
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(target.position);

        //GameObject.Destroy(fx2, 10);

        thisChar.transform.LookAt(target.transform);
        Debug.Log("#ViciousMockery#Animation Called");
        thisChar.anima.ViciousMockery();
        GameObject weaponPrefab = Resources.Load<GameObject>("ViciousMockeryWeapon");
        weapon = GameObject.Instantiate<GameObject>(weaponPrefab);
        weapon.transform.localScale = Vector3.one;
        weapon.transform.parent = thisChar.hipsAttach;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        
        DisplayWeapon(false);
        thisChar.DisplaySpecialAbilityFeedback("Vicious Mockery");
        target.DisplaySpecialAbilityFeedback("Mocked by " + thisChar.myCharacter.name);
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!damageDelivered) {
            thisChar.transform.LookAt(target._t);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "ViciousMockeryDamage") {
            damageDelivered = true;
            target.GetMocked();
        }
        if (message == "DisplayWeapon") {
            DisplayWeapon(true);

        }
        if (message == "HideWeapon") {
            DisplayWeapon(false);
            GameObject.Destroy(weapon);
        }

        if (message == "DisplayEffect") {
            GameObject effect = Resources.Load<GameObject>("ViciousMockeryEffect");
            var fx1 = GameObject.Instantiate(effect);
            fx1.transform.position = weapon.transform.position;
            fx1.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
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