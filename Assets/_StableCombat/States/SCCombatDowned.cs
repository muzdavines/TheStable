using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCCombatDowned : SCCombatStanceState {

    public float getUpTime;
    public override void TransitionTo(StableCombatCharState state) {
        Debug.Log("Trying to change to " + state.GetType().ToString());
        if (!state.GetType().GetInterfaces().Contains(typeof(SCReviveUnit))) { return; }
        base.TransitionTo(state);
    }
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.GetDowned();
        ball.GetDropped();
        getUpTime = Time.time + 10;
    }
    public override void Update() {
        base.Update();
        if (Time.time < getUpTime) { Debug.Log("Downed. GetUp: " + getUpTime + "  Time: " + Time.time); return; }
        getUpTime = Mathf.Infinity;
        thisChar.health = thisChar.maxHealth;
        thisChar.stamina = thisChar.maxStamina;
        thisChar.balance = thisChar.maxBalance;
        thisChar.mind = thisChar.maxMind;
        thisChar.GetRevived();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
