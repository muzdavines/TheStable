using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGetStripped : SCFailAgainstTackle
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
        thisChar.anima.GetStripped();
        Debug.Log("#TODO#Add Stripped Anim");
    }
}
