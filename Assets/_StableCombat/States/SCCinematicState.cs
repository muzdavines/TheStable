using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SCCinematicState : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.Seated();
        GameObject.Destroy(thisChar.transform.Find("GKBlocker(Clone)").gameObject);
    }

    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}