using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBallOfPower : StableCombatCharState
{
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.ball?.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
                return;
        }
        ball.gameObject.AddComponent<BallOfPowerEffect>().Init(thisChar);
        thisChar.DisplaySpecialAbilityFeedback("Ball of Power");
        thisChar.Idle();
    }
}
