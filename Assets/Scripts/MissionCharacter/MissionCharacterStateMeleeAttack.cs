using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateMeleeAttack : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public Transform attackTarget;
    bool inRange = false;
    public int dotDamage;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        nextNumCheck = .1f;
        agent.SetDestination(attackTarget.position);
        anim.SetBool("Walk", true);
        anim.SetBool("Run", false);
        agent.isStopped = false;
        Camera.main.GetComponent<CameraController>().cameraTarget = agent.gameObject;
        //Calc pass/fail
        //Calc time to pick based on Char skill

    }

    public override void Update() {
        base.Update();
        if (!inRange && Vector3.Distance(thisChar.transform.position, attackTarget.position) < 1f) {
            inRange = true;
            agent.isStopped = true;
            anim.SetBool("Walk", false);
            StartAnim();
        } else { return; }

        if (Time.time < nextNumCheck) {

            return;
        }
        
    }
    void StartAnim() {
        anim.SetTrigger("MeleeAttack");
        agent.transform.LookAt(attackTarget);
        attackTarget.LookAt(agent.transform);
        

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "MeleeAttackFinished":
                AnimFinished();
                break;
            case "MeleeAttackHitTarget":
                HitTarget();
                break;
        }
    }
    public void HitTarget() {
        (attackTarget.GetComponent<MissionCharacter>().state as MissionCharacterState).StartAnim();
        
    }
    public void AnimFinished() {
        GameObject.FindObjectOfType<CombatTestController>().AnimComplete();
        
    }
    public override void WillExit() {
        base.WillExit();
    }
}