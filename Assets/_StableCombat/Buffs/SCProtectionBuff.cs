using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCProtectionBuff : SCBuff
{
    public void TakeDamage() {
       thisChar.DisplaySpecialAbilityFeedback("Protection from Damage");
    }

    public void GetTackled() {
        thisChar.DisplaySpecialAbilityFeedback("Protection from Tackle");
    }
}
