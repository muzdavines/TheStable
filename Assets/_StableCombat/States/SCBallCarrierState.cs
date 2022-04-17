using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBallCarrierState : StableCombatCharState
{
    public const float shootAngleMin = .65f;
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        if (message == "TryTackle") {
            return TryTackle(sender);
        }
        else return null;
    }

    public bool HasAngleToShoot() {
        Vector3 goalForward = thisChar.enemyGoal.transform.TransformDirection(Vector3.forward);
        Vector3 toShooter = (thisChar.position - thisChar.enemyGoal.transform.position).normalized;
        float shootAngle = Vector3.Dot(goalForward, toShooter);
        return shootAngle > shootAngleMin;
    }
    public bool ShouldShoot() {
        if (thisChar.enemyGoal.Distance(thisChar) <= thisChar.distToShoot && HasAngleToShoot()) {
            return true;
        }
        return false;
    }
    SCResolution TryTackle(StableCombatChar tackler) {
        var res = new SCResolution();
        bool tackleSuccess = false;
        int tacklerRoll = Random.Range(1, 21);
        int carrierRoll = Random.Range(1, 21);
        if (tacklerRoll == 20) { tacklerRoll = 40; }
        if (carrierRoll == 20) { carrierRoll = 40; }
        if (tacklerRoll == 1) { tacklerRoll = -20; }
        if (carrierRoll == 1) { carrierRoll = -20; }
        int tacklerValue = tackler.myCharacter.tackling + tacklerRoll;
        int carrierValue = thisChar.myCharacter.carrying + carrierRoll;
        if (tacklerValue > carrierValue) {
            tackleSuccess = true;
        }
        Debug.Log("#TackleRoll#Tackler Roll: " + tacklerRoll + " Tackler Abil " + tackler.myCharacter.tackling + "  Carrier Roll: " + carrierRoll + " Carrier Abil: " + thisChar.myCharacter.carrying + " Success: " + tackleSuccess);
        res.success = tackleSuccess;
        res.tackleType = TackleType.Tackle;
        if (tackler.myCharacter.archetype == Character.Archetype.Rogue) {
            if (tackleSuccess) {
                thisChar.GetStripped(tackler);
            }
            else {
                thisChar.AvoidStrip(tackler);
            }
            res.tackleType = TackleType.Strip;
            return res;
        }
        
        switch (thisChar.myCharacter.archetype) {
            case Character.Archetype.Rogue:
                if (tackleSuccess) {
                    thisChar.GetTackled();
                } else {
                    thisChar.DodgeTackle(tackler);
                }
                break;
            case Character.Archetype.Wizard:
                if (tackleSuccess) {
                    thisChar.GetTackled();
                }
                else {
                    thisChar.DodgeTackle(tackler);
                }
                break;
            case Character.Archetype.Warrior:
                if (tackleSuccess) {
                    thisChar.GetTackled();
                }
                else {
                    thisChar.BreakTackle(tackler);
                }
                break;
        }
        return res;
        /*

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
                        thisChar.GetTackled();

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
                        thisChar.GetTackled();

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

        return res;*/
    }

}
