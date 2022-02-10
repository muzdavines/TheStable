using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
using TMPro;
public class MissionHeroCellView : EnhancedScrollerCellView {
    public Text heroNameText;
    public Character thisChar;
    public LaunchMissionController missionController;
    public TeamTacticsController tacticsController;
    public bool tactics;
    public TMP_Dropdown myDropdown;
    public void SetData(Character data) {
        thisChar = data;
        heroNameText.text = data.name + (tactics ? "  ("+data.archetype.ToString()+")" : "");
        missionController = FindObjectOfType<LaunchMissionController>();
        tacticsController = FindObjectOfType<TeamTacticsController>();
        if (myDropdown != null) {
            myDropdown.value = (int)thisChar.currentPosition;
            if (!thisChar.activeInLineup) { myDropdown.gameObject.SetActive(false); }
        }
        
    }

    public void OnHoverEnter() {
        print("MouseEnter " + thisChar.name);
        GameObject.FindObjectOfType<HeroInfoPanelController>().OnHover(thisChar);


    }
    public void OnHoverExit() {
        print("MouseExit" + thisChar.name);
        GameObject.FindObjectOfType<HeroInfoPanelController>().OnHoverExit();
    }
    public void OnClick() {
        if (tactics) {
            if (!thisChar.activeInLineup) {
                if (Game.instance.playerStable.NumberHeroesInLineup() >= 5 || !thisChar.IsAvailable()) {
                    print("Max Heroes Reached or Hero Not Available");
                    return;
                }
            }
            thisChar.activeInLineup = !thisChar.activeInLineup;
        }
        else {
            if (!thisChar.activeForNextMission) {
                if (Game.instance.playerStable.NumberActiveHeroes() >= missionController.thisContract.maxHeroes || !thisChar.IsAvailable()) {
                    print("Max Heroes Reached or Hero Not Available");
                    return;
                }
            }
            thisChar.activeForNextMission = !thisChar.activeForNextMission;
        }
        if (!thisChar.activeForNextMission) { thisChar.currentPosition = Position.NA; }
        foreach (MissionHeroesScrollerController mhsc in FindObjectsOfType<MissionHeroesScrollerController>()) {
            mhsc.OnEnable();
            if (tacticsController != null) {
                tacticsController.OnEnable();
            }
        }
        OnHoverExit();

    }
    public void ChangePosition(int newPos) {
        tacticsController.ChangePosition((Position)(newPos), thisChar);
    }
}
