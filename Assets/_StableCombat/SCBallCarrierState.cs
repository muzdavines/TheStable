using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBallCarrierState : StableCombatCharState
{
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        if (message == "TryTackle") {
            return TryTackle(sender);
        }
        else return null;
    }

    SCResolution TryTackle(StableCombatChar tackler) {
        var res = new SCResolution();
        int tackling = tackler.tackling;
        int dodging = thisChar.dodging;
        float roll = Random.Range(0, dodging + 1) - Random.Range(0, tackling + 1);
        Debug.Log("#DiceRoll#Dodge Roll: " + roll);
        if (roll >= 0) { res.success = false; thisChar.DodgeTackle(); } else { res.success = true; thisChar.GetTackled(); }
        return res;
    }
}
