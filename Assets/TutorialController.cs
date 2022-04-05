using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public TutorialSet[] tutorials;
    void Start()
    {
        int thisIndex = Game.instance.tutorialStageFinished;
        tutorials[thisIndex].StartTutorial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
