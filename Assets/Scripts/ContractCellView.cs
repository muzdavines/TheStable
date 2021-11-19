using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;

public class ContractCellView : EnhancedScrollerCellView {
    public Text contractDescription;
    public MissionContract thisContract;
    public void SetData(MissionContract data) {
        thisContract = data;
        thisContract.Init();
        contractDescription.text = "Contract: " + data.contractType.ToString() + "            " + data.description + "            Difficulty: " + data.difficulty + "    Due Date: " + data.executionDate.GetDateString()+"   Max Heroes: "+data.maxHeroes;
    }

    public void OnHoverEnter() {
        print("MouseEnter " + thisContract.description);
        GameObject.FindObjectOfType<ContractInfoPanelController>().OnHover(thisContract);


    }
    public void OnHoverExit() {
        print("MouseExit" + thisContract.description);
        GameObject.FindObjectOfType<ContractInfoPanelController>().OnHoverExit();
    }
    public virtual void OnClick() {
        Game.instance.playerStable.AcceptContract(thisContract);
        Game.instance.contractMarket.Remove(thisContract);
        OnHoverExit();
        FindObjectOfType<ContractScrollerController>().OnEnable();
    }
}
