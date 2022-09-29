using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSpeed : MonoBehaviour
{
   
    public float timeScale = 1.0f;    
    public Slider slider;
    private void Start() {
        slider.value = timeScale;
        Time.timeScale = timeScale;
    }
    void Update() {
        Time.timeScale = timeScale;
    }

    public void UpdateSpeed() {
        timeScale = slider.value;
    }
}
