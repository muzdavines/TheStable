using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableContractCellView : ContractCellView
{
    
    public override void OnClick() {
        FindObjectOfType<LaunchMissionController>().OpenMissionPanel(thisContract);
    }
}
