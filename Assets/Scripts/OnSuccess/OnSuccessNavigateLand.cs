using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessNavigateLand : OnStepSuccess
{
    public override void OnSuccess() {
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\nLand Navigation Successful!";
        Helper.Speech(control.heroes[Random.Range(0, control.heroes.Count)].currentMissionCharacter.gameObject.transform, "I'm glad we didn't go down that path!", 3f);
    }
}
