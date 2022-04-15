using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPursueBallCarrier : StableCombatCharState
{
    bool speedBoosted;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (Time.time < thisChar.tackleCooldown) {
            thisChar.BackOffCarrier(false);
            return;
        }
    }
    public override void Update() {
        base.Update();
        if (thisChar.ball.holder == null) {
            thisChar.Idle();
            return;
        }
        Vector3 holderPos = thisChar.ball.holder.transform.position;
        if (Vector3.Distance(thisChar.transform.position, thisChar.ball.holder.transform.position) <= 2f) {
            var resolution = thisChar.state.SendMessage(thisChar.ball.holder, "TryTackle");
            if (resolution == null) {
                Debug.Log("#Tackle# Resolution Null");
                thisChar.MissTackle();
            } else {
                switch (resolution.tackleType) {
                    case TackleType.Strip:
                        if (resolution.success) {
                            thisChar.SuccessStrip();
                        } else {
                            thisChar.FailStrip();
                        }
                        break;
                    case TackleType.Tackle:
                        if (resolution.success) {
                            thisChar.Tackle();
                        }
                        else {
                            thisChar.MissTackle();
                        }
                        break;
                }
            }
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
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        if (message == "TryBlock") {
            return TryBlock(sender);
        }
        else return null;
    }

    
}
