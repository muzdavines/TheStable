using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGetTackled : SCFailAgainstTackle {
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.Knockdown();
        GameObject.FindObjectOfType<FieldSportSoundManager>()?.Ooh();
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown = 6;
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
