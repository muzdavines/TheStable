using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSoulSteal :  StableCombatCharState {
    public StableCombatChar enemy;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (enemy == null || enemy.isKnockedDown) {
            thisChar.Idle();
            return;
        }
        thisChar.agent.TotalStop();
        thisChar.anima.SoulSteal();
    }
    public override void Update() {
        base.Update();
        thisChar.agent.TotalStop();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SoulSteal") {
            if (enemy != null && !enemy.isKnockedDown) {
                float shooting = enemy.myCharacter.shooting * .25f;
                float carrying = enemy.myCharacter.carrying * .25f;
                float tackling = enemy.myCharacter.tackling * .25f;
                float passing = enemy.myCharacter.passing * .25f;
                Debug.Log("#SoulSteal#Shooting: " + shooting+ "  "+ thisChar.myCharacter.shooting + " "+ shooting / thisChar.myCharacter.shooting);
                Debug.Log("#SoulSteal#Carrying: " + carrying + "  " + thisChar.myCharacter.carrying + " " + carrying / thisChar.myCharacter.carrying);
                Debug.Log("#SoulSteal#Tackling: " + tackling);
                Debug.Log("#SoulSteal#Passing: " + passing);
                enemy.AbilityBuff(10f, -.25f, new CharacterAttribute[] { CharacterAttribute.all });
                
                thisChar.StartCoroutine(DelayAdd(.1f, 10f, shooting / thisChar.myCharacter.shooting, new CharacterAttribute[] { CharacterAttribute.shooting }));
                thisChar.StartCoroutine(DelayAdd(.2f, 10f, carrying / thisChar.myCharacter.carrying, new CharacterAttribute[] { CharacterAttribute.carrying }));
                thisChar.StartCoroutine(DelayAdd(.3f, 10f, passing / thisChar.myCharacter.passing, new CharacterAttribute[] { CharacterAttribute.passing }));
                thisChar.StartCoroutine(DelayAdd(.4f, 10f, tackling / thisChar.myCharacter.tackling, new CharacterAttribute[] { CharacterAttribute.tackling }));
                enemy.DisplaySpecialAbilityFeedback("Soul Stolen");
                thisChar.DisplaySpecialAbilityFeedback("Soul Steal");
                enemy.TakeDamage(new StableDamage() { mind = 50 }, thisChar, true);
            }
        }

    }
    IEnumerator DelayAdd(float delay, float duration, float mod, CharacterAttribute[] attrib) {
        yield return new WaitForSeconds(delay);
        thisChar.AbilityBuff(duration, mod, attrib);

    }

    public override void WillExit() {
        base.WillExit();
    }
}
