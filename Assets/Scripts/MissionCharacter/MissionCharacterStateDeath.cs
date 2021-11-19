using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateDeath : MissionCharacterState {
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        thisChar = owner.controller;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        agent.isStopped = true;
        thisChar.character.incapacitated = true;
    }

    public override void Update() {
        base.Update();
        
    }

    public override void WillExit() {
        base.WillExit();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "DeathFinished":
                Debug.Log("#Anim#DeathFinishedAnimReceived");
                AnimFinished();
                break;
        }
    }
    public void AnimFinished() {
        GameObject.FindObjectOfType<CombatTestController>().AnimComplete();
        thisChar.healthBar.gameObject.SetActive(false);
        agent.enabled = false;
        thisChar.anim.enabled = false;

    }

    

    public override void StartAnim() {
        base.StartAnim();
        thisChar.healthBar.RemoveDots(100);
        anim.SetTrigger("Death");
        GameObject.Instantiate(Resources.Load<GameObject>("DeathBlood"), thisChar.transform.position + new Vector3(0,2,0), thisChar.transform.rotation);

    }
}
