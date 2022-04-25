using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPowerSlam : SCCombatStanceState {

    public int level = 1;
    public StableDamage damage;

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.PowerSlam();
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SpawnEffect") {
            GameObject effect = Resources.Load<GameObject>("PowerSlam");
            var effectObj = GameObject.Instantiate(effect);
            effectObj.transform.position = thisChar.transform.position;
            effectObj.transform.rotation = thisChar.transform.rotation;
            if (damage == null) { damage = new StableDamage() { stamina = 8, isKnockdown = true }; }
            effectObj.GetComponent<PowerSlamEffect>().Init(thisChar, damage);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
