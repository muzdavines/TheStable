using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OnStepFail : MonoBehaviour {
    public abstract void OnFail();
    public List<MissionPOI> affectedPOI;
    public float affecttedPOIMod;

}
