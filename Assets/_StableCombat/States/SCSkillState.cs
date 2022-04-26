using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSkillState : StableCombatCharState {
    public MissionPOI poi;
    public int scoreIndex = -1;
    public float threshold;
    public List<Roll> playerScore = new List<Roll>();
    public List<Roll> otherScore = new List<Roll>();


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
    public void StopWalking() {
        thisChar.anima.Idle();
        if (poi != null) {
            thisChar.agent.transform.rotation = poi.targetPos.rotation;
        }
        thisChar.agent.isStopped = true;
    }
    public int[] skillArray = { 6, 10, 12, 14, 15 };
    public virtual float GetPlayerScore(int skill) {
        int numberOfDice = skill;
        float comp = 0;
        float crit = Random.Range(1, 101);
       
        for (int i = 0; i<numberOfDice; i++) {
            var thisRoll = Random.Range(1, 21);
            if (thisRoll > comp) {
                comp = thisRoll;
            }
        }
        comp += (skill * 4);
        if (crit <= 1) {
            comp = 100;
        }
        return comp;
    }
}
