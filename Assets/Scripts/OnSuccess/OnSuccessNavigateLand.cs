using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessNavigateLand : OnStepSuccess
{
    public override void OnSuccess() {
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nLand Navigation Successful!";
        Helper.Speech(control.allChars[Random.Range(0, control.heroes.Count)].gameObject.transform, "I'm glad we didn't go down that path!", 3f);
    }
}
