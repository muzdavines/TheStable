using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfinityPBR;

public class MushroomMonsterLP_Demo : MonoBehaviour
{
    public Animator animator;
    public ColorShiftRuntime colorShiftRuntime;
    public BlendShapesManager bsm;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator> ();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeAll();
        }
    }
	
    public void Locomotion(float newValue){
        animator.SetFloat ("locomotion", newValue);
    }

    public void RandomizeAll()
    {
        bsm = GetComponent<BlendShapesManager>();

        colorShiftRuntime.SetRandomColorSet();
        RandomAll();
    }
    
    public void RandomAll()
    {
        foreach (var bs in bsm.blendShapeGameObjects)
        {
            foreach (var bsv in bs.blendShapeValues)
            {
                bsm.SetRandomShapeValue(bsv);
            }
        }
    }

}
