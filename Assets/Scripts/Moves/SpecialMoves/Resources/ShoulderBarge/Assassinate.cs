using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassinate : ActiveSpecialMove {
    float lastFired;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Assassinate();
    }

    public override string GetName() {
        return "Assassinate";
    }

    public override string GetDescription() {
        return "Fires a long range shot from a hidden hand crossbow with deadly accuracy. All but the most stalwart opponents will be killed instantly.";
    }

    public override bool Check(StableCombatChar _char) {

        if (Time.time <= lastFired + 20) { return false; }
        if (_char.isKnockedDown) { return false; }
        if (_char.ball == null || _char.ball.OtherTeamHolding(_char.team)) {
            Debug.Log("#Assassinate#Fired");
            OnActivate(_char);
            return true;
        }
        return false;
    }

}
