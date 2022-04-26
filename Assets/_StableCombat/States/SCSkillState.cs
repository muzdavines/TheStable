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
        Debug.Log("#Mission#ProcessBuzz");
        if (scoreIndex < 0) {
            scoreIndex++;
        }
        else {
            Debug.Log("#Buzz#Score Index: " + scoreIndex + " playerScore Count: " + playerScore.Count);
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
        int numberOfDice = Mathf.Clamp(skill,1,20);
        float comp = 0;
        float crit = Random.Range(1, 101);
        int bestRoll = 0;
        for (int i = 0; i<numberOfDice; i++) {
            var thisRoll = Random.Range(1, 21);
            if (thisRoll > comp) {
                comp = thisRoll;
                bestRoll = thisRoll;
            }
        }
        comp += (skill * 4);
        comp += skill == 0 ? -5 : 0;
        comp = Mathf.Clamp(comp, 1, 200);
        Debug.LogFormat("#Player Roll#Skill:{0}  Roll: {1}  Value: {2} Crit: {3}", skill, bestRoll, comp, crit);
        if (crit <= 1) {
            comp = 100;
        }
        return comp;
    }
}
