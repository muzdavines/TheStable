using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
public class TacticsHeroCellView : EnhancedScrollerCellView {
    public Text heroNameText;
    public Character thisChar;
    public TeamTacticsController tactics;
    public void SetData(Character data) {
        thisChar = data;
        heroNameText.text = data.name;
        tactics = FindObjectOfType<TeamTacticsController>();
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
        if (!thisChar.activeInLineup) {
            if (Game.instance.playerStable.NumberHeroesInLineup() >= 5 || !thisChar.IsAvailable()) {
                print("Max Heroes Reached or Hero Not Available");
                return;
            }
        }
        thisChar.activeInLineup = !thisChar.activeInLineup;

        foreach (MissionHeroesScrollerController mhsc in FindObjectsOfType<MissionHeroesScrollerController>()) {
            mhsc.OnEnable();
        }
        OnHoverExit();

    }
}
