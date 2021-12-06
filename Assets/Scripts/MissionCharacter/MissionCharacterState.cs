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
    public float threshold;
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
    public virtual float GetPlayerScore(int skill) {
        float diceRoll = Random.Range(1, 21);
        threshold = poi.step.level;
        int critRoll = Random.Range(1, 21);
        int critMod = 1;
        if (critRoll == 20) { critMod = 2; }
        if (critRoll == 1) { critMod = 0; }
        float comp = (diceRoll * (1 + poi.step.mod)) + (skill * critMod);
        float tempThreshold = threshold == 0 ? 1 : threshold;
        float playerMax = (skill * 2) + 20;
        Debug.Log("Dice Roll: " + diceRoll + "  CritRoll: " + critRoll + "  Mod: " + (1 + poi.step.mod) + "  Skill: " + skill + "  Total: " + comp);
        playerScore.Add(new Roll() { diceRoll = diceRoll, critRoll = critRoll, mod = 1 + poi.step.mod, skill = skill, total = comp, max = playerMax > threshold ? playerMax * 2 : threshold * 2, threshold = threshold });
        otherScore.Add(new Roll() { total = tempThreshold, max = playerMax > threshold ? playerMax * 2 : threshold * 2, threshold = threshold });
        return comp;
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
