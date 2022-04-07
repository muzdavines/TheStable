using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCOneTimerToGoal : StableCombatCharState {

    bool kickReady = false;
    bool shotFired = false;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.OneTimer();
        thisChar.transform.LookAt(thisChar.enemyGoal.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
        if (ball.Distance(thisChar) < 1.8f && kickReady) {
            Shoot();
        }
    }

    public override void AnimEventReceiver(string message) {
        if (message == "OneTimer") {
            kickReady = true;
        }
        base.AnimEventReceiver(message);
    }
    public override void BallCollision(Collision collision) {
        Shoot();
    }
    public void Shoot() {
        if (shotFired) { return; }
        shotFired = true;
        ball.lastHolder = thisChar;
        ball.Shoot(thisChar.enemyGoal.transform.position + (thisChar.enemyGoal.transform.right * Random.Range(2f, 2.5f) * (Random.Range(0f, 1f) > .5f ? 1 : -1)), 0, 1);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
