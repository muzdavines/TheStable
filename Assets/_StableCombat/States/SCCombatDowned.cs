using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCCombatDowned : SCCombatStanceState, ApexState {

    public float getUpTime;
    public override void TransitionTo(StableCombatCharState state) {
        Debug.Log("Trying to change to " + state.GetType().ToString());
        if (!state.GetType().GetInterfaces().Contains(typeof(SCReviveUnit))) { return; }
        base.TransitionTo(state);
    }
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.GetDowned();
        if (thisChar.fieldSport) {
            ball.GetDropped();
            //getUpTime = Time.time + 10;
        } else { getUpTime = Mathf.Infinity; }
        getUpTime = Mathf.Infinity;
    }
    public override void Update() {
        base.Update();
        if (Time.time < getUpTime) { return; }
        getUpTime = Mathf.Infinity;
        
        thisChar.GetRevived();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
