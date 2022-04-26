using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCNavigateLand :  SCSkillState {
    float nextNumCheck = Mathf.Infinity;
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public Transform badLocation;
    bool complete;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 15.0f;

        StopWalking();

        fireActionTime = Time.time + 5;
        didFireAction = false;
        success = NavigationResult();
        
        //Calc pass/fail
        //Calc time to pick based on Char skill
        Helper.Cam().SetTarget(thisChar.transform);
        FireAction();
    }
    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Hunt Action");
        didFireAction = true;
        fireActionTime = Mathf.Infinity;
        thisChar.anima.SkillNavigateLand();
        //Arrow
        Debug.Log("#TODO#Add Control Cam to Character Controller");
        Helper.Speech(thisChar.transform, "Wait, I think I know a shortcut.");
        nextNumCheck = Time.time + (success ? 5.0f : Mathf.Infinity);

        if (!success) {
            Debug.Log("#TODO#Broadcast Walk to Location");
            thisChar.agent.SetDestination(badLocation.position);
            thisChar.agent.isStopped = false;
            thisChar.anima.Idle();
            Helper.Cam().SetControl(); }
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
            if (Vector3.Distance(badLocation.position, thisChar.transform.position) < 4f) {
                ArrivedAtBadLocation();
                complete = true; }
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
        Debug.Log("#TODO#thisChar.ControlCam(false, 5f))");
    }

    public void ArrivedAtBadLocation() {
        StopWalking();
        Debug.Log("#TODO#Fix Action Type here to someone shaking their head.");
        poi.Resolve(false);
        nextNumCheck = Mathf.Infinity;
    }

    public void NavSuccess() {
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
        Helper.Speech(thisChar.transform, "No, wait, that's not the right way. Let's keep going.");
        poi.Resolve(true);
    }

    public bool NavigationResult() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21) * poi.step.mod;
        int skill = 10;//thisChar.myCharacter.landNavigation;
        Debug.Log("#TODO#Switch this to Trait");
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        }
        else { return false; }


    }
}
