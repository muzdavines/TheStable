using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCModelSelector : MonoBehaviour
{
    public GameObject[] models;
    public Material[] materials;
    public void Init(int modelNum, int team) {
        models[modelNum].SetActive(true);
        models[modelNum].GetComponent<SkinnedMeshRenderer>().material = materials[team];
    }
}
