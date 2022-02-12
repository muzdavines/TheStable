using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCampState : SCSkillState {
    float nextNumCheck = Mathf.Infinity;
    public Step step;
    int quality;
    GameObject smoke;
    bool didFireAction;
    Transform walkTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 8.0f;
        StopWalking();
        walkTarget = poi.allPurposeTransforms[0];
        thisChar.agent.SetDestination(walkTarget.position + new Vector3(3, 0, 0));
        didFireAction = false;
        quality = CampResult();

        Debug.Log("#TODO#Set Shoulder Cam");
        //Helper.Cam().SetTarget(thisChar.shoulderCam);
        smoke = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("CampBuildSmoke"));

    }
    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Camp Action");
        didFireAction = true;
        thisChar.agent.transform.rotation = poi.targetPos.rotation;
        thisChar.agent.isStopped = true;
        FireAction();
        thisChar.anima.SkillGeneric();
        //Arrow

    }
    bool campDisplayed = false;
    void DisplayCamp() {
        Debug.Log("DisplayCamp " + quality);
        if (campDisplayed) { return; }
        campDisplayed = true;
        for (int i = 0; i < quality; i++) {
            poi.allPurposeTransforms[i].gameObject.SetActive(true);
            foreach (ParticleSystem p in smoke.GetComponentsInChildren<ParticleSystem>()) {
                p.Stop(true);
            }
        }
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
        //need to wait until lockpick anim is done
        if (!didFireAction && Vector3.Distance(thisChar.transform.position, walkTarget.position) < 1) { FireAction(); }
        if (Time.time > nextNumCheck) {
            DisplayCamp();
            poi.Resolve(quality);
            thisChar.Idle();
            nextNumCheck = Mathf.Infinity;
            return;
        }
    }

    public override void WillExit() {
        base.WillExit();
        Camera.main.GetComponent<CameraController>().offset += new Vector3(0, 30, 0);
    }


    public int CampResult() {
        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21);
        int skill = thisChar.myCharacter.survivalist;
        float comp = diceRoll + skill;
        int quality = (int)(comp * .1f);
        GameObject.FindObjectOfType<MissionController>().update.text = "Quality of Camp: " + quality;
        return quality;

    }
}
