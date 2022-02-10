using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCMissionIdle : StableCombatCharState {
    float nextNumCheck = Mathf.Infinity;
    public bool act;
    CombatController combatController;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        nextNumCheck = Time.time + 5.0f;
        thisChar.anima.Idle();
        thisChar.agent.isStopped = true;
        combatController = GameObject.FindObjectOfType<CombatController>();
    }
    public override void Update() {
        base.Update();
        if (combatController.combatActive) {
            thisChar.CombatIdle();
            return;
        }
        if (!act || Time.time < nextNumCheck) {
            return;
        }
        Debug.Log("#Mission# BroadcastNextStep");
        thisChar.BroadcastNextStep();
        nextNumCheck = Time.time + 5.0f;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
