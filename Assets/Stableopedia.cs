using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stableopedia : MonoBehaviour {
    public CanvasGroup[] panels;
    private int index;
    void Start()
    {
        PreviousSlide();
    }

    public void NextSlide() {
        index++;
        DisplaySlide();
    }

    public void PreviousSlide() {
        index--;
        DisplaySlide();
    }

    void DisplaySlide() {
        index = Mathf.Clamp(index,0,panels.Length-1);
        foreach (var p in panels) {
            p.alpha = 0;
        }

        panels[index].alpha = 1;
    }

    void Update()
    {
        
    }
}
