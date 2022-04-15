using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCClosingSpeed : StableCombatCharState {
    SCSpeedBuff speedBuff;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        speedBuff = thisChar.SpeedBuff(10, 5);
        GameObject effect = GameObject.Instantiate(Resources.Load<GameObject>("ClosingSpeedEffect"));
        effect.transform.parent = thisChar.transform;
        effect.transform.localPosition = Vector3.zero;
        GameObject.Destroy(effect, 8f);
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null) {
            thisChar.Idle();
            if (speedBuff != null) { speedBuff.EndEffect(); }
            return;
        }
        Vector3 holderPos = thisChar.ball.holder.transform.position;
        if (Vector3.Distance(thisChar.transform.position, thisChar.ball.holder.transform.position) <= 2f) {
            thisChar.Tackle();
            thisChar.ball.holder.GetTackled();
            if (speedBuff != null) { speedBuff.EndEffect(); }
            return;
        }

        thisChar.agent.SetDestination(holderPos);
        thisChar.agent.isStopped = false;
        ///if in range, try tackle. Might need messaging here, or just fire it. for now just do 50/50, pass tackle, fail animate a fall and stop the tackler for a bit
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        if (speedBuff != null) { speedBuff.EndEffect(); }
        base.WillExit();
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        return null;
    }


}