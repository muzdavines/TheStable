using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateAssassinate : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    public MissionPOI poi;
    public Step step;
    bool success;
    public Transform killTarget;
    AudioClip audioClip;
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        step = poi.step;
        nextNumCheck = Time.time + 8.0f;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        anim.SetTrigger("Execute");
        anim.SetFloat("ExecuteType", 0);
        success = DidExecute();
        if (success) {
            killTarget.GetComponent<Animator>().SetTrigger("Executed");
            killTarget.GetComponent<Animator>().SetFloat("ExecutedType", 0);
            audioClip = Resources.Load<AudioClip>("Execution1");
            Helper.PlayOneShot(audioClip);
        } else {
            Debug.Log("play fail anim");
        }
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

    public bool DidExecute() {
        Debug.Log("Sidestepping, fix later");
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + 20 + " Needed: " + 10;
        return true;

        float threshold = step.level * 10;
        float diceRoll = Random.Range(1, 21);
        int skill = thisChar.character.lockpicking;
        float comp = diceRoll + skill;
        GameObject.FindObjectOfType<MissionController>().update.text = "Roll: " + comp + " Needed: " + threshold;
        if (comp >= threshold) {
            return true;
        }
        else { return false; }
    }
}
