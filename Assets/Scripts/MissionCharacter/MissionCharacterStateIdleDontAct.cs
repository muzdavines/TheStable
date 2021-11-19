using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStateIdleDontAct : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;

    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        nextNumCheck = Time.time + 5.0f;
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        agent.isStopped = true;
    }

    public override void Update() {
        base.Update();
        if (Time.time < nextNumCheck) {
            return;
        }
        nextNumCheck = Time.time + 5.0f;

    }

    public override void WillExit() {
        base.WillExit();
    }

}
