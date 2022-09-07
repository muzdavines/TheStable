using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;

public class HeroCellView : EnhancedScrollerCellView, UIElement
{
    public Text heroNameText;
    public Text shooting, passing, tackling, carrying, melee, ranged, magic, speed, dex, agi, str, meleeWeapon, rangedWeapon;
    public Text xp;
    public Text archetype;
    public Text nextAvailable;
    public Character thisChar;
    public Color defaultColor;
    public void SetData(Character data)
    {
        thisChar = data;
        heroNameText.text = data.name;
        shooting.text = "Shooting: "+data.shooting;
        passing.text = "Passing: "+data.passing;
        tackling.text = "Tackling: "+data.tackling;
        carrying.text = "Carrying: "+data.carrying;
        melee.text = "Melee: "+data.melee;
        ranged.text = "Ranged: "+data.ranged;
        magic.text = "Magic: "+data.magic;
        speed.text = "Speed: "+data.runspeed;
        dex.text = "Dexterity: "+data.dexterity;
        agi.text = "Agility: "+data.agility;
        str.text = "Strength: "+data.strength;
        meleeWeapon.text = "Melee Weapon: "+data.meleeWeapon?.itemName;
        rangedWeapon.text = "Ranged Weapon: "+data.rangedWeapon?.itemName;
        xp.text = "XP to Spend: " + data.xp;
        archetype.text = data.archetype.ToString();
        if (data.returnDate.IsOnOrAfter(Helper.Today(), false)) {
            nextAvailable.text = "Next available for contract:\n"+ data.returnDate.GetDateString();
        } else {
            nextAvailable.text = "";
        }
        GetComponent<Image>().color = Helper.GetCellViewColor();
    }
    public void UpdateOnAdvance() {
        SetData(thisChar);
    }

    public void OnHoverEnter()
    {
        print("MouseEnter "+thisChar.name);
        GameObject.FindObjectOfType<HeroInfoPanelController>().OnHover(thisChar);
        

    }
    public void OnHoverExit()
    {
        print("MouseExit"+thisChar.name);
        GameObject.FindObjectOfType<HeroInfoPanelController>().OnHoverExit();
    }
    public virtual void OnClick()
    {
        FindObjectOfType<HeroMainController>().OpenPanel(thisChar);
    }
}
