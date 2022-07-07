using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCOneTimerToGoal : StableCombatCharState {

    bool kickReady = false;
    bool ballClose = false;
    bool shotFired = false;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ball.transform.position.y <= .2f) {
            thisChar.PickupBall();
            return;
        }
        thisChar.anima.OneTimer();
        thisChar.transform.LookAt(thisChar.enemyGoal.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
        Time.timeScale = .25f;
        // thisChar.matchController.ZoomCam(thisChar);
        thisChar.matchController.AddAnnouncerLine(thisChar.myCharacter.name + " Attempts the One Timer!");
    }

    public override void Update() {
        base.Update();
        if (ball.Distance(thisChar) < 2f) {
            ballClose = true;
        }
        if (kickReady && ballClose) {
            Shoot();
        }
    }

    public override void AnimEventReceiver(string message) {
        if (message == "SlowTimeOn") {
            Time.timeScale = .25f;
        }
        if (message == "SlowTimeOff") {
            Time.timeScale = 1f;
        }
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
        ball.ChangeHolder(thisChar);
        Vector3 shotTarget = thisChar.enemyGoal.transform.position;
        //if (Random.Range(0, 100) < thisChar.myCharacter.shooting) {
        if (true){
            shotTarget = thisChar.enemyGoal.topRight.position;
        }
        else {
            shotTarget += thisChar.enemyGoal.transform.right * Random.Range(2f, 2.5f) * (Random.Range(0f, 1f) > .5f ? 1 : -1);
        }
        Debug.Log("#OneTimer#Pos:"+ shotTarget);
        ball.Shoot(shotTarget, 0, 1.5f);
        Time.timeScale = 1f;
    }

    public override void WillExit() {
        base.WillExit();
        Time.timeScale = 1f;
       // thisChar.matchController.ZoomCamOff();
    }
}
