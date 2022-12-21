using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
using TMPro;
public class MissionHeroCellView : EnhancedScrollerCellView {
    public Text heroNameText;
    public Text nextAvailable;
    public Character thisChar;
    public LaunchMissionController missionController;
    public TeamTacticsController tacticsController;
    public bool tactics;
    public TMP_Dropdown myDropdown;
    public HeroHoverPopupController hover;
    public void SetData(Character data) {
        thisChar = data;
        heroNameText.text = data.name + (tactics ? "  ("+data.archetype.ToString()+")" : "");
        missionController = FindObjectOfType<LaunchMissionController>();
        tacticsController = FindObjectOfType<TeamTacticsController>();
        hover = FindObjectOfType<HeroHoverPopupController>();
        if (myDropdown != null) {
            myDropdown.value = (int)thisChar.currentPosition;
            if (!thisChar.activeInLineup) { myDropdown.gameObject.SetActive(false); }
        }
        if (data.returnDate.IsOnOrAfter(Helper.Today(), false)) {
            nextAvailable.text = "Next available: " + data.returnDate.GetDateString();
        }
        else {
            nextAvailable.text = "";
        }

    }

    public void OnHoverEnter() {
        print("MouseEnter " + thisChar.name);
        if (hover) {
            hover.Display(thisChar);
        }


    }
    public void OnHoverExit() {
        print("MouseExit" + thisChar.name);
        if (hover) {
            hover.Close();
        }
    }
    public void OnClick() {
        if (tactics) {
            return;
            if (Game.instance.playerStable.finance.gold < 0) {
                return;
            }
            if (!thisChar.activeInLineup) {
                if (Game.instance.playerStable.NumberHeroesInLineup() >= 6) {
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
        return;
        tacticsController.ChangePosition((Position)(newPos), thisChar);
        SetData(thisChar);
    }
}
