using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGetMocked : SCFailAgainstTackle {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.AbilityBuff(8, -.50f, new CharacterAttribute[] { CharacterAttribute.carrying, CharacterAttribute.passing, CharacterAttribute.shooting, CharacterAttribute.tackling });
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown = 6;
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

    public override void FireAnimation() {
        //thisChar.anima.Knockdown();
    }
}