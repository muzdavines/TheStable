using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCReset : StableCombatCharState, SCReviveUnit, ApexState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.playStyle = PlayStyle.Play;
        thisChar.RestoreHealth();
        thisChar.Idle();
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
}
