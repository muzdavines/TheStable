using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfinityPBR;

public class SkeletonDemoLP : MonoBehaviour {

    public GameObject[] bodyParts;
    public Animator animator;
    public GameObject[] swords;
    public GameObject[] armors;
    public GameObject[] shields;

    public Material[] mats;
    public PrefabAndObjectManager wardrobeManager;

    void Start()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomMats();
        }
    }

    public void RandomMats()
    {
        //wardrobeManager.RandomWardrobe();
        SetBodyMaterial(mats[Random.Range(0, mats.Length)]);
        SetSwordMaterial(mats[Random.Range(0, mats.Length)]);
        SetShieldMaterial(mats[Random.Range(0, mats.Length)]);
        SetArmorMaterial(mats[Random.Range(0, mats.Length)]);
    }
    
    public void SetHue(float value)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach (Transform child in bodyParts[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Hue", value);
            }
        }

        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Hue", value);
        }
        for (int i = 0; i < armors.Length; i++)
        {
            foreach (Transform child in armors[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Hue", value);
            }
        }
        for (int i = 0; i < shields.Length; i++)
        {
            foreach (Transform child in shields[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Hue", value);
            }
        }
    }
    
    public void SetSaturation(float value)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach (Transform child in bodyParts[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Saturation", value);
            }
        }

        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Saturation", value);
        }
        for (int i = 0; i < armors.Length; i++)
        {
            foreach (Transform child in armors[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Saturation", value);
            }
        }
        for (int i = 0; i < shields.Length; i++)
        {
            foreach (Transform child in shields[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Saturation", value);
            }
        }
    }
    
    public void SetValue(float value)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach (Transform child in bodyParts[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Value", value);
            }
        }

        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Value", value);
        }
        for (int i = 0; i < armors.Length; i++)
        {
            foreach (Transform child in armors[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Value", value);
            }
        }
        for (int i = 0; i < shields.Length; i++)
        {
            foreach (Transform child in shields[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Value", value);
            }
        }
    }

    public void SetBodyMaterial(Material mat)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach (Transform child in bodyParts[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
            }
        }
        
    }
    
    public void SetSwordMaterial(Material mat)
    {
        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].GetComponent<SkinnedMeshRenderer>().material = mat;
        }
        
    }
    
    public void SetArmorMaterial(Material mat)
    {
        for (int i = 0; i < armors.Length; i++)
        {
            foreach (Transform child in armors[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
            }
        }
        
    }
    
    public void SetShieldMaterial(Material mat)
    {
        for (int i = 0; i < shields.Length; i++)
        {
            foreach (Transform child in shields[i].transform)
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    child.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
            }
        }
        
    }
    
    

    public void UpdateLocomotion(float value)
    {
        animator.SetFloat("Locomotion", value);
    }
}
