using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailConnection : OnStepFail
{
    public Transform speaker;
    public override void OnFail()
    {
        speaker = GetComponent<MissionPOI>().allPurposeTransforms[0];
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nThe contact did not trust the party. He has refused to speak further.";
        Helper.Speech(speaker, "Ummm. I'm not sure what you mean.", 2f);
    }


}
