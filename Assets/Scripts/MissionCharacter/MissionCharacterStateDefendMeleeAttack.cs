using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateDefendMeleeAttack : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public float defenseType = 0;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        
        agent.isStopped = true;

    }

    public override void Update() {
        base.Update();
        //need to wait until lockpick anim is done
        if (Time.time < nextNumCheck) {

            return;
        }
    
    }

    public void AnimFinished() {
        GameObject.FindObjectOfType<CombatTestController>().AnimComplete();
    }
    public override void WillExit() {
        base.WillExit();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
           case "DefendMeleeAttackFinished":
                AnimFinished();
                break;
        }
    }

    public override void StartAnim() {
        base.StartAnim();
        anim.SetTrigger("DefendMeleeAttack");
        anim.SetFloat("DefendMeleeAttackType", defenseType);
        thisChar.UpdateHealthBar();
    }
}