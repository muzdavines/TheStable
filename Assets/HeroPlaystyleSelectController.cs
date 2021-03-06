using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroPlaystyleSelectController : MonoBehaviour
{

    public Button combatSelectorMelee, combatSelectorRanged;
    public Button styleSelectorFight, styleSelectorPlay;
    public HeroUIController controller;
    public StableCombatChar myChar;
    public void SelectPlaystyle(string s) {
        print("#TODO#Add method to switch playstyle, with Feel");
        switch (s) {
            case "Fight":
                styleSelectorFight.GetComponent<Image>().color = Color.white;
                styleSelectorPlay.GetComponent<Image>().color = Color.grey;
                myChar.SetPlayStyle(PlayStyle.Fight);
                break;
            case "Play":
                styleSelectorFight.GetComponent<Image>().color = Color.grey;
                styleSelectorPlay.GetComponent<Image>().color = Color.white;
                myChar.SetPlayStyle(PlayStyle.Play);
                break;
        }
    }

    public void SelectCombatStyle(string s) {
        print("#TODO#Add method to switch combat style, with Feel");
        switch (s) {
            case "Melee":
                combatSelectorMelee.GetComponent<Image>().color = Color.white;
                combatSelectorRanged.GetComponent<Image>().color = Color.grey;
                myChar.SetCombatFocus(CombatFocus.Melee);
                break;
            case "Ranged":
                combatSelectorMelee.GetComponent<Image>().color = Color.grey;
                combatSelectorRanged.GetComponent<Image>().color = Color.white;
                myChar.SetCombatFocus(CombatFocus.Ranged);
                break;
        }
    }

    public void SetPlaystyle(PlayStyle _playStyle) {
        switch (_playStyle) {
            case PlayStyle.Fight:
                styleSelectorFight.GetComponent<Image>().color = Color.white;
                styleSelectorPlay.GetComponent<Image>().color = Color.grey;
                break;
            case PlayStyle.Play:
                styleSelectorFight.GetComponent<Image>().color = Color.grey;
                styleSelectorPlay.GetComponent<Image>().color = Color.white;
                break;
        }
    }

    public void SetCombatStyle(CombatFocus _combatStyle) {
        switch (_combatStyle) {
            case CombatFocus.Melee:
                combatSelectorMelee.GetComponent<Image>().color = Color.white;
                combatSelectorRanged.GetComponent<Image>().color = Color.grey;
                break;
            case CombatFocus.Ranged:
                combatSelectorMelee.GetComponent<Image>().color = Color.grey;
                combatSelectorRanged.GetComponent<Image>().color = Color.white;
                break;
        }
    }

    public void Init(StableCombatChar _myChar) {
        myChar = _myChar;
        SetCombatStyle(myChar.combatFocus);
        SetPlaystyle(PlayStyle.Play);
    }
}
