using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MummyDemoLP : MonoBehaviour
{

    public GameObject mummy;
    public Vector3 posStanding;
    public Vector3 posLaying;

    public GameObject wraps;

    public SkinnedMeshRenderer[] mummyR;
    public SkinnedMeshRenderer[] sarcophagusR;
   
    public Material[] materials;

    public SkinnedMeshRenderer[] renderer;
    
    
    public void ResetStanding(){
        mummy.transform.position = posStanding;
    }
    
    public void ResetLaying(){
        mummy.transform.position = posLaying;
    }

    public void Locomotion(float v)
    {
        mummy.GetComponent<Animator>().SetFloat("locomotion", v);
    }

    public void SuperRandom()
    {
        wraps.SetActive(Random.Range(0, 2) == 1 ? true : false);

        int matIndex = Random.Range(0, materials.Length);
        int matIndex2 = Random.Range(0, materials.Length);

        for (int i = 0; i < mummyR.Length; i++)
        {
            mummyR[i].sharedMaterial = materials[matIndex];
        }
        for (int i = 0; i < sarcophagusR.Length; i++)
        {
            sarcophagusR[i].sharedMaterial = materials[matIndex2];
        }
    }
    
    public void SetHue(float value)
    {
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].material.SetFloat("_Hue", value);
        }
    }
    
    public void SetSaturation(float value)
    {
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].material.SetFloat("_Saturation", value);
        }
    }
    
    public void SetValue(float value)
    {
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].material.SetFloat("_Value", value);
        }
    }
}
