using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCMissionMoveTo : StableCombatCharState
{
    float nextNumCheck = Mathf.Infinity;
    public Transform target;
    public float timeOut;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        nextNumCheck = Time.time + 1.0f;
        timeOut = Time.time + 30;
        try {
            thisChar.agent.SetDestination(target.position);
        }
        catch { Debug.Log("Error with agent for " + thisChar.agent.name); }
        thisChar.anima.Idle();
        thisChar.agent.isStopped = false;
        thisChar.agent.speed = 15 * .4f;
    }
    public override void Update() {
        base.Update();
        if (Time.time < nextNumCheck) {
            return;
        }
        if (Time.time >= timeOut) {
            timeOut = Time.time + 20;
            thisChar.transform.position = target.position;
            return;
        }
        Debug.Log("#Mission#Move to Target: " + target.name);
        try {
            thisChar.agent.SetDestination(target.position);
            thisChar.agent.isStopped = false;
        }
        catch { Debug.Log("Error with agent for " + thisChar.agent.name); }
        if (Vector3.Distance(thisChar.transform.position, target.position) < 4f) {
            Debug.Log("Target Met");
            thisChar.agent.velocity = Vector3.zero;
            thisChar.agent.speed = thisChar.myCharacter.runspeed * .4f;
            thisChar.Idle();
        }
        nextNumCheck = Time.time + 1.0f;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        thisChar.agent.speed = thisChar.myCharacter.runspeed * .4f;
        thisChar.agent.velocity = Vector3.zero;
        base.WillExit();
    }
}
