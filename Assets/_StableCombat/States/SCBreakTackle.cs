using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBreakTackle : SCSucceedAgainstTackle {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.anim.applyRootMotion = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        thisChar.anim.applyRootMotion = false;
        base.WillExit();
    }
    public override void FireAnimation() {
        if (ball.holder == thisChar) {
            Debug.Log("#Dodge#Angle: " + angle + "  " + Mathf.Abs(angle));
        }
        if (Mathf.Abs(angle) < 70) {
            thisChar.anima.DodgeTackle("Front");
        }
        else {
            thisChar.anima.DodgeTackle("Back");
        }
    }
}
    