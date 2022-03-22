using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBackOffCarrier : StableCombatCharState {
    float timeOut = 0f;
    float defaultTime = 5f;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (timeOut == 0) {
            timeOut = Time.time + defaultTime;
        }
    }
    public override void Update() {
        base.Update();
        if (Time.time >= timeOut) {
            thisChar.Idle();
        }
        if (thisChar.ball.holder == null) {
            thisChar.Idle();
            return;
        }
        thisChar.agent.isStopped = false;
        if (Time.frameCount % 30 != 0) { return; }
        Vector3 b = thisChar.myGoal.transform.position;
        Vector3 a = ball.transform.position;

        Vector3 targetPos = a + (b - a).normalized * 3f;
        thisChar.agent.SetDestination(targetPos);
        //thisChar.transform.LookAt(b);
        // thisChar.transform.rotation = Quaternion.Euler(0, thisChar.transform.rotation.y, 0);

        if (ball.holder == null || ball.holder.team == thisChar.team) {
            thisChar.Idle();
            return;
        }
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
