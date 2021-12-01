using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateLockpick : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
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
        thisChar.ControlCam(false, 5f);
    }

    public bool DidPickLock() {
        float comp = GetPlayerScore(thisChar.character.lockpicking);
        //GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        scoreIndex = 0;
        ProcessBuzz();
        if (comp >= threshold) {
            return true;
        } else { return false; }
       

    }
}
