using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCFlameCircle : SCCombatStanceState {

    public int level = 1;
    public StableDamage damage;
    
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.FlameCircle();
        thisChar.DisplaySpecialAbilityFeedback("Flame Circle");
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SpawnEffect") {
            GameObject effect = Resources.Load<GameObject>("FlameCircle");
            var effectObj = GameObject.Instantiate(effect);
            effectObj.transform.parent = thisChar.transform;
            effectObj.transform.localPosition = Vector3.zero;
            effectObj.GetComponent<FlameCircleEffect>().Init(thisChar, damage);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
