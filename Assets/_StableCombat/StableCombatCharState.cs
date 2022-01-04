using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableCombatCharState {
    public StableCombatCharStateOwner owner;
    public StableCombatChar thisChar;
    public Ball ball;
    public bool canGrabBall = true;
    public bool checkForIdle = false;
    public string name {
        get { return this.GetType().ToString(); }
    }
    public static string AppStateChangedNotification = "StableCombatCharStateChangedNotification";
    public void TransitionTo(StableCombatCharState state) {
        owner.state = state;
        state.owner = owner;
        thisChar = owner.controller;
        this.WillExit();
        state.EnterFrom(this);
        if (Application.isEditor) {
            Debug.Log("#StateChange# " + AppStateChangedNotification + " " + state);
        }

    }
    public virtual void EnterFrom(StableCombatCharState state) {
        thisChar = owner.controller;
        ball = thisChar.ball;
        if (state != null) {
            if (Application.isEditor) {
                Debug.Log("#StateChange# EnterFrom: " + state.name + "  " + this.name);
            }
        }
        else {
            if (Application.isEditor) {
                Debug.Log("#StateChange# EnterFrom null" + "  " + this.name);
            }

        }
    }
    public virtual void WillExit() {
        Debug.Log("#StateChange# ExitFrom " + this.name);
        //Helper.Cam().SetTarget(thisChar.shoulderCam.transform);
    }
    public virtual void AnimEventReceiver(string message) {
        Debug.Log("#Anim# Anim Event Received by " + name + " " + message);
    }
   
    public void TryGrabBall() {
        if (ball.Distance(thisChar) < .6f) {
            thisChar.PickupBall();
            return;
        }
    }

    public virtual void BallCollision(Collision collision) {
        if (canGrabBall) {
            thisChar.PickupBall();
        }
    }

    public virtual SCResolution ReceiveMessage(StableCombatChar sender, string message) {
        Debug.Log("#Message# Message received by " + thisChar.name + " from " + sender.name + ": " + message);
        return null;
    }
    public virtual SCResolution SendMessage(StableCombatChar target, string message) {
        return target.state.ReceiveMessage(thisChar, message);
    }
    public virtual void Update() {
        if (Time.frameCount % 10 == 0) {

        }
        if (canGrabBall) {
            TryGrabBall();
        }
    }

}


public interface StableCombatCharStateOwner {
    StableCombatCharState state { get; set; }
    StableCombatChar controller { get; set; }
}
