using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using UnityEngine.UI;

public class BusinessCellView : EnhancedScrollerCellView {
    public Text businessText;
    public Finance.Business thisBusiness;
    public void SetData(Finance.Business data) {
        thisBusiness = data;
        businessText.text = data.GetInfo();
    }

    public void OnHoverEnter() {
       // print("MouseEnter " + thisChar.name);
       // GameObject.FindObjectOfType<HeroInfoPanelController>().OnHover(thisChar);


    }
    public void OnHoverExit() {
      //  print("MouseExit" + thisChar.name);
       // GameObject.FindObjectOfType<HeroInfoPanelController>().OnHoverExit();
    }
    public void OnClick() {
       /* if (Game.instance.playerStable.PurchaseHero(thisChar)) {
            FindObjectOfType<StableManagementController>().UpdateHeader();
            OnHoverExit();
            Game.instance.freeAgentMarket.market.Remove(thisChar);
            Destroy(gameObject);
        }*/
    }
}
