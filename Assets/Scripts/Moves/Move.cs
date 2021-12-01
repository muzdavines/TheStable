using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu]
public class Move : ScriptableObject
{
    public string description;
    public int cooldown;
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
    public virtual void HitEffect(CharacterHealth health, Character attacker) {

    }

    public List<string> modifiers = new List<string>();
    

}

public enum MoveType { None = 0, RightJab = 1, LeftJab = 2, RoundhouseKick = 3, JumpFrontKick = 4, KidneyPunch = 5, Backhand = 6, SideKick = 7, HeartPunch = 8, Uppercut = 9, ElbowCruch = 10, DoubleFistPunch = 11, SwordThrust = 12, SwordHack = 13, BowShot = 1000, KnockdownBowShot = 1001 }
[System.Flags]
public enum MoveWeaponType { None = 0, Fists = 1, Sword = 2, Axe = 4, Bow = 8, Pistol = 16 }

public class MoveSave {

    public string description;
    public int cooldown;
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