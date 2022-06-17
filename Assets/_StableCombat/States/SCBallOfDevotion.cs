using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBallOfDevotion : StableCombatCharState {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.ball?.holder == null || thisChar.ball.holder != thisChar) {
            thisChar.Idle();
            Debug.Log("#BallOfDevotion#State Exit to Idle");
            return;
        }
        ball.gameObject.AddComponent<BallOfDevotionEffect>().Init(thisChar);
        thisChar.DisplaySpecialAbilityFeedback("Ball of Devotion");
        thisChar.Idle();
    }
}
