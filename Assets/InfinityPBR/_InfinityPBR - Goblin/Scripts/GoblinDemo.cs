using System.Collections;
using System.Collections.Generic;
using InfinityPBR;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GoblinDemo : MonoBehaviour
{

    private Animator animator;
    public Button[] textureButtons;
    public Toggle[] wardrobeToggles;

    private BlendShapesPresetManager bsm;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        bsm = GetComponent<BlendShapesPresetManager>();
    }

    public void SetLocomotion(float value)
    {
        animator.SetFloat("Locomotion", value);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SuperRandomize();
            bsm.ActivatePreset(Random.Range(0, bsm.presets.Count));
        }
    }

    public void SuperRandomize()
    {
        GetComponent<ColorShiftRuntime>().SetColorSet(Random.Range(0, 4)); // Armor
        GetComponent<ColorShiftRuntime>().SetColorSet(Random.Range(4, 8)); // Body
        
       
        for (int i = 0; i < wardrobeToggles.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                wardrobeToggles[i].isOn = true;
            }
            else
            {
                wardrobeToggles[i].isOn = false;
            }
        }
    }
}
