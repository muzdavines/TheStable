using System.Collections;
using System.Collections.Generic;
using FIMSpace.Generating;
using UnityEngine;

public class DungeonTestInst : MonoBehaviour {
    public GameObject go;
    public GameObject go2;
    public BuildPlannerExecutor plan;

    void Start() {
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart() {
        yield return new WaitForSeconds(1.0f);
        plan.Generate();
    }
}
