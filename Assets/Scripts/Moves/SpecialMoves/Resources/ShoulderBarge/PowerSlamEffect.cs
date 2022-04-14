using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSlamEffect : MonoBehaviour {
    [SerializeField]
    public StableDamage damage;
    StableCombatChar caster;
    void Start() {
        Destroy(gameObject, 5);
    }

    public void Init(StableCombatChar _caster, StableDamage _damage) {
        caster = _caster;
        damage = _damage;
        foreach (Collider col in Physics.OverlapSphere(transform.position, 5)) {
            Debug.Log("#SpecialAbility#Power Slam " + col.transform.name);
            var ssc = col.GetComponent<StableCombatChar>();
            if (ssc == null) { Debug.Log("#SpecialAbility#Power Slam null " + col.name); continue; }
            if (ssc.team == caster.team) { Debug.Log("#SpecialAbility#Power Slam " + ssc.team + "  " + caster.team); continue; }
                
            Debug.Log("#SpecialAbility#Power Slam Damage" + ssc.name);
            damage.isKnockdown = true;
            ssc.TakeDamage(damage);
        }
    }
 }