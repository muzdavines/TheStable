using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSingleHeal : StableCombatCharState, CannotSpecial  {
    public StableCombatChar target;
    public bool healDelivered;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (target == null) { Debug.Log("#SingleHeal#Idle"); thisChar.Idle(); }
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(target.position);
        GameObject effect = Resources.Load<GameObject>("SmokeEffect");
        var fx1 = GameObject.Instantiate(effect, thisChar.position, thisChar.transform.rotation);
        GameObject.Destroy(fx1, 10);
       
        thisChar.transform.LookAt(target.transform);
        thisChar.anima.SingleHeal();
        thisChar.DisplaySpecialAbilityFeedback("Enemy Heal");

    }

    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (!healDelivered)
        {
            thisChar.transform.LookAt(target.transform);
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "ActivateHeal")
        {
            healDelivered = true;
            target.TakeHeals(new StableDamage() { balance = 10, mind = 10, stamina = 10}) ;
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}