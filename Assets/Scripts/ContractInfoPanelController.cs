using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContractInfoPanelController : MonoBehaviour, PopupElement
{
    public Text description, contractType, difficulty, stages, goldReward, executionDate;
    public GameObject panel;
    public void OnHover(MissionContract c) {
        panel.SetActive(true);
        description.text = "Description: "+c.description;
        contractType.text = "Contract: "+c.contractType.ToString();
        difficulty.text = "Difficulty: "+c.difficulty.ToString();
        stages.text = "Stages: " + c.stages.Count;
        goldReward.text = "Payment: " + c.goldReward;
        executionDate.text = "Due Date: " + c.executionDate.GetDateString();


    }
    public void OnHoverExit() {
        panel.SetActive(false);
    }

    public void Close() {
        panel.SetActive(false);
    }
}
