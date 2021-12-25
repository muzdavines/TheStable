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
            var resolution = thisChar.state.SendMessage(thisChar.ball.holder, "TryTackle");
            bool didTackle = (resolution != null && resolution.success);
            if (didTackle) { thisChar.Tackle(); return; } else { thisChar.MissTackle(); return; }
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

    SCResolution TryBlock(StableCombatChar blocker) {
        var res = new SCResolution();
        int tackling = blocker.blocking;
        int dodging = thisChar.dodging;
        float roll = Random.Range(0, dodging + 1) - Random.Range(0, tackling + 1);
        Debug.Log("#DiceRoll#Dodge Roll: " + roll);
        if (roll >= 0) { res.success = false; thisChar.DodgeTackle(); } else { res.success = true; thisChar.GetTackled(); }
        return res;
    }
}
