using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCAbilityBuff : SCBuff
{
    public float mod = 0;
    CharacterAttribute[] attributes;
    public float modShooting;
    public float modCarrying;
    public float modTackling;
    public float modPassing;
    public int rawShootingDiff;
    public int rawCarryingDiff;
    public int rawTacklingDiff;
    public int rawPassingDiff;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="abilityMod"></param>
    /// <param name="_attributes">Ability Mod should be in percent form, with -.25 being a 25% reduction, and 1 being a 100% increase</param>
    /// <returns></returns>
    public virtual SCAbilityBuff Init(float duration, float abilityMod, CharacterAttribute[] _attributes) {
        base.Init(duration);
        if (abilityMod <= -1) { abilityMod = -.9f; };
        mod = abilityMod;
        attributes = _attributes;
        modCarrying = thisChar.myCharacter.carrying;
        modPassing = thisChar.myCharacter.passing;
        modShooting = thisChar.myCharacter.shooting;
        modTackling = thisChar.myCharacter.tackling;
        foreach (var a in _attributes) {
            switch (a) {
                case CharacterAttribute.carrying:
                    modCarrying = thisChar.myCharacter.carrying * (1 + abilityMod);
                    break;
                case CharacterAttribute.passing:
                    modPassing = thisChar.myCharacter.passing * (1 + abilityMod);
                    break;
                case CharacterAttribute.shooting:
                    modShooting = thisChar.myCharacter.shooting * (1 + abilityMod);
                    break;
                case CharacterAttribute.tackling:
                    modTackling = thisChar.myCharacter.tackling * (1 + abilityMod);
                    break;
                case CharacterAttribute.all:
                    modCarrying = thisChar.myCharacter.carrying * (1 + abilityMod);
                    modPassing = thisChar.myCharacter.passing * (1 + abilityMod);
                    modShooting = thisChar.myCharacter.shooting * (1 + abilityMod);
                    modTackling = thisChar.myCharacter.tackling * (1 + abilityMod);
                    break;
            }
        }
        rawPassingDiff = Mathf.RoundToInt(modPassing) - thisChar.myCharacter.passing;
        rawShootingDiff = Mathf.RoundToInt(modShooting)- thisChar.myCharacter.shooting;
        rawTacklingDiff = Mathf.RoundToInt(modTackling)- thisChar.myCharacter.tackling;
        rawCarryingDiff = Mathf.RoundToInt(modCarrying)- thisChar.myCharacter.carrying;

        thisChar.myCharacter.passing += rawPassingDiff;
        thisChar.myCharacter.shooting += rawShootingDiff;
        thisChar.myCharacter.tackling += rawTacklingDiff;
        thisChar.myCharacter.carrying += rawCarryingDiff;
        return this as SCAbilityBuff;
    }

    public override void EndEffect() {
        thisChar.myCharacter.passing -= rawPassingDiff;
        thisChar.myCharacter.shooting -= rawShootingDiff;
        thisChar.myCharacter.tackling -= rawTacklingDiff;
        thisChar.myCharacter.carrying -= rawCarryingDiff;
        base.EndEffect();
        return;
        foreach (var a in attributes) {
            switch (a) {
                case CharacterAttribute.carrying:
                    thisChar.myCharacter.carrying -= rawCarryingDiff;
                    break;
                case CharacterAttribute.passing:
                    thisChar.myCharacter.passing -= rawPassingDiff;
                    break;
                case CharacterAttribute.shooting:
                    thisChar.myCharacter.shooting -= rawShootingDiff;
                    break;
                case CharacterAttribute.tackling:
                    thisChar.myCharacter.tackling -= rawTacklingDiff;
                    break;
                case CharacterAttribute.all:
                    thisChar.myCharacter.carrying -= rawCarryingDiff;
                    thisChar.myCharacter.passing -= rawPassingDiff;
                    thisChar.myCharacter.shooting -= rawShootingDiff;
                    thisChar.myCharacter.tackling -= rawTacklingDiff;
                    break;
            }
        }
       
    }
}
