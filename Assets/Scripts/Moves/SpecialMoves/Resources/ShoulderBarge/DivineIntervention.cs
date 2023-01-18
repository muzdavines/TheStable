using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineIntervention : ActiveSpecialMove {
    float lastFired;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.DivineIntervention();
    }
    public override string GetName() {
        return "Divine Intervention";
    }

    public override string GetDescription() {
        return
            "Protects team from incoming projectiles, reflecting those projectiles back at the source when possible.";
    }

    public override bool Check(StableCombatChar _char) {

        if (Time.time <= lastFired + 5) { return false; }
        if (_char.isKnockedDown) { return false; }
        if (FindObjectOfType<SCProjectile>()) {
            Debug.Log("#DivineIntervention#Activate");
            OnActivate(_char);
            return true;
        }
        return false;
    }

}