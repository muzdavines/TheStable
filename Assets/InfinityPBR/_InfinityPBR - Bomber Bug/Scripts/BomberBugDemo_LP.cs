using System.Collections;
using System.Collections.Generic;
using InfinityPBR;
using UnityEngine;
using UnityEngine.UI;

public class BomberBugDemo_LP : MonoBehaviour
{
    
    public BlendShapesPresetManager blendShapesPresetManager;
    public GameObject canvas;
    private Animator animator;
    public Toggle[] toggleWardrobe;
    public float desiredWeight = 0.0f;
    public bool checkTransitionDone = false;

    public ColorShifterObject colorShifterObject;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Randomize();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
           // ToggleCanvas();
        }
        
        if (animator.GetLayerWeight(1) != desiredWeight){
            animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), desiredWeight, Time.deltaTime * 3));
        }

        if (animator.IsInTransition (1)) {
            checkTransitionDone = true;
        }
        if (checkTransitionDone && !animator.IsInTransition (1)) {
            checkTransitionDone = false;
            if (animator.GetCurrentAnimatorStateInfo (1).IsName ("fly idle")) {
                desiredWeight = 0;
            }
        }
    }

    public void ChangeColor(string newColorSet)
    {
        colorShifterObject.SetColorSet(newColorSet);
    }

    public void RandomColorSet()
    {
        colorShifterObject.SetRandomColorSet();
    }
    
    public void Locomotion(float newValue){
        animator.SetFloat ("locomotion", newValue);
    }

    public void ToggleCanvas()
    {
        canvas.SetActive(!canvas.active);
    }

    public void Default()
    {
        blendShapesPresetManager.ActivatePreset("Default");
    }
    
    public void Randomize()
    {
        blendShapesPresetManager.ActivatePreset("Random");
        RandomColorSet();

        for (int i = 0; i < toggleWardrobe.Length; i++)
        {
            bool value = Random.Range(0, 2) == 1;
            toggleWardrobe[i].isOn = value;
            toggleWardrobe[i].onValueChanged.Invoke(value);
        }
    }

    public void SetBodyLayerWeight(float weight)
    {
        desiredWeight = weight;
    }
}
