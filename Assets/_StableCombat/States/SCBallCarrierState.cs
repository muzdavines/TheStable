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
        TackleType tackleType = tackler.myCharacter.strength >= tackler.myCharacter.dexterity ? TackleType.Tackle : TackleType.Strip;
        res.tackleType = tackleType;
        float tacklerRoll = 0;
        float carrierRoll = 0;
        switch (tackleType) {
            case TackleType.Strip:
                float stripRoll = tackler.myCharacter.dexterity + tackler.myCharacter.tackling;
                float stripDodge = thisChar.myCharacter.dexterity + thisChar.myCharacter.carrying;
                if (stripRoll > stripDodge) {
                    res.success = true;
                    thisChar.GetStripped(tackler);
                }
                else {
                    res.success = false;
                    thisChar.AvoidStrip(tackler);
                }
                tacklerRoll = stripRoll;
                carrierRoll = stripDodge;
                break;
            case TackleType.Tackle:
                float tackleRoll = tackler.myCharacter.strength + tackler.myCharacter.tackling;
                float tackleDodge = 0;
                if (thisChar.myCharacter.strength * .5f > thisChar.myCharacter.agility) {
                    tackleDodge = (thisChar.myCharacter.strength * .5f) + thisChar.myCharacter.carrying;
                    if (tackleRoll > tackleDodge) {
                        res.success = true;
                        thisChar.GetTackled(tackler);

                    }
                    else {
                        res.success = false;
                        thisChar.BreakTackle(tackler);
                    }
                    tacklerRoll = tackleRoll;
                    carrierRoll = tackleDodge;
                }
                else {
                    tackleDodge = thisChar.myCharacter.agility + thisChar.myCharacter.carrying;
                    if (tackleRoll > tackleDodge) {
                        res.success = true;
                        thisChar.GetTackled(tackler);

                    }
                    else {
                        res.success = false;
                        thisChar.DodgeTackle(tackler);
                    }
                    tacklerRoll = tackleRoll;
                    carrierRoll = tackleDodge;
                }
                break;
        }
        Debug.Log("#Tackle# Tackle Type:" + tackleType.ToString() + "\nTackler\nSTR: " + tackler.myCharacter.strength + " DEX: " + tackler.myCharacter.dexterity + " TAK: " + tackler.myCharacter.tackling + "\nCarrier\nSTR: " + thisChar.myCharacter.strength + " DEX: " + thisChar.myCharacter.dexterity + " AGI: " + thisChar.myCharacter.dexterity + " CAR: " + thisChar.myCharacter.carrying+"\nResolution: "+tacklerRoll + " "+ carrierRoll + " "+res.success);

        //float roll = Random.Range(0, dodging + 1) - Random.Range(0, tackling + 1);
        //Debug.Log("#DiceRoll#Dodge Roll: " + roll);
        //if (roll >= 0) { res.success = false; thisChar.DodgeTackle(tackler); } else { res.success = true; thisChar.GetTackled(tackler); }

        return res;
    }

}
