using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LeagueController : MonoBehaviour, UIElement
{
    public Text matchInfo, leagueTable;
    public GameObject playMatchButton;
    public League.Match activeMatch;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMatch() {
        Game.instance.activeMatch = activeMatch;
        SceneManager.LoadScene("MissionCombatTester");
    }
    public void UpdateOnAdvance() {
        UpdateLeague();
    }
    
    public void OnEnable() {
        UpdateLeague();
    }
    public void UpdateLeague() {
        CheckIfMatchDay();
        SimOtherGames();
        UpdateLeagueTable();
        
    }
    public void UpdateLeagueTable() {
        leagueTable.text = "                 League Table\n";
        foreach (string s in Game.instance.leagues[0].GetTable()) {
            leagueTable.text += s + "\n";
        }
    }
    public void SimOtherGames() {
        foreach (League.Match match in Game.instance.leagues[0].schedule) {
            if (match.final || match.date.IsOnOrAfter(Helper.Today())) {
                continue;
            }
            var homeGoals = Random.Range(0, 8);
            var awayGoals = Random.Range(0, 8);
            match.awayGoals = awayGoals;
            match.homeGoals = homeGoals;
            match.final = true;
            match.ProcessResult();
        }
    }
    public void CheckIfMatchDay() {
        
        foreach (League.Match match in Game.instance.leagues[0].schedule) {
            Debug.Log(match.IsPlayerMatch() + " " + match.away.stable.stableName + "  " + match.home.stable.stableName);
            if (!match.IsPlayerMatch()) {
                continue;
            }
            if (match.date.IsToday()) {
                if (match.final) {
                    matchInfo.text = match.home.stable.stableName + ": " + match.homeGoals + "    " + match.away.stable.stableName + ": " + match.awayGoals;
                    playMatchButton.SetActive(false);
                }
                else {
                    matchInfo.text = match.home.stable.stableName + " v. " + match.away.stable.stableName;
                    playMatchButton.SetActive(true);
                    activeMatch = match;
                }
                activeMatch = match;
                break;
            }
            else {
                League.Match nextMatch = new League.Match();
                foreach (League.Match nMatch in Game.instance.leagues[0].schedule) {
                    if (nMatch.final || !nMatch.IsPlayerMatch()) { continue; }
                    nextMatch = nMatch;
                    break;
                }
                activeMatch = nextMatch;
                matchInfo.text = "Next Match in " + nextMatch.date.DaysBetween(Helper.Today()) + " days. \n" + nextMatch.home.stable.stableName + " v. " + nextMatch.away.stable.stableName;
                playMatchButton.SetActive(false);
            }
        }
    }
}
