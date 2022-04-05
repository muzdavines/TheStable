using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAttackMove : Move
{
   
    public override void HitEffect(CharacterHealth health, Character attacker) {
        base.HitEffect(health, attacker);
        StableDamage hitDamage = new StableDamage();
        Debug.Log("TODO: Add attributes here to damage");
        hitDamage.stamina = staminaDamage;
        hitDamage.balance = balanceDamage;
        hitDamage.mind = mindDamage;
        hitDamage.health = healthDamage;
        health.Deal(hitDamage);
        foreach (string mod in modifiers) {
            //MoveModifier m = Game.instance.modifierList.GetModifier(mod);
           // Debug.Log(m.directDamage + " for " + m.modName);
           // m.ApplyEffect(health, attacker);
        }
    }
}
