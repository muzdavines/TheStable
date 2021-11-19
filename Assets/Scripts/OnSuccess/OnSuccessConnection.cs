using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessConnection : OnStepSuccess
{
    public Transform speaker;
    public override void OnSuccess()
    {
        speaker = GetComponent<MissionPOI>().allPurposeTransforms[1]; 
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nThe contact trusted the party and gave them the lead.";

        Helper.Speech(speaker, "You're looking for De Medici. He's in the tavern at the back table.", 2f);
    }

}
