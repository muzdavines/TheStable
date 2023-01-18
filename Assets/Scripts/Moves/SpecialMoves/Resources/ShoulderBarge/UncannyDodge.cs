using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UncannyDodge : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
    }
    public override string GetName() {
        return "Uncanny Dodge";
    }

    public override string GetDescription() {
        return "As a ball carrier, acrobatically leaps over a tackler leaving them stunned. Upon landing, the Swashbuckler draws his pistol and fires a deadly shot at the nearest enemy.";
    }

    public override bool Check(StableCombatChar _char) {
        return false;
    }
    public override bool SpotCheck(StableCombatChar _char) {
        base.SpotCheck(_char);
        if (Time.time > lastFired + 10) {
            lastFired = Time.time;
            Debug.Log("#UncannyDodge#Fire Dodge");
            return true;
        }
        return false;
    }
}