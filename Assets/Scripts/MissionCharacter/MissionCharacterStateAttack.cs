using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateAttack : MissionCharacterCombatState {
    float lastAttack;
    int comboNum;
    int currentAttackAnimID;
    bool shiftPosition;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        lastAttack = Time.time + Random.Range(0,4.33f);
    }
    public override void Update() {
        base.Update();
        FireAttack();
        ShiftPosition();
        thisChar.transform.LookAt(target);
    }
    float nextShift;
    Vector3 shiftPos;
    void ShiftPosition() {
        if (!shiftPosition) { return; }
        if (Time.time >= nextShift) {// || Vector3.Distance(thisChar.transform.position, shiftPos) < .5f) {
            nextShift = Time.time + 2;
            thisChar.agent.isStopped = false;
            shiftPos = thisChar.transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
            thisChar.agent.SetDestination(shiftPos);
            thisChar.transform.LookAt(target);
        }
    }

    void CheckDistance() {
        if (!InAttackRange()) {
            thisChar.RunAtTarget();
        }
    }
    bool CanStartAttack() {
        //Debug.Log(thisChar.DistanceToTarget() +"  Max Range:  "+ thisChar.maxAttackRange);
        if (Time.time >= lastAttack + 4) {
            return true;
        } else { return false; }
    }

    public void FireAttack() {
        if (CanStartAttack()) {
            if (!InAttackRange()) { thisChar.RunAtTarget(); return; }
            thisChar.agent.SetDestination(target.position);
            
            lastAttack = Time.time;
            anim.applyRootMotion = true;
            anim.SetInteger("MeleeType", 4);
            anim.SetTrigger("Hit");
            anim.ResetTrigger("ComboHit");
            shiftPosition = false;
            shiftPos = new Vector3(0, 1000, 0);
        } else { if (shiftPosition == false) { nextShift = Time.time + 2; } shiftPosition = true; }
    }
    public void AttackStart(int i, Limb limb) {
        currentAttackAnimID = i;
        BaseMelee thisWeapon = GetWeapon(limb);
        thisWeapon.Begin();
    }

    public void AttackEnd(int i, Limb limb) {
        currentAttackAnimID = 0;
        BaseMelee thisWeapon = GetWeapon(limb);
        thisWeapon.End();
        nextShift = Time.time + 3;
    }

    

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
    public void InputBeginMeleeScan(int id, Limb limb, Move move = null) {
        Debug.Log("InputBegin "+limb.ToString());
        if (move == null) {
            Debug.Log("No move for MCSA");
            return;
        }
        if (currentAttackAnimID != id) {
            currentAttackAnimID = id;
        }
            BaseMelee thisWeapon = GetWeapon(limb);
        
            if (thisWeapon == null) { Debug.Log("MCSA thisWeapon null");return; }
            thisWeapon.currentMove = GameObject.Instantiate(move);
            thisWeapon.EndScan();
            thisWeapon.BeginScan(move);
        
    }

    /// <summary>
    /// Tell the motor to stop scanning for melee collisions.
    /// </summary>
    public void InputEndMeleeScan(int id, Limb limb) {
        
        if (currentAttackAnimID== id) {
            currentAttackAnimID = 0;
            BaseMelee thisWeapon = GetWeapon(limb);
            
            thisWeapon?.EndScan();
        }
    }
    BaseMelee GetWeapon(Limb limb) {
        BaseMelee thisWeapon = null;
        switch (limb) {
            case Limb.RightHand:
                if (thisChar.rightHandWeapon != null) {
                    thisWeapon = thisChar.rightHandWeapon.GetComponent<BaseMelee>();
                }
                break;
            case Limb.LeftHand:
                if (thisChar.leftHandWeapon != null) {
                    thisWeapon = thisChar.leftHandWeapon.GetComponent<BaseMelee>();
                }
                break;
            case Limb.RightLeg:
                if (thisChar.rightLegWeapon != null) {
                    thisWeapon = thisChar.rightLegWeapon.GetComponent<BaseMelee>();
                }
                break;
            case Limb.LeftLeg:
                if (thisChar.leftLegWeapon != null) {
                    thisWeapon = thisChar.leftLegWeapon.GetComponent<BaseMelee>();
                }
                break;
        }
        return thisWeapon;
    }
}
