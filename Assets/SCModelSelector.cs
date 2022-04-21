using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCModelSelector : MonoBehaviour
{
    public GameObject[] models;
    public Material[] materials;
    public int playerMatVariant = 0;
    
    public GameObject[] all;
    public GameObject[] male;
    public GameObject[] female;

    public void Init(int modelNum, int skin, bool playerStable = false) {
        models[modelNum].SetActive(true);
        if (skin == -1) { return; }
        if (skin == 99) { models[modelNum].GetComponent<SkinnedMeshRenderer>().material = materials[materials.Length]; return; }
        models[modelNum].GetComponent<SkinnedMeshRenderer>().material = materials[skin];
    }
}
