using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCGuardNet : StableCombatCharState
{
    Transform playerWithBall;
    Transform myNet;
    public GuardNetPosition guardPosition;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        playerWithBall = ball.holder.transform;
        myNet = thisChar.myGoal.transform;
        thisChar.anima.shouldBackpedal = true;
    }
    public override void Update() {
        base.Update();
        if (thisChar.anima.shouldBackpedal) {
            thisChar.transform.LookAt(ball.transform);
        }
        thisChar.transform.rotation = Quaternion.Euler(new Vector3(0, thisChar.transform.rotation.eulerAngles.y, 0));
        if (Time.frameCount % 15 != 0) { return; }
        if (ball.holder == null || ball.holder.team == thisChar.team) { thisChar.Idle(); }
        if (guardPosition != GuardNetPosition.Center && ball.Distance(thisChar.myGoal.transform) < 24) {
            thisChar.PursueBallCarrier();
            return;
        }
        Vector3 targetPos = (playerWithBall.position + myNet.position) / 2;
        switch (guardPosition) {
            case GuardNetPosition.Left:
                targetPos += playerWithBall.transform.right * -5f;
                break;
            case GuardNetPosition.Right:
                targetPos += playerWithBall.transform.right * 5f;
                break;

        }
        thisChar.agent.isStopped = false;
        thisChar.agent.SetDestination(targetPos);
        if (Time.frameCount % 60 != 0) {
            if (Vector3.Distance(ball.transform.position, thisChar.myGoal.transform.position) > (Vector3.Distance(thisChar.position, thisChar.myGoal.transform.position))){
                thisChar.anima.shouldBackpedal = true;
            } else { thisChar.anima.shouldBackpedal = false; }
        }
        Debug.Log("#TODO# Check if should be backwards running");

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        
    }
    public override SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        base.ReceiveMessage(sender, message);
        if (message == "TryBlock") {
            return TryBlock(sender);
        }
        else return null;
    }


    public override void WillExit() {
        base.WillExit();
        thisChar.anima.shouldBackpedal = false;
    }

}

public enum GuardNetPosition { Center, Left, Right, None}
