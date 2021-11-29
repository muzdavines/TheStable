using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MatchController : MonoBehaviour
{
    public League.Match match;
    public Text matchResults;

    public void Start() {
        match = Game.instance.activeMatch;
    }
    public void SimMatch() {
        if (match.final) { print("Already Simmed!"); return; }
        var homeGoals = Random.Range(0, 8);
        var awayGoals = Random.Range(0, 8);
        FinalizeResult(homeGoals, awayGoals);
        DisplayResults();
    }

    //Call this with results
    public void FinalizeResult(int homeGoals, int awayGoals) {
        match.awayGoals = awayGoals;
        match.homeGoals = homeGoals;
        match.final = true;
        match.ProcessResult();
    }

    public void DisplayResults() {
        matchResults.text = match.home.stable.stableName + ": " + match.homeGoals + "    " + match.away.stable.stableName + ": " + match.awayGoals;
    }

    public void ExitMatch() {
        SceneManager.LoadScene("StableManagement");
    }
}
