using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShadowStrikeVictim : StableCombatCharState, CannotInterrupt {
    Vector3 stayPos;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown = 6;
        thisChar.anima.ShadowStrikeVictim();
        stayPos = thisChar._t.position;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.position = stayPos;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}