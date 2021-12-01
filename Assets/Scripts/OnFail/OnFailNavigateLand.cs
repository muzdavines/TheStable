using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailNavigateLand : OnStepFail {

    public MissionPOI campPOI;

    public override void OnFail() {
        Helper.UIUpdate("The group has gotten lost. They will set up camp late and suffer a penalty to its quality.");
        StartCoroutine(BroadcastNextStep());
        campPOI.step.mod += -.1f; //10% penalty
        Helper.Speech(FindObjectOfType<MissionController>().currentActiveStepChar.currentObject.transform, "Oh, no. This isn't the right way...");
        foreach (var p in affectedPOI) {
            p.step.mod += affecttedPOIMod;
        }
    }
    IEnumerator BroadcastNextStep() {
        yield return new WaitForSeconds(4.0f);
        FindObjectOfType<MissionController>().heroes[0].currentMissionCharacter.BroadcastNextStep();
    }

}
