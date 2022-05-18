using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShoulderBarge : SCCombatStanceState
{
    List<StableCombatChar> alreadyHit;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar._t.LookAt(thisChar.myAttackTarget.position);
        thisChar.anima.ShoulderBarge();
        alreadyHit = new List<StableCombatChar>();
    }
    public override void Update() {
        base.Update();
        foreach (Collider col in Physics.OverlapSphere(thisChar.position, 3)) {
            StableCombatChar scc = col.GetComponent<StableCombatChar>();
            if (scc == null) { continue; }
            if (scc.team == thisChar.team) { continue; }
            if (scc.isKnockedDown) { continue; }
            if (alreadyHit.Contains(scc)) { continue; }
            if (scc.myCharacter.archetype == Character.Archetype.Rogue || scc.myCharacter.archetype == Character.Archetype.Midfielder) {
                scc.GetTackled();
                alreadyHit.Add(scc);
                return;
            }
            scc.TakeDamage(new StableDamage() { stamina = 10 }, thisChar);
            alreadyHit.Add(scc);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
