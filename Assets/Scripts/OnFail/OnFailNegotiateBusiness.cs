using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailNegotiateBusiness : OnStepFail {
    Transform speaker;
    public override void OnFail() {
        speaker = GetComponent<MissionPOI>().allPurposeTransforms[0];
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nThe party failed to reach an agreement.";
        Helper.Speech(speaker, "I'm not sure how you conduct business at your stable,\nbut that's not how we do things around here. No deal.", 2f);
    }
}
