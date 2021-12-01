using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VikingCrewTools;

public class MissionCharacterState
{
    public MissionCharacterStateOwner owner;
    public Animator anim;
    public MissionCharacter thisChar;
    public NavMeshAgent agent;
    public List<Roll> playerScore = new List<Roll>();
    public List<Roll> otherScore = new List<Roll>();
    public MissionPOI poi;
    public int scoreIndex = -1;
    public string name {
        get {
            return this.GetType().ToString();
        }
    }
    public static string AppStateChangedNotification = "MissionCharacterStateChangedNotification";
    public void TransitionTo(MissionCharacterState state) {
        owner.state = state;
        state.owner = owner;
        this.WillExit();
        state.EnterFrom(this);
        if (Application.isEditor) {
            Debug.Log("#StateChange# "+AppStateChangedNotification + " " + state);
        }
        
    }

    public virtual void EnterFrom(MissionCharacterState state) {
        thisChar = owner.controller;
        anim = thisChar.GetComponent<Animator>();
        agent = thisChar.GetComponent<NavMeshAgent>();
        if (state != null) {
            if (Application.isEditor) {
                Debug.Log ("#StateChange# EnterFrom: " + state.name + "  " + this.name);
            }
        }
        else {
            if (Application.isEditor) {
                Debug.Log ("#StateChange# EnterFrom null" + "  " + this.name);
            }

        }
    }
    public virtual void ProcessBuzz() {
        if (scoreIndex < 0) {
            scoreIndex++;
        }
        else {
            if (scoreIndex < playerScore.Count) {
                poi.control.buzz.Display(playerScore[scoreIndex], otherScore[scoreIndex], scoreIndex++);
            }
        }
    }

    public virtual void WillExit() {
        Helper.Cam().SetTarget(thisChar.shoulderCam.transform);
    }
    public virtual void StartAnim() {

    }
    public virtual void AnimEventReceiver(string message) {
        Debug.Log("#Anim# Anim Event Received by " + name + " " + message);
    }
    public virtual void Update() {
        if (Time.frameCount % 10 == 0) {

        }
    }
}

public interface MissionCharacterStateOwner {
    MissionCharacterState state { get; set; }
    MissionCharacter controller { get; set; }
}
