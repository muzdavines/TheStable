using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessGamble : OnStepSuccess {
    public override void OnSuccess() {
        print("GAMBLE SUCCESS!");
        if (GetComponent<MissionPOI>().isFinalForMission) { FindObjectOfType<MissionFinalDetails>().successful = true; }
        FindObjectOfType<MissionController>().update.text += "\nGambling successful! You received a [ENTER REWARD].";
        foreach (var p in affectedPOI) {
            p.step.mod += affecttedPOIMod;
        }
    }
}
