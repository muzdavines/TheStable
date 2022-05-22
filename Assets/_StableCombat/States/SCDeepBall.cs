using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCDeepBall : StableCombatCharState {
    public StableCombatChar passTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        
        //GameObject effect = GameObject.Instantiate(Resources.Load<GameObject>("DeepBallEffect"));
        thisChar.DisplaySpecialAbilityFeedback("Deep Ball");
        thisChar.anima.PassBall();
        thisChar.transform.LookAt(passTarget.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        //effect.transform.parent = thisChar.transform;
        //effect.transform.localPosition = Vector3.zero;
        //GameObject.Destroy(effect, 8f);
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.transform.LookAt(passTarget.transform);
        ///if in range, try tackle. Might need messaging here, or just fire it. for now just do 50/50, pass tackle, fail animate a fall and stop the tackler for a bit
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "PassBall") {
            ball.PassTo(passTarget, 1, false);
            passTarget.TryCatchPass();
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        return null;
    }


}