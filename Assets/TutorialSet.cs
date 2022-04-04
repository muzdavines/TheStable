using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSet : MonoBehaviour
{
    float nextStageTime;
    int stage;
    public TutorialStage[] stages;
    public void StartTutorial() {
        gameObject.SetActive(true);
        stage = 0;
        Display();
    }

    public void Update() {
       
    }
    public void NextStage() {
        stage++;
        Display();
    }
    public void Display() {
        if (stage >= stages.Length) {
            print(stage + " Stage");
            EndTutorial();
            return;
        }
        stages[stage].Begin(this);
    }
    void EndTutorial() {
        Game.instance.tutorialStageFinished++;
        foreach (var g in stages) {
            g.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
        
    }
    
}
