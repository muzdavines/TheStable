using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public TutorialSet[] tutorials;
    void Start()
    {
        int thisIndex = Game.instance.tutorialStageFinished;
        if (thisIndex < tutorials.Length) {
            tutorials[thisIndex].StartTutorial();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            foreach (var t in tutorials) {
                t.gameObject.SetActive(false);
            }
        }
    }
}
