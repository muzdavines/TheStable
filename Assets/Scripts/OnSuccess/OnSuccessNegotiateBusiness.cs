using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessNegotiateBusiness : OnStepSuccess {
    Transform speaker;
    public override void OnSuccess() {
        speaker = GetComponent<MissionPOI>().allPurposeTransforms[0];
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nThe party succesfully negotiated the contract.";
        Helper.Speech(speaker, "I'm glad we could come to an agreement!\nSend me a contract and we will finalize the details.", 2f);
    }
}
