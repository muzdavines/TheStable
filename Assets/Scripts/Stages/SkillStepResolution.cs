using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillStepResolution : MonoBehaviour
{
    List<MissionPOI> affectedPOI;
    float affectedPOIMod;
    private void Start() {
        if (affectedPOI == null) {
            affectedPOI = new List<MissionPOI>();
        }
    }
    public void OnSuccess(string message) {
        Debug.Log("#Step#Success!");
        if (GetComponent<MissionPOI>().isFinalForMission) { FindObjectOfType<MissionFinalDetails>().successful = true; }
        FindObjectOfType<MissionController>().update.text += "\n" + message;
        foreach (var p in affectedPOI) {
            p.step.mod += affectedPOIMod;
        }
    }
    public void OnFailed(string message) {
        Debug.Log("#Step#Fail.");
        if (GetComponent<MissionPOI>().isFinalForMission) { FindObjectOfType<MissionFinalDetails>().successful = false; }
        FindObjectOfType<MissionController>().update.text += "\n" + message;
        foreach (var p in affectedPOI) {
            p.step.mod -= affectedPOIMod;
        }
    }
    public void OnAvoid(bool minReqNotMet) {
        if (minReqNotMet) {
            print("Min Req Not Met");
        }
        else {
            print("Avoided something bad");
        }
    }
}
