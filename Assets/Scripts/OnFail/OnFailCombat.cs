using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailCombat : OnStepFail {
    public Transform speaker;
    public override void OnFail() {
        //speaker = GetComponent<MissionPOI>().allPurposeTransforms[0];
        MissionController control = FindObjectOfType<MissionController>();
        FindObjectOfType<MissionFinalDetails>().successful = false;
        control.update.text += "\nYour Stable has taken a blow to its reputation today.";
    }
}
