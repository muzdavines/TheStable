﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateLockpick : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public MissionPOI poi;
    public Step step;
    bool success;
    
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 5.0f;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        anim.SetTrigger("Action");
        anim.SetFloat("ActionType", (int)step.type);
        success = DidPickLock();
        agent.transform.rotation = poi.targetPos.rotation;
        agent.isStopped = true;
        
        //Calc pass/fail
        //Calc time to pick based on Char skill

    }

    public override void Update() {
        base.Update();
        //need to wait until lockpick anim is done
        if (Time.time < nextNumCheck) {

            return;
        }
        poi.Resolve(success);
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
    }

    public override void WillExit() {
        base.WillExit();
    }

    public bool DidPickLock() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21);
        int skill = thisChar.character.lockpicking;
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        } else { return false; }


    }
}