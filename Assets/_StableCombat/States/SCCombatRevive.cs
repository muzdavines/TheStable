using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCombatRevive : SCCombatStanceState, SCReviveUnit {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.RestoreHealth();
        thisChar.CombatIdle();
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
