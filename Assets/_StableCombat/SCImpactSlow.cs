using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCImpactSlow : SCImpact {
    public float duration = 5;
    public float speedChange = -7;
    public float range = 5;
    public override SCImpact Impact(int team = 0) {
        base.Impact(team);
        foreach (Collider col in Physics.OverlapSphere(transform.position, range)) {
            Debug.Log("#Impact#Slow " + col.transform.name);
            var ssc = col.GetComponent<StableCombatChar>();
            if (ssc == null) { Debug.Log("#Impact#Slow null " + col.name); continue; }
            if (ssc.team == team) { Debug.Log("##Impact#Slow " + ssc.team + "  " + team); continue; }

            Debug.Log("#Impact#Slow Damage" + ssc.name);
            ssc.SpeedBuff(duration, speedChange);
        }
        return this;
    }

   
}
