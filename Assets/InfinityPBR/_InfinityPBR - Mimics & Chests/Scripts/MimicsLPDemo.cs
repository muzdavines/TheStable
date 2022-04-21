using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfinityPBR;

public class MimicsLPDemo : MonoBehaviour
{
    public Animator animator;
    public ColorShiftRuntime colorShiftRuntime;
    public GameObject[] backHinges;
    public GameObject[] handles;
    public GameObject[] latches;
    public GameObject gems1;
    public GameObject gems2;
    public GameObject[] mimics;
    public GameObject[] teeth;
    public GameObject tongue;
    private int latch = 0;
    private int handle = 0;
    private int backHinge = 0;
    public BlendShapesManager bsm;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator> ();
        NextLatch(0);
        NextHinge(0);
        NextHandle(0);
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
        latch = Random.Range(0, latches.Length);
        handle = Random.Range(0, handles.Length);
        backHinge = Random.Range(0, backHinges.Length);
        NextLatch(0);
        NextHinge(0);
        NextHandle(0);

        bsm = GetComponent<BlendShapesManager>();
        RandomAll();

        if (Random.Range(0, 2) == 0)
        {
            gems1.SetActive(false);
            gems2.SetActive(false);
            for (int i = 0; i < mimics.Length; i++)
            {
                mimics[i].SetActive(true);
            }

            if (Random.Range(0, 2) == 0)
            {
                for (int i = 0; i < teeth.Length; i++)
                    teeth[i].SetActive(true);
            }
            else
            {
                for (int i = 0; i < teeth.Length; i++)
                    teeth[i].SetActive(false);
            }

            if (Random.Range(0, 2) == 0)
            {
                tongue.SetActive(false);
            }
            else
                tongue.SetActive(true);
        }
        else
        {
            for (int i = 0; i < mimics.Length; i++)
                mimics[i].SetActive(false);
            tongue.SetActive(false);
            for (int i = 0; i < teeth.Length; i++)
                teeth[i].SetActive(false);

            gems1.SetActive(false);
            gems2.SetActive(false);
            if (Random.Range(0, 2) == 0)
            {
                gems1.SetActive(true);
            }
            if (Random.Range(0, 2) == 0)
            {
                gems2.SetActive(true);
            }
        }

        colorShiftRuntime.SetRandomColorSet();
    }

    public void NextLatch(int value)
    {
        latch += value;
        if (latch < 0)
            latch = latches.Length - 1;
        if (latch >= latches.Length)
            latch = 0;
        for (int i = 0; i < latches.Length; i++)
        {
            latches[i].SetActive(false);
        }
        
        latches[latch].SetActive(true);
    }
    
    public void NextHinge(int value)
    {
        backHinge += value;
        if (backHinge < 0)
            backHinge = backHinges.Length - 1;
        if (backHinge >= backHinges.Length)
            backHinge = 0;
        for (int i = 0; i < backHinges.Length; i++)
        {
            backHinges[i].SetActive(false);
        }

        backHinges[backHinge].SetActive(true);
    }
    
    public void NextHandle(int value)
    {
        handle += value;
        if (handle < 0)
            handle = handles.Length - 1;
        if (handle >= handles.Length)
            handle = 0;
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].SetActive(false);
        }

        handles[handle].SetActive(true);
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
