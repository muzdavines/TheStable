using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Move : ScriptableObject
{
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
public enum MoveWeaponType { Fists = 0, Sword = 1, Axe = 2, Bow = 3 }