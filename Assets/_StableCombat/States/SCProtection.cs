using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCProtection : StableCombatCharState {
    
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ball.holder == null) {
            thisChar.Idle();
            return;
        }

        foreach (StableCombatChar s in ball.holder.coach.players) {
            s.ProtectionBuff();
        }
        thisChar.DisplaySpecialAbilityFeedback("Protection");
        thisChar.Idle();
        
    }
}
