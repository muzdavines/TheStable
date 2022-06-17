using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCAbilityBuff : SCBuff
{
    float mod = 0;
    CharacterAttribute[] attributes;
    float modShooting;
    float modCarrying;
    float modTackling;
    float modPassing;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="abilityMod"></param>
    /// <param name="_attributes">Ability Mod should be in percent form, with -.25 being a 25% reduction, and 1 being a 100% increase</param>
    /// <returns></returns>
    public SCAbilityBuff Init(float duration, float abilityMod, CharacterAttribute[] _attributes) {
        base.Init(duration);
        if (abilityMod <= -1) { abilityMod = -.9f; };
        mod = abilityMod;
        attributes = _attributes;
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
            }
        }
        thisChar.myCharacter.passing = Mathf.RoundToInt(modPassing);
        thisChar.myCharacter.shooting = Mathf.RoundToInt(modShooting);
        thisChar.myCharacter.tackling = Mathf.RoundToInt(modTackling);
        thisChar.myCharacter.carrying = Mathf.RoundToInt(modCarrying);
        return this as SCAbilityBuff;
    }

    public override void EndEffect() {
        foreach (var a in attributes) {
            switch (a) {
                case CharacterAttribute.carrying:
                    thisChar.myCharacter.carrying = Mathf.RoundToInt(modCarrying / (1 + mod));
                    break;
                case CharacterAttribute.passing:
                    thisChar.myCharacter.passing = Mathf.RoundToInt(modPassing / (1 + mod));
                    break;
                case CharacterAttribute.shooting:
                    thisChar.myCharacter.shooting = Mathf.RoundToInt(modShooting / (1 + mod));
                    break;
                case CharacterAttribute.tackling:
                    thisChar.myCharacter.tackling = Mathf.RoundToInt(modTackling / (1 + mod));
                    break;
            }
        }
        base.EndEffect();
    }


}
