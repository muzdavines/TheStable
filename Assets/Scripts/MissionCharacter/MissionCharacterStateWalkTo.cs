using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateWalkTo : MissionCharacterState
{
    float nextNumCheck = Mathf.Infinity;
    public Transform target;
    
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        thisChar = owner.controller;
        nextNumCheck = Time.time + 1.0f;
        try
        {
            agent.SetDestination(target.position);
        } catch { Debug.Log("Error with agent for " + agent.name); }
        anim.SetBool("Walk", true);
        anim.SetBool("Run", false);
        agent.isStopped = false;

        anim.speed = Random.Range(.96f, 1.09f);
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

    public override void WillExit() {
        base.WillExit();
    }
}
