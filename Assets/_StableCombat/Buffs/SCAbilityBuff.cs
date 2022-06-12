using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCAbilityBuff : SCBuff
{
    float mod = 0;
    CharacterAttribute[] attributes;
    public void Init(float duration, float abilityMod, CharacterAttribute[] _attributes) {
        base.Init(duration);
        mod = abilityMod;
        attributes = _attributes;
        foreach (var a in _attributes) {
            switch (a) {
                case CharacterAttribute.carrying:

            }
        }
    }
}
