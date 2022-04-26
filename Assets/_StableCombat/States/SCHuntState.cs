using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCHuntState :  SCSkillState {
    float nextNumCheck = Mathf.Infinity;
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public HuntedAnimal animal;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 3.0f;
        StopWalking();
        fireActionTime = Time.time + 5;
        didFireAction = false;
        success = HuntResult();
        //Calc pass/fail
        //Calc time to pick based on Char skill
        Helper.Speech(thisChar.agent.transform, "Hold! The hunt is afoot!");
    }
    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Hunt Action");
        didFireAction = true;
        fireActionTime = Mathf.Infinity;
        thisChar.anima.SkillHunt();
        /*
        anim.SetFloat("ActionType", (int)step.type);
        anim.SetTrigger("Action");*/
        //Arrow
        Debug.Log("#TODO#Add Control Cam to Character Controller");
        //thisChar.ControlCam();
    }
    public void SpawnArrow() {
        animal = poi.GetComponent<MissionHelperHunt>().animal.GetComponent<HuntedAnimal>();
        HuntArrow arrow = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("HuntArrow")).GetComponent<HuntArrow>();
        arrow.transform.position = thisChar.transform.position + new Vector3(0, 1, 0);
        if (success) {
            arrow.target = animal.transform;
        }
        else { arrow.target = animal.transform.Find("MissTarget").transform; }
        arrow.animal = animal;
        arrow.speed = 22;
        arrow.success = success;
        arrow.charHunt = this;
    }
    public bool HuntResult() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21) * poi.step.mod;
        int skill = 10;// thisChar.myCharacter.hunting;
        Debug.Log("#TODO#Switch this to Trait");
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        }
        else { return false; }
    }
    public void ArrowComplete() {
        poi.Resolve(success);
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
    }
    public override void Update() {
        base.Update();
        if (Time.time >= fireActionTime) { FireAction(); }
        if (Time.time < nextNumCheck) {
            return;
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "FireWeaponRH":
                SpawnArrow();
                break;
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
