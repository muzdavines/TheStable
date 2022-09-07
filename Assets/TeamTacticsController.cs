using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamTacticsController : MonoBehaviour, UIElement
{
    public TacticsFieldPositionController[] positions;
    public MissionHeroesScrollerController[] scrollers;

    public void ChangePosition(Position pos, Character character) {
        if (pos == Position.GK && character.archetype != Character.Archetype.Goalkeeper) { return; }

        if (character.archetype == Character.Archetype.Goalkeeper && pos != Position.GK) { return; }

        //check if occupied
        Character swapPlayer = null;
        Position swapPosition = Position.NA;
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.activeInLineup && c.currentPosition == pos) {
                swapPlayer = c;
                swapPosition = c.currentPosition;
                break;
            }
        }
        if (character.currentPosition != Position.NA) {
            positions[((int)character.currentPosition) - 1].GetComponentInChildren<TextMeshProUGUI>().text = swapPlayer == null ? "" : swapPlayer.name;
            if (swapPlayer != null) {
                swapPlayer.currentPosition = character.currentPosition;
            }
        }
        else {
            if (swapPlayer != null) {
                swapPlayer.currentPosition = Position.NA;
            }
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
    public void Update() {
        if (Time.frameCount % 10 == 0) {
            //Init();
        }
    }
    public void DelayInit() {
        StartCoroutine(InitAfter(.5f));
    }
    IEnumerator InitAfter(float time) {
        yield return new WaitForEndOfFrame();
        Init();
    }
    public void UpdateOnAdvance() {
        OnEnable();
    }
    public void Init() {
        Debug.Log("Init Team Tactics Controller");
        ResetPositions();
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.activeInLineup) {
                ChangePosition(c.currentPosition, c);
            }
        }
    }
    
   
}
