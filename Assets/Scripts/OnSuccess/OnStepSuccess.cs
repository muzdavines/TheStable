using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OnStepSuccess : MonoBehaviour
{
    public abstract void OnSuccess();
    public virtual void OnSuccess(int quality) { }
    public List<MissionPOI> affectedPOI;
    public float affecttedPOIMod;
}
