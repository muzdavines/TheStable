using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
public class MissionHeroCellView : EnhancedScrollerCellView {
    public Text heroNameText;
    public Character thisChar;
    public LaunchMissionController missionController;
    public void SetData(Character data) {
        thisChar = data;
        heroNameText.text = data.name;
        missionController = FindObjectOfType<LaunchMissionController>();
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
        if (!thisChar.activeForNextMission) {
            if (Game.instance.playerStable.NumberActiveHeroes() >= missionController.thisContract.maxHeroes || !thisChar.IsAvailable()) {
                print("Max Heroes Reached or Hero Not Available");
                return;
            }
        }
        thisChar.activeForNextMission = !thisChar.activeForNextMission;

        foreach (MissionHeroesScrollerController mhsc in FindObjectsOfType<MissionHeroesScrollerController>()) {
            mhsc.OnEnable();
        }
        OnHoverExit();

    }
}
