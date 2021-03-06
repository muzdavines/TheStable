using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIntroState :  StableCombatCharState, CannotSpecial {

    public Transform myPos;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.transform.position = myPos.position;
        thisChar.transform.rotation = myPos.rotation;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar.anima.Idle();
    }
    public override void Update() {
        base.Update();
        thisChar.transform.position = myPos.position;
        thisChar.transform.rotation = myPos.rotation;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
