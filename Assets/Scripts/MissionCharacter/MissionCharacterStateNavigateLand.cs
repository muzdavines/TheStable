using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateNavigateLand : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public MissionPOI poi;
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public Transform badLocation;
    bool complete;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 15.0f;
        
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);


        fireActionTime = Time.time + 5;
        didFireAction = false;
        success = NavigationResult();
        agent.transform.rotation = poi.targetPos.rotation;
        agent.isStopped = true;

        //Calc pass/fail
        //Calc time to pick based on Char skill
        Helper.Cam().SetTarget(agent.transform);
        FireAction();


    }

    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Hunt Action");
        didFireAction = true;
        fireActionTime = Mathf.Infinity;
        anim.SetFloat("ActionType", (int)step.type);
        anim.SetTrigger("Action");
        //Arrow
        thisChar.ControlCam();
        Helper.Speech(agent.transform, "Wait, I think I know a shortcut.");
        nextNumCheck = Time.time + (success ? 5.0f : Mathf.Infinity);
        
        if (!success) { thisChar.BroadcastWalkTo(badLocation); agent.SetDestination(badLocation.position); agent.isStopped = false; anim.SetBool("Walk", true); Helper.Cam().SetControl(); }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "":
                break;
        }
    }

    public override void Update() {
        base.Update();
        if (complete) { return; }
        //need to wait until lockpick anim is done
        if (!success) {
            //Debug.Log("Dist: " + Vector3.Distance(badLocation.position, agent.transform.position));
            if (Vector3.Distance(badLocation.position, agent.transform.position) < 4f) { ArrivedAtBadLocation(); complete = true; }
            return;
        }
        if (Time.time < nextNumCheck) {
            return;
        }
        NavSuccess();
        complete = true;
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.ControlCam(false, 5f);
    }

    public void ArrivedAtBadLocation() {
        thisChar.IdleDontAct();
        anim.SetFloat("ActionType", 5); //make this someone shaking their head
        Debug.Log("Fix Action Type here to someone shaking their head.");
        anim.SetTrigger("Action");
        poi.Resolve(false);
        nextNumCheck = Mathf.Infinity;
        
    }

    public void NavSuccess() {
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
        Helper.Speech(agent.transform, "No, wait, that's not the right way. Let's keep going.");
        poi.Resolve(true);
    }

    public bool NavigationResult() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21)*poi.step.mod;
        int skill = thisChar.character.landNavigation;
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        }
        else { return false; }


    }
}