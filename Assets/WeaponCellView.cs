using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
public class WeaponCellView : EnhancedScrollerCellView {
    public Text weaponNameText;
    public Weapon thisWeapon;
    public HeroEditController control;

    public void SetData(Weapon data) {
        control = FindObjectOfType<HeroEditController>();
        thisWeapon = data;
        weaponNameText.text = "<b><size=30>" + data.itemName.Space() + "</size></b>\n" + GetStats();
    }
    public string GetStats() {
        if (control == null || control.activeCharacter == null) { return ""; }
        return "";
        //return "STAM: " + thisMove.staminaDamage + " BAL: " + thisMove.balanceDamage + " MIND: " + thisMove.mindDamage + " HLTH: " + thisMove.healthDamage + "\nPhysical: " + thisMove.keyPhysicalAttribute + " (" + control.activeCharacter.GetCharacterAttributeValue(thisMove.keyPhysicalAttribute) + ")\n" + "Technical: " + thisMove.keyTechnicalAttribute + " (" + control.activeCharacter.GetCharacterAttributeValue(thisMove.keyTechnicalAttribute) + ")";
    }
    public void OnHoverEnter() {

        //GameObject.FindObjectOfType<HeroInfoPanelController>().OnHover(thisChar);


    }
    public void OnHoverExit() {

        //GameObject.FindObjectOfType<HeroInfoPanelController>().OnHoverExit();
    }
    public virtual void OnClick() {
        control.ListWeaponClicked(thisWeapon);
    }
}
