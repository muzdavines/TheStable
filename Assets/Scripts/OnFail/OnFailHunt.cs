using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailHunt : OnStepFail
{
    public override void OnFail() {
        MissionHelperHunt helper = GetComponent<MissionHelperHunt>();
        FindObjectOfType<MissionController>().update.text += "\n" + helper.GetComponent<MissionPOI>().currentCharacterToAttempt + " failed to kill the " + helper.animalToLoad + " Killed!";

    }
}

