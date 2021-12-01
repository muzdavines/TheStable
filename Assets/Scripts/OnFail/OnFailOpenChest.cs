using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailOpenChest : OnStepFail
{
    public string notification = "Chest Opened!";
    public override void OnFail() {
        FindObjectOfType<MissionController>().update.text += "\n" + notification;
        foreach (var p in affectedPOI) {
            p.step.mod += affecttedPOIMod;
        }
    }
}
