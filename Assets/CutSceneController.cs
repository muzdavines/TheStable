using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneController : MonoBehaviour
{
    public string sceneToLoad;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            LoadNextScene();
        }
    }

    public void LoadNextScene() {
        InitFirstMatch();
        SceneManager.LoadScene(sceneToLoad);
    }
    public void InitFirstMatch() {
        var thisMatch = new League.Match();
        Stable home = Instantiate(Resources.Load<StableSO>("StartingStable")).stable;
        Stable away = Instantiate(Resources.Load<StableSO>("Cutscene2Stable")).stable;
        thisMatch.home = new League.Team() { stable = home };
        thisMatch.away = new League.Team() { stable = away };
        Game.instance.activeMatch = thisMatch;
    }
}
