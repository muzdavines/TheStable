using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RallyingCry : ActiveSpecialMove {


    public List<SCAbilityBuff> teamBuffs;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        teamBuffs ??= new List<SCAbilityBuff>();
        Debug.Log("#Execute#Activate Execute");
        _char.RallyingCry(this);
    }
    public override string GetName() {
        return "Rallying Cry";
    }

    public override string GetDescription() {
        return "After a goal against the hero's team, if the team is losing, the Hero inspires his teammates, raising all of their attributes by 50%";
    }

    public override bool Check(StableCombatChar _char) {
        return false;
    }
    public void DeactivateAll() {
        teamBuffs ??= new List<SCAbilityBuff>();
        foreach (var t in teamBuffs) {
            t.EndEffect();
        }
        teamBuffs = new List<SCAbilityBuff>();
    }
}
