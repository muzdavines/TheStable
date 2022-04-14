using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class StableDamage 
{
    public int stamina, balance, mind, health;
    public bool isKnockdown;
    public void Modify(float mod) {
        stamina = (int)(stamina * mod);
        balance = (int)(balance * mod);
        mind = (int)(mind * mod);
        health = (int)(health * mod);
    }
}
