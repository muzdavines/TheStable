using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCCombatDowned : SCCombatStanceState, ApexState {

    public float getUpTime;
    Vector3 stayPos;
    public override void TransitionTo(StableCombatCharState state) {
        Debug.Log("Trying to change to " + state.GetType().ToString());
        thisChar.ReleaseTarget();
        if (!state.GetType().GetInterfaces().Contains(typeof(SCReviveUnit))) { return; }
       
        base.TransitionTo(state);
    }
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        stayPos = thisChar._t.position;
        thisChar.anima.GetDowned();
        thisChar.ReleaseTarget();
        thisChar.agent.enabled = false;
        thisChar.GetComponent<Collider>().enabled = false;
        if (thisChar.fieldSport) {
            if (ball?.holder == thisChar) { ball.GetDropped(); }
            //getUpTime = Time.time + 10;
        } else { getUpTime = Mathf.Infinity; }
        getUpTime = Mathf.Infinity;
    }
    public override void Update() {
        base.Update();
        thisChar._t.position = stayPos;
        thisChar.agent.isStopped = true;
        if (Time.time < getUpTime) { return; }
        getUpTime = Mathf.Infinity;
        
        thisChar.GetRevived();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.agent.enabled = true;
        thisChar.GetComponent<Collider>().enabled = true;
    }
}
