using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VikingCrew.Tools.UI;
public class MissionCharacterStateHunt : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public MissionPOI poi;
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public HuntedAnimal animal;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 3.0f;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        fireActionTime = Time.time + 5;
        didFireAction = false;
        success = HuntResult();
        agent.transform.rotation = poi.targetPos.rotation;
        agent.isStopped = true;

        //Calc pass/fail
        //Calc time to pick based on Char skill
        Helper.Speech(agent.transform, "Hold! The hunt is afoot!");
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

    }

    public void SpawnArrow() {
        animal = poi.GetComponent<MissionHelperHunt>().animal.GetComponent<HuntedAnimal>();
        HuntArrow arrow = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("HuntArrow")).GetComponent<HuntArrow>();
        arrow.transform.position = agent.transform.position + new Vector3(0, 1, 0);
        if (success) {
            arrow.target = animal.transform;
        } else { arrow.target = animal.transform.Find("MissTarget").transform; }
        arrow.animal = animal;
        arrow.speed = 22;
        arrow.success = success;
        //arrow.charHunt = this;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "FireArrow":
                SpawnArrow();
                break;
        }
    }

    public override void Update() {
        base.Update();
        //need to wait until lockpick anim is done
        if (Time.time >= fireActionTime) { FireAction(); }
        if (Time.time < nextNumCheck) {
            return;
        }
        
       
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.ControlCam(false, 5f);
    }

    public void ArrowComplete() {
        poi.Resolve(success);
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
    }
   
    public bool HuntResult() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21)* poi.step.mod;
        int skill = thisChar.character.hunting;
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        }
        else { return false; }


    }
}