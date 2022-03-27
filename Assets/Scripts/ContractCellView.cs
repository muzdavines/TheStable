using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
using TMPro;

public class ContractCellView : EnhancedScrollerCellView {
    public Text contractDescription, maxHeroes, diff, due;
    public TextMeshProUGUI title;
    public MissionContract thisContract;
    public void SetData(MissionContract data) {
        thisContract = data;
        thisContract.Init();
        contractDescription.text = data.description;
        maxHeroes.text = "Max Heroes: "+ data.maxHeroes;
        diff.text = "Difficulty: " + data.difficulty;
        due.text = "Due Date: " + data.executionDate.GetDateString();
        title.text = data.contractType.ToString();
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
