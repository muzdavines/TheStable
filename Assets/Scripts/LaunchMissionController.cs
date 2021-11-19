using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LaunchMissionController : MonoBehaviour {
    public GameObject panel;
    public MissionContract thisContract;
    public Text contractInfo;
    public MissionPreferencesScrollerController prefs;
    public void Start() {
        panel.SetActive(false);
    }
    public void OpenMissionPanel(MissionContract _thisContract) {
        panel.SetActive(true);
        thisContract = _thisContract;
        contractInfo.text = "Contract: " + thisContract.contractType.ToString() + "\nMax Heroes: " + thisContract.maxHeroes;
        prefs.Init(thisContract);
    }

    public void Launch() {
        Game.instance.playerStable.activeContract = thisContract;
        SceneManager.LoadScene("Mission");
    }

    public void Cancel() {
        Game.instance.playerStable.SetAllHeroesInactive();
        foreach (Stage s in thisContract.stages) {
            s.preferredMethod = 0;
        }
        panel.SetActive(false);
    }
}
