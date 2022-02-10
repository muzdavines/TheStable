using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSkillState : StableCombatCharState
{
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
}
