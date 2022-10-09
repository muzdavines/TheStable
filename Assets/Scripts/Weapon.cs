using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Weapon : Item
{
    public int damage;
    public string prefabName;
    public bool dualWield, isHeavy;
    public bool usesLegs;
    public string prefabNameLegs;
    /// <summary>
    /// The prefab name that holds the settings to be copied
    /// </summary>
    public string settingsPrefab;
    public MoveWeaponType weaponType;
    public MoveType moveType;
    public int meleeAttackRange = -1;

}
