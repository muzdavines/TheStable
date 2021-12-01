using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailGamble : OnStepFail {
    public Transform speaker;
    public override void OnFail() {
        speaker = GetComponent<MissionPOI>().allPurposeTransforms[1];
        Helper.Speech(speaker, "The deciding roll goes to me!!! Sorry friend, wasn't your day.", 0f);
        MissionController control = FindObjectOfType<MissionController>();
        FindObjectOfType<MissionFinalDetails>().successful = false;
        control.update.text += "\nThe attempt at gambling failed. Confidence is shaken.";
        foreach (var p in affectedPOI) {
            p.step.mod += affecttedPOIMod;
        }
    }
}
