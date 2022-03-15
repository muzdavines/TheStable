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
    }
    public override void Update() {
        base.Update();
        if (Time.frameCount % 15 != 0) { return; }
        if (ball.holder == null || ball.holder.team == thisChar.team) { thisChar.Idle(); }
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
        Debug.Log("#TODO# Check if should be backwards running");

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        
    }

    

    public override void WillExit() {
        base.WillExit();
       
    }

}

public enum GuardNetPosition { Center, Left, Right, None}
