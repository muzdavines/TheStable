using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBackstab : SCCombatStanceState, CannotInterrupt, CannotTarget {

    Vector3 backstabDaggerPos = new Vector3(0, 3.2f, -1f);
    Vector3 backStabDaggerRot = new Vector3(-180f, 0, 0);

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.myAttackTarget == null || thisChar.myAttackTarget.isKnockedDown) {
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
        var fx2 = GameObject.Instantiate(effect, thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward, thisChar.transform.rotation);
        GameObject.Destroy(fx1, 10);
        GameObject.Destroy(fx2, 10);
        thisChar.transform.position = thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward;
        thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        thisChar.anima.Backstab();
        thisChar.myAttackTarget.BackstabVictim();
        thisChar.DisplaySpecialAbilityFeedback("Backstab");
    }
    bool damageDelivered;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!damageDelivered) {
            thisChar.transform.position = thisChar.myAttackTarget.position - thisChar.myAttackTarget.transform.forward;
            thisChar.transform.LookAt(thisChar.myAttackTarget.transform);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "BackstabDamage") {
            damageDelivered = true;
            thisChar.myAttackTarget.TakeDamage(new StableDamage() { balance = 1, health = 2, mind = 1, stamina = 8, isKnockdown = true });
        }
        if (message == "RotateKnifeStab") {
            RotateKnifeStab();
        }
        if (message == "RotateKnifeOriginal") {
            RotateKnifeOriginal();
        }
    }
    void RotateKnifeStab() {
        thisChar.RHMWeapon.gameObject.SetActive(true);
        thisChar.LHMWeapon.gameObject.SetActive(true);
        thisChar.RHMWeapon.transform.localPosition = backstabDaggerPos;
        thisChar.RHMWeapon.transform.localRotation = Quaternion.Euler(backStabDaggerRot);
        thisChar.LHMWeapon.transform.localPosition = backstabDaggerPos;
        thisChar.LHMWeapon.transform.localRotation = Quaternion.Euler(backStabDaggerRot);
    }
    void RotateKnifeOriginal() {
        thisChar.RHMWeapon.transform.localPosition = Vector3.zero;
        thisChar.RHMWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        thisChar.LHMWeapon.transform.localPosition = Vector3.zero;
        thisChar.LHMWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if (thisChar.fieldSport) {
            thisChar.MeleeWeaponsOn(false);
        }
    }
    public override void WillExit() {
        base.WillExit();
        RotateKnifeOriginal();
    }
}
