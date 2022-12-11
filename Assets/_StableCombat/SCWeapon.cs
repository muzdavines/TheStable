using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCWeapon : MonoBehaviour
{
    public Collider col;
    public AnimancerController anim;
    public StableCombatChar me;
    public Weapon weaponBlueprint;
    public Transform projectileSpawnPoint;
    private bool isHeavy;
    private int numHits;
    public List<StableCombatChar> hitThisTurn;
    void Start()
    {
        if (col == null) { col = GetComponent<Collider>(); }
        col.enabled = false;
        anim = GetComponentInParent<AnimancerController>();
        me = anim.GetComponent<StableCombatChar>();
        col.isTrigger = true;
    }
    public void Init(StableCombatChar character, Weapon _blueprint) {
        if (col == null) { col = GetComponentInChildren<Collider>(); }
        col.enabled = false;
        col.isTrigger = true;
        weaponBlueprint = _blueprint;
        me = character;
        anim = me.GetComponent<AnimancerController>();
        isHeavy = weaponBlueprint.isHeavy;
        hitThisTurn = new List<StableCombatChar>();
    }


    public void OnTriggerEnter(Collider other) {
        Debug.Log("#SCWeapon#"+transform.name+" Collision: " + other.name);
        StableCombatChar target = other.GetComponent<StableCombatChar>();
        if (target == null || target == me || target.team == me.team) { return; }

        if (hitThisTurn.Contains(target)) {
            return;}
        target.TakeDamage(anim.currentMeleeMove.damage, me);
        numHits++;
        hitThisTurn.Add(target);
        if (!isHeavy || numHits >= 4) {
            EndScan();
        }
    }

    public void Scan() {
        hitThisTurn = new List<StableCombatChar>();
        numHits = 0;
        col.enabled = true;
        Debug.Log("#SCWeapon#Scan "+name);
    }

    public void EndScan() {
        col.enabled = false;
        numHits = 0;
        Debug.Log("#SCWeapon#EndScan "+name);
    }

    public void FireProjectile(Move thisMove) {
        if (thisMove == null) { Debug.Log("No projectile"); return; }
        SCProjectile projectile = Instantiate<SCProjectile>(thisMove.projectile);
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.localRotation = projectileSpawnPoint.rotation;
        projectile.Fire(me.myAttackTarget._t, thisMove.damage, me);
    }


    void Update()
    {
        
    }
}
