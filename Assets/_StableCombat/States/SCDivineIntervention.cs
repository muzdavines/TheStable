using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCDivineIntervention : StableCombatCharState {
    List<SCProjectile> projs;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.DivineIntervention();
        thisChar.DisplaySpecialAbilityFeedback("Divine Intervention");
        thisChar.agent.TotalStop();
        var tempProjs = GameObject.FindObjectsOfType<SCProjectile>();
        foreach (var t in tempProjs) {
            if (t == null) { continue; }
            if (t.launcherChar.team == thisChar.team) {
                continue;
            }
            projs.Add(t);
        }
    }

    public override void Update() {
        base.Update();
        thisChar.agent.TotalStop();
        foreach (var p in projs) {
            if (p == null) { continue; }
            if (p.launcherChar.team == thisChar.team) { continue; }
            if (p.targetChar != thisChar) {
                GameObject.Destroy(p.gameObject);
                continue;
            } else {
                if (Vector3.Distance(p.transform.position, thisChar.position) < 1.5f) {
                    p.targetChar = p.launcherChar;
                    p.launcherChar = thisChar;
                    Debug.Log("#DivineInt#New Target: " + p.targetChar.name + " New Launcher: " + p.launcherChar.name);
                }
            }
        }
        projs.RemoveAll(x => x == null);
        if (projs.Count == 0) {
            thisChar.Idle();
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
