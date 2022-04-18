using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StableCombatCharState {
    public StableCombatCharStateOwner owner;
    public StableCombatChar thisChar;
    public Ball ball;
    public bool canGrabBall = true;
    public bool checkForIdle = false;

    const float oneTimerDistanceToGoal = 20f;
    const float oneTimerDistanceToBall = 1.8f;
    
    public string name {
        get { return this.GetType().ToString(); }
    }
    public static string AppStateChangedNotification = "StableCombatCharStateChangedNotification";
    public virtual void TransitionTo(StableCombatCharState state) {
        if (!state.IsApexState() && thisChar.isStateLocked) {
            Debug.LogError("Attempting to Transition to " + state.GetType() + " but current state is " + owner.state.GetType());
            return;
        }
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
        thisChar.anima.shouldBackpedal = false;
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

    public bool CheckOneTimer() {
        //int _rand = Random.Range(0, 1000000);
        //Debug.Log("#OneTimer#player Dist to Goal: " + thisChar.enemyGoal.Distance(thisChar) + " vs " + oneTimerDistanceToGoal + "  " + _rand);
        if (ball.isHeld) { return false; }
        if (thisChar.enemyGoal.Distance(thisChar) < oneTimerDistanceToGoal) {
            //Debug.Log("#OneTimer#ball Dist to player: " + ball.Distance(thisChar) + " vs " + oneTimerDistanceToBall + "  " + _rand);
            if (ball.Distance(thisChar) < oneTimerDistanceToBall) {
                //Debug.Log("#OneTimer#OneTime " + _rand);
                thisChar.OneTimerToGoal();
                return true;
            }
        }
        return false;
    }

    public SCResolution TryBlock(StableCombatChar blocker) {
        var res = new SCResolution();
        int tackling = blocker.myCharacter.blocking;
        int dodging = thisChar.myCharacter.carrying;
        //float roll = Random.Range(0, dodging + 1) - Random.Range(0, tackling + 1);
        //Debug.Log("#DiceRoll#Dodge Roll: " + roll);
        //if (roll >= 0) { res.success = false; thisChar.DodgeTackle(blocker); } else { res.success = true; thisChar.GetTackled(blocker); }
        if (dodging>=tackling) { res.success = false; thisChar.DodgeTackle(blocker); } else { res.success = true; thisChar.GetTackled(); }

        return res;
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

    public bool CheckSpecials() {
        foreach (SpecialMove m in thisChar.myCharacter.activeSpecialMoves) {
            if (m.Check(thisChar)) {
                return true;
            }
        }
        return false;
    }

    public virtual void Update() {
        if (Time.frameCount % 10 == 0) {
            if (thisChar.isKnockedDown) { return; }
            foreach (var special in thisChar.myCharacter.activeSpecialMoves) {
                if (special.Check(thisChar)) {
                    return;
                }
            }
            if (thisChar != null && thisChar.fieldSport && canGrabBall) {
                TryGrabBall();
            }
        }
    }

}


public interface StableCombatCharStateOwner {
    StableCombatCharState state { get; set; }
    StableCombatChar controller { get; set; }
}

public interface SCReviveUnit {


}

public interface ApexState {

}

public static class SCCSHelper {
    public static bool IsApexState(this StableCombatCharState thisState) {
        return thisState.GetType().GetInterfaces().Contains(typeof(ApexState));
    }
}
