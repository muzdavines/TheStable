using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCImpactSplashHeals : SCImpact {
    public StableDamage healAmounts;
    public float range = 5;
    public override SCImpact Impact(int team = 0) {
        base.Impact(team);
        foreach (Collider col in Physics.OverlapSphere(transform.position, range)) {
            Debug.Log("#Impact#Splash Heals " + col.transform.name);
            var ssc = col.GetComponent<StableCombatChar>();
            if (ssc == null) { Debug.Log("#Impact#Slow null " + col.name); continue; }
            if (ssc.team != team) { Debug.Log("##Impact#Slow " + ssc.team + "  " + team); continue; }

            Debug.Log("#Impact#Splash Heals" + ssc.name);
            ssc.TakeHeals(healAmounts);
        }
        return this;
    }


}
