using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCDivineIntervention : StableCombatCharState {
    private float timeOut;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        timeOut = Time.time + 2;
        thisChar.anima.DivineIntervention();
        thisChar.DisplaySpecialAbilityFeedback("Divine Intervention");
        thisChar.agent.TotalStop();
        CheckProjectiles();
    }

    public override void Update() {
        base.Update();
        thisChar.agent.TotalStop();
        CheckProjectiles();
    }
    void CheckProjectiles() {
        var tempProjs = GameObject.FindObjectsOfType<SCProjectile>();
        Debug.Log("#DivineIntervention#Proj Count: "+tempProjs.Length);
        foreach (var t in tempProjs) {
            if (t == null) {
                continue;
            }
            if (t.launcherChar.team == thisChar.team) {
                continue;
            }
            if (t.targetChar != thisChar) {
                GameObject.Destroy(t.gameObject);
                t.targetChar.DisplaySpecialAbilityFeedback("Divine Intervention");
                Debug.Log("#DivineIntervention#Destroying");
                continue;
            }
            Debug.Log("#DivineIntervention#Reflecting");
            if (Vector3.Distance(t.transform.position, thisChar.position) < 1.5f) {
                t.targetChar = t.launcherChar;
                t.launcherChar = thisChar;
                Debug.Log("#DivineIntervention#New Target: " + t.targetChar.name + " New Launcher: " + t.launcherChar.name);
            }
        }
    }
    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
