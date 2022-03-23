using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBackOffCarrier : StableCombatCharState {
    float debugID;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (thisChar.fieldPosition == Position.GK) {
            thisChar.GKIdle();
            return;
        }
        debugID = Random.Range(0, 100000);
        Debug.Log("#Backoff# Entered " + thisChar.tackleCooldown + "  " + Time.time + " " + debugID);
        thisChar.anima.Idle();
    }
    public override void Update() {
        base.Update();
        if (Time.time >= thisChar.tackleCooldown) {
            Debug.Log("#Backoff# TimedOut " + thisChar.tackleCooldown + "  " + Time.time + " " + debugID);
            thisChar.Idle();
        }
        if (ball.holder == null || ball.holder.team == thisChar.team) {
            Debug.Log("#Backoff# HolderNullOrTeam " + thisChar.tackleCooldown + "  " + Time.time + " " + debugID);
            thisChar.Idle();
            return;
        }
        thisChar.agent.isStopped = false;
        //if (Time.frameCount % 15 != 0) { return; }
        
        Vector3 a = ball.transform.position;

        Vector3 targetPos = ball.holder.position + ball.holder.transform.forward * 3;
        thisChar.agent.SetDestination(targetPos);
        //thisChar.transform.LookAt(b);
        // thisChar.transform.rotation = Quaternion.Euler(0, thisChar.transform.rotation.y, 0);

        
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
