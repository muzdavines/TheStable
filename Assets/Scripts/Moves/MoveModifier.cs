using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveModifier : ScriptableObject
{
    public string modName;
    public bool directDamage;
    public bool dotDamage;
    public bool aoeDamage;
    public bool stamina;
    public bool balance;
    public bool mind;
    public bool health;
    public string primaryAttribute;
    public string secondaryAttribute;
    public float hitPct;

    public void ApplyEffect(CharacterHealth target, Character attacker) {
        GameObject go = Instantiate<GameObject>(Resources.Load<GameObject>(modName + "ModEffect"), target.transform.position+new Vector3(0,1,0), target.transform.rotation);
        
        hitPct = 1f;

        primaryAttribute = "attackMagic";
        secondaryAttribute = "";
        int primaryNumber = attacker.GetCharacterttributeValue(primaryAttribute);
        int secondaryNumber = secondaryAttribute == "" ? 0 : attacker.GetCharacterttributeValue(secondaryAttribute);

        float attributeModifier = ((primaryNumber * .5f) + (secondaryNumber * .25f));

        float dotModifier = 2.0f; //We double total damage because it will be applied over 30 seconds. Use this to modify that coefficient
        StableDamage thisDamage = new StableDamage();

        if (stamina) {
            thisDamage.stamina = (int)attributeModifier;
        }
        if (balance) {
            thisDamage.balance = (int)attributeModifier;
        }
        if (mind) {
            thisDamage.mind = (int)attributeModifier;
        }
        if (health) {
            thisDamage.health = (int)attributeModifier;
        }

        if (directDamage) {
            target.Deal(thisDamage);
        }
       
        if (dotDamage) {
            DOTDamage[] ds = target.gameObject.GetComponents<DOTDamage>();
            foreach (DOTDamage thisD in ds) {
                if (thisD.dotName == modName) { return; }
            }
            Transform decal = go.transform.Find("Decal");
            if (decal != null) {
                decal.parent = target.transform;
                Destroy(decal, 30);
            }
            DOTDamage d = target.gameObject.AddComponent<DOTDamage>();
            d.Init(thisDamage, target, dotModifier, modName);
        }

        if (aoeDamage) {
            int friendlyTeam = attacker.currentObject.GetComponent<Actor>().Side;
            float aoeMod = .5f;
            Vector3 attackerPos = attacker.currentObject.transform.position;
            CharacterHealth[] others = FindObjectsOfType<CharacterHealth>();
            thisDamage.Modify(aoeMod);
            foreach (CharacterHealth otherHealth in others) {
                if (otherHealth.GetComponent<Actor>().Side == friendlyTeam) {
                    continue;
                }
                if (Vector3.Distance(otherHealth.transform.position, attackerPos) > 50) {
                    Debug.Log(otherHealth.transform.name + " is too far for " + attacker.name + "s' AOE: " + modName);
                    continue;
                }
                GameObject aoeDamageEffect = Instantiate<GameObject>(Resources.Load<GameObject>(modName + "ModEffectDamage"), otherHealth.transform.position + new Vector3(0, 1, 0), otherHealth.transform.rotation);
                
                Debug.Log("AOE: " + thisDamage.balance + "  "+thisDamage.health);
                otherHealth.Deal(thisDamage);

                /*
                 * establish friendly team
                 * establish aoe modifier, maybe .3 or .25
                 * Get all CharacterHealths in Range
                 * iterate and check for team
                 * if team not same, apply direct damage
                 * Instantiate damageEffect on each target, maybe arc from target or drop from sky.  Use [X]modEffectDamage
                 * 
                 */
            }
        }
    }
}

