using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCDodgeTackle : SCSucceedAgainstTackle
{
   
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
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
