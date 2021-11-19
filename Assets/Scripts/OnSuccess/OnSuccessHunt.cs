using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessHunt : OnStepSuccess
{
    public override void OnSuccess() {
        MissionHelperHunt helper = GetComponent<MissionHelperHunt>();
        MissionController control = FindObjectOfType<MissionController>();
        control.update.text += "\n" + GetComponent<MissionHelperHunt>().animalToLoad+ " Killed!";
        Helper.Speech(control.heroes[Random.Range(0, control.heroes.Count)].currentMissionCharacter.gameObject.transform, "We eat well tonight!", 3f);
        
    }
}
