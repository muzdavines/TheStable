using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTeleportTeammate : StableCombatCharState,CannotInterrupt, CannotSpecial,CannotTarget {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.ball?.holder == null) {
            thisChar.Idle();
                return;
        }

        ball.holder.transform.position =
            ball.holder.enemyGoal.transform.position + ball.holder.enemyGoal.transform.forward * 10;
        thisChar.DisplaySpecialAbilityFeedback("Teleport Teammate");
        thisChar.Idle();
    }
}
