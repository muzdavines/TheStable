using System.Collections;
using System.Collections.Generic;
using InfinityPBR;
using UnityEngine;
using UnityEngine.UI;

public class BatsDemo_LP : MonoBehaviour
{
    
    public BlendShapesPresetManager blendShapesPresetManager;
    public GameObject canvas;
    private Animator animator;
    public Toggle[] toggleWardrobe;

    public ColorShifterObject colorShifterObject;
    
    public float deathAnimationTime = 2.0f;									// How long the Death 1, 2, and 3 animations take (before the ground bounce) aka 60 frames = 2 seconds
    public Vector3 deathStartPosition;									// Start position of death motion
    public Vector3 deathEndPosition;										// End position of death motion
    private float t = 0.0f;													// Time counter for motion
    private bool isDying = false;											// Are we currently dying?
    public float deathGroundHeight = 5.1f; // Height of the ground under the death animation start position
    
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
            ToggleCanvas();
        }
        
        if (isDying) {																								// If we are dying
            t += Time.deltaTime / deathAnimationTime;																// add computed deltaTime to the counter t
            if (t < 1.0) {																							// If we havne't reached the end yet
                transform.position = Vector3.Lerp (deathStartPosition, deathEndPosition, t);	// Move closer to the end position over time
                
            } else {																								// If we are at the end
                isDying = false;																					// Set isDying = false
            }
        } else if (t >= 1.0) {																						// If we just ended the death (Do this a frame AFTER the end)
            transform.position = deathEndPosition;													// Set to the end position
        }
    }
    
    public void startDeath(){
															// For each bat
			deathStartPosition = transform.position;											// Set start position
			deathEndPosition = new Vector3(deathStartPosition.x, deathGroundHeight, deathStartPosition.z);	// Compute end position (for the demo, just the gound at y = .05 [to keep the bat fully above ground])
			isDying = true;																				// Set isDying
			t = 0;																						// Set the time counter t to 0
		
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
    
}
