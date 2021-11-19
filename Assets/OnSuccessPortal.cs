using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessPortal : OnStepSuccess {
    public override void OnSuccess() {
        print("PORTAL SUCCESS!");
        if (GetComponent<MissionPOI>().isFinalForMission) { FindObjectOfType<MissionFinalDetails>().successful = true; }
        //FindObjectOfType<MissionController>().update.text += "\nCombat Complete.";

    }
}