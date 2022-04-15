using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSummonFireGolem : SCCombatStanceState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.anima.Summon();
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SpawnEffect") {
            GameObject effect = Resources.Load<GameObject>("SummonFireGolem");
            var effectObj = GameObject.Instantiate(effect);
            effectObj.transform.position = thisChar.position + new Vector3(1, 0, 0);
            effectObj.GetComponent<SummonFireGolemEffect>().Init(thisChar, new StableDamage());
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}