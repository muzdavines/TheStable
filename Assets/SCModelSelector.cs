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

    public void Init(int modelNum, int team, bool playerStable = false) {
        models[modelNum].SetActive(true);
        if (playerStable) {
            models[modelNum].GetComponent<SkinnedMeshRenderer>().material = Resources.Load<Material>("PlayerStableMat"+playerMatVariant);
        }
        else {
            models[modelNum].GetComponent<SkinnedMeshRenderer>().material = materials[team];
        }
    }
}
