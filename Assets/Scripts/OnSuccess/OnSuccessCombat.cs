using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessCombat : OnStepSuccess {
    public override void OnSuccess() {
        print("COMBAT SUCCESS!");
        if (GetComponent<MissionPOI>().isFinalForMission) { FindObjectOfType<MissionFinalDetails>().successful = true; }
        FindObjectOfType<MissionController>().update.text += "\nCombat Complete.";

    }
}
