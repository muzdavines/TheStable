using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterCombatState : MissionCharacterState
{
    public Transform FindEnemy() {
        List<MissionCharacter> theseChars = thisChar.missionController.allChars;
        foreach (var c in theseChars) {
            if (c.team == thisChar.team) { continue; }
            if (Vector3.Distance (c.transform.position, thisChar.transform.position) < thisChar.detectRange) {
                return c.transform;
            }
        }

        return null;
    }

    public Transform FindBall() {
        return null;
    }
    public Transform FindLeader() {
        return null;
    }
    public Transform FindPOI() {
        return null;
    }
    public bool InAttackRange() {

        if (target == null) { Debug.Log("Target is null for Range, returning."); return false; }
        return (Vector3.Distance(target.position, thisChar.transform.position) <= thisChar.maxAttackRange);
    }
}
