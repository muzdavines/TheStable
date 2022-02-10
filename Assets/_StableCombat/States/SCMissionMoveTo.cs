using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCMissionMoveTo : StableCombatCharState
{
    float nextNumCheck = Mathf.Infinity;
    public Transform target;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        nextNumCheck = Time.time + 1.0f;
        try {
            thisChar.agent.SetDestination(target.position);
        }
        catch { Debug.Log("Error with agent for " + thisChar.agent.name); }
        thisChar.anima.Idle();
        thisChar.agent.isStopped = false;
    }
    public override void Update() {
        base.Update();
        if (Time.time < nextNumCheck) {
            return;
        }
        if (Vector3.Distance(thisChar.transform.position, target.position) < 4f) {
            Debug.Log("Target Met");
            thisChar.Idle();
        }
        nextNumCheck = Time.time + 1.0f;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
