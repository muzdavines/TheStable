using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPursueBallCarrier : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null) {
            thisChar.Idle();
            return;
        }
        Vector3 myPos = thisChar.transform.position;
        Vector3 holderPos = thisChar.ball.holder.transform.position;
        if (Vector3.Distance(thisChar.transform.position, thisChar.ball.holder.transform.position)<=1) {
            thisChar.Tackle();
            thisChar.ball.holder.GetTackled();
        }
        thisChar.agent.SetDestination(holderPos);
        thisChar.agent.isStopped = false;
        
        ///if in range, try tackle. Might need messaging here, or just fire it. for now just do 50/50, pass tackle, fail animate a fall and stop the tackler for a bit

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
