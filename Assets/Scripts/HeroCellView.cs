﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;

public class HeroCellView : EnhancedScrollerCellView
{
    public Text heroNameText;
    public Character thisChar;
    public Color defaultColor;
    public void SetData(Character data)
    {
        thisChar = data;
        heroNameText.text = data.name;
        GetComponent<Image>().color = Helper.GetCellViewColor();
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
        FindObjectOfType<HeroEditController>().OpenPanel(thisChar);
    }
}