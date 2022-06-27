using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCExecuteVictim : StableCombatCharState, CannotSpecial, CannotInterrupt {
    Vector3 stayPos;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown = 6;
        if (ball.holder !=null && ball.holder == thisChar) {
            ball.GetDropped();
        }
        thisChar.anima.ExecuteVictim();
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
