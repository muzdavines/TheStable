using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCOneTimerToGoal : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.OneTimer();
        thisChar.transform.LookAt(thisChar.enemyGoal.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    public override void BallCollision(Collision collision) {
        ball.lastHolder = thisChar;
        ball.Shoot(thisChar.enemyGoal, 0, 1);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
