using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSwordFlurryVictim : StableCombatCharState {
    public StableCombatChar attacker;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.SetDestination(attacker.position);
    }

    public override void Update() {
        base.Update();
        if (thisChar.Distance(attacker) <= 1) {
            thisChar.agent.TotalStop();
        }
        else {
            thisChar.agent.SetDestination(attacker.position);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}