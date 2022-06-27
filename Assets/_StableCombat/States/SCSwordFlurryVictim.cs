using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSwordFlurryVictim : StableCombatCharState, CannotInterrupt, CannotSpecial, CanDamageButNotChangeState {
    public StableCombatChar attacker;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.agent.SetDestination(attacker.position);
    }
    float timeout;
    public override void Update() {
        base.Update();
        if (thisChar.Distance(attacker) <= 1) {
            thisChar.agent.TotalStop();
        }
        else {
            thisChar.agent.SetDestination(attacker.position);
        }
        if (attacker.state.GetType() != typeof(SCSwordFlurry)) {
            timeout += Time.deltaTime;
            if (timeout >= 1.0f) {
                thisChar.Idle();
            }
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}