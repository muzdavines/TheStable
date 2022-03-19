using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamTacticsController : MonoBehaviour
{
    public TacticsFieldPositionController[] positions;

    public void ChangePosition(Position pos, Character character) {
        if (pos == Position.GK && character.archetype != Character.Archetype.Goalkeeper) { return; }
        if (character.currentPosition != Position.NA) {
            positions[((int)character.currentPosition) - 1].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        if ((int)pos > 0) {
            positions[((int)pos) - 1].GetComponentInChildren<TextMeshProUGUI>().text = character.name;
        }
        character.currentPosition = pos;
    }
    public void OnEnable() {
        Init();
    }
    public void ResetPositions() {
        foreach (TacticsFieldPositionController t in positions) {
            t.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }
    public void Init() {
        ResetPositions();
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.activeInLineup) {
                ChangePosition(c.currentPosition, c);
            }
        }
    }
   
}
