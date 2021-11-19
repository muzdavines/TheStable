using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;

public class MissionPreferencesCellView : EnhancedScrollerCellView {
    public Text pref;
    public Stage thisStage;
    
    public void SetData(Stage data) {
        thisStage = data;
        UpdateText();
    }
    public void UpdateText() {
        //pref.text = "Test";
        //pref.text = thisStage.description;
        pref.text = "<b>" + thisStage.type.ToString() + "</b>\n" + thisStage.description + "\n<i>Preferred Approach: " + thisStage.options[thisStage.preferredMethod]+"</i>";

    }
    public void OnHoverEnter() {
        

    }
    public void OnHoverExit() {
        
    }
    public void OnClick() {
        thisStage.preferredMethod += 1;
        if (thisStage.preferredMethod >= thisStage.options.Count) { thisStage.preferredMethod = 0; }
        UpdateText();

        /*if (!thisChar.activeForNextMission) {
            if (Game.instance.playerStable.NumberActiveHeroes() >= missionController.thisContract.maxHeroes) {
                print("Max Heroes Reached");
                return;
            }
        }
        thisChar.activeForNextMission = !thisChar.activeForNextMission;

        foreach (MissionHeroesScrollerController mhsc in FindObjectsOfType<MissionHeroesScrollerController>()) {
            mhsc.OnEnable();
        }
        OnHoverExit();
        */
    }
}