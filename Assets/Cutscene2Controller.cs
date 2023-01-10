using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Cutscene2Controller : MonoBehaviour
{
    public PlayableDirector director;
   public void PauseGame() {
        Time.timeScale = 0;
    }
    public void UnpauseGame() {
        Time.timeScale = 1;
    }
    public void StopDirector() {
        director.Pause();
    }
    public void StartDirector() {
        director.Resume();
    }
    public void Update() {
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
            StartDirector();
        }
    }
}
