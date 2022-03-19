using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGKState : StableCombatCharState
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

    public bool ShouldDiveForBall(){
        if (ball.holder != null){ return false;}
        if (ball.Distance(thisChar.transform) > 1){return false;}
        if (ball.velocity < 1)return false;

        return true;

    }
}
