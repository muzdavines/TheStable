using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCWeapon : MonoBehaviour
{
    public Collider col;
    public AnimancerController anim;
    public StableCombatChar me;
    public Weapon weaponBlueprint;
    void Start()
    {
        if (col == null) { col = GetComponent<Collider>(); }
        col.enabled = false;
        anim = GetComponentInParent<AnimancerController>();
        me = anim.GetComponent<StableCombatChar>();
        col.isTrigger = true;
    }
    public void Init(StableCombatChar character, Weapon _blueprint) {
        if (col == null) { col = GetComponent<Collider>(); }
        col.enabled = false;
        col.isTrigger = true;
        weaponBlueprint = _blueprint;
        me = character;
        anim = me.GetComponent<AnimancerController>();
    }


    public void OnTriggerEnter(Collider other) {
        Debug.Log("#SCWeapon#Collision: " + other.name);
        StableCombatChar target = other.GetComponent<StableCombatChar>();
        if (target == null || target == me) { return; }
        target.TakeDamage(anim.currentMove.damage);
    }

    public void Scan() {
        col.enabled = true;
        Debug.Log("#SCWeapon#Scan");
    }

    public void EndScan() {
        col.enabled = false;
        Debug.Log("#SCWeapon#EndScan");
    }


    void Update()
    {
        
    }
}
