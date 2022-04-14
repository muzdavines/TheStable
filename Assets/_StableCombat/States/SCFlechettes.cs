using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCFlechettes : SCCombatStanceState {

    public int level = 1;
    public StableDamage damage;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.Flechettes();
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SpawnEffect") {
            GameObject effect = Resources.Load<GameObject>("Flechettes");
            var effectObj = GameObject.Instantiate(effect);
            effectObj.transform.parent = thisChar.transform;
            effectObj.transform.localPosition = Vector3.zero;
            effectObj.transform.parent = null;
            effectObj.GetComponent<FlechettesEffect>().Init(thisChar, damage);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}