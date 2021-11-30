using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;
public class ActiveWeaponCellView : WeaponCellView {
    public override void OnClick() {
        control.ActiveWeaponClicked();
    }
}