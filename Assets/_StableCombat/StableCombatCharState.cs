using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableCombatCharState {
    public StableCombatCharStateOwner owner;
    public StableCombatChar thisChar;
    public Ball ball;
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
        if (message == "IdleStart") {
            thisChar.Idle();
        }
    }

    public virtual void ReceiveMessage(StableCombatChar sender, string message) {
        Debug.Log("#Message# Message received by " + thisChar.name + " from " + sender.name + ": " + message);
    }
    public virtual void SendMessage(StableCombatChar target, string message) {
        target.state.ReceiveMessage(thisChar, message);
    }
    public virtual void Update() {
        if (Time.frameCount % 10 == 0) {

        }
    }

}


public interface StableCombatCharStateOwner {
    StableCombatCharState state { get; set; }
    StableCombatChar controller { get; set; }
}
