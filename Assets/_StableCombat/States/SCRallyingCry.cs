using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRallyingCry : StableCombatCharState {
    public ActiveSpecialMove specialMove;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        foreach (StableCombatChar c in thisChar.coach.players) {
            if (c.GetComponent<SCRallyingCryBuff>()) {
                continue;
            } else {
                CharacterAttribute[] mods = { CharacterAttribute.carrying, CharacterAttribute.passing, CharacterAttribute.shooting, CharacterAttribute.tackling };
                (specialMove as RallyingCry).teamBuffs.Add(c.RallyingCryBuff(100000f, .5f, mods));
                c.DisplaySpecialAbilityFeedback("Rallying Cry");
            }
        }
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
