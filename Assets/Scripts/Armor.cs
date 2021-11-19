using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Armor : Item
{
    public int defense; //armor rating for damage calc. affected by condition 0-100 pct
    public int health; //adds dots
    public float defenseValue { get { return defense * (condition*.01f); } }
    public void DamageCondition(float amount) {
        condition -= amount;
        Mathf.Clamp(condition, 0, 100);
    }
}
