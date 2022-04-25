using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCKnockdown : StableCombatCharState
{

    Vector3 stayPos;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.isStopped = true;
        stayPos = thisChar.position;
        //thisChar.anim.SetTrigger("Knockdown");
        thisChar.anima.Knockdown();
        thisChar.playStyle = PlayStyle.Play;
        thisChar.SetTackleCooldown();
        thisChar.ReleaseTarget();
    }
    public override void Update() {
        base.Update();
        thisChar._t.position = stayPos;
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
