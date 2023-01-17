using Animancer;
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu]
public class Move : ScriptableObject
{
    public string description;
    public float cooldown;
    public float maxRange = 5f;
    public float minRange = 0f;
    public int accuracy;
    public int movePointCost;
    public int staminaDamage;
    public int balanceDamage;
    public int mindDamage;
    public int healthDamage;
    public string keyPhysicalAttribute;
    public string keyTechnicalAttribute;
    public ClipTransition animation;
    public Limb limb;
    public MoveType moveType;
    public StableDamage damage { get { return new StableDamage() { balance = balanceDamage, health = healthDamage, mind = mindDamage, stamina = staminaDamage }; } }
    public MoveWeaponType moveWeaponType;
    public SCProjectile projectile;
    public virtual void HitEffect(Character attacker) {

    }

    public virtual string GetDescription() {
        return description;
    }

    public virtual string GetName() {
        return GetType().ToString().Title();
    }
    public List<string> modifiers = new List<string>();
    public void Update() {

    }
  
}
public enum Limb { RightHand, LeftHand, LeftLeg, RightLeg}
public enum MoveType { Melee, Ranged, Support }
[System.Flags]
public enum MoveWeaponType { None = 0, Fists = 1, Sword = 2, Axe = 4, Bow = 8, Pistol = 16, MageGloves = 32, Dagger = 64, Halberd = 128, Hammer = 256}

public class MoveSave {

    public string description;
    public float cooldown;
    public int accuracy;
    public int staminaDamage;
    public int balanceDamage;
    public int mindDamage;
    public int healthDamage;
    public string keyPhysicalAttribute;
    public string keyTechnicalAttribute;
    public Limb limb;
    public MoveType moveType;
    public MoveWeaponType moveWeaponType;
    public List<string> modifiers = new List<string>();
    public MoveSave CopyValues (Move source) {
        this.description = source.description;
        this.cooldown = source.cooldown;
        this.accuracy = source.accuracy;
        this.staminaDamage = source.staminaDamage;
        this.balanceDamage = source.balanceDamage;
        this.mindDamage = source.mindDamage;
        this.healthDamage = source.healthDamage;
        this.keyPhysicalAttribute = source.keyPhysicalAttribute;
        this.keyTechnicalAttribute = source.keyTechnicalAttribute;
        this.limb = source.limb;
        this.moveType = source.moveType;
        this.moveWeaponType = source.moveWeaponType;
        this.modifiers = source.modifiers;
        return this;
    }
}