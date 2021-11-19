using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;

public class FreeAgentCellView : EnhancedScrollerCellView {
    public Text heroNameText;
    public Character thisChar;
    public void SetData(Character data) {
        thisChar = data;
        heroNameText.text = data.name;
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
        if (Game.instance.playerStable.PurchaseHero(thisChar)) {
            FindObjectOfType<StableManagementController>().UpdateHeader();
            OnHoverExit();
            Game.instance.freeAgentMarket.market.Remove(thisChar);
            Destroy(gameObject);
        }
    }
}
