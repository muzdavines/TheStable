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
        int topScorer = 0;
        Character topScorerChar = null;
        string topScorerStable = "";
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.seasonStats.goals > topScorer) {
                topScorer = c.seasonStats.goals;
                topScorerChar = c;
                topScorerStable = Game.instance.playerStable.stableName;
            }
        }
        foreach (Stable s in Game.instance.otherStables) {
            foreach (Character c in s.heroes) {
                if (c.seasonStats.goals > topScorer) {
                    topScorer = c.seasonStats.goals;
                    topScorerChar = c;
                    topScorerStable = s.stableName;
                }
            }
        }
        if (topScorerChar != null) {
            leagueTable.text += "Top Scorer: " + topScorerChar.name + "  Goals: " + topScorer + ", " + topScorerStable;
        }
    }
    public void SimOtherGames() {
        foreach (League.Match match in Game.instance.leagues[0].schedule) {
            if (match.final || match.date.IsOnOrAfter(Helper.Today())) {
                continue;
            }
            SimMatch(match);
        }
    }

    public void SimMatch(League.Match thisMatch) {
        bool homeAggressor = true;
        int homeGoals = 0;
        int awayGoals = 0;
        for (int i = 0; i<10000; i++) {
            Character homeChar = thisMatch.home.stable.heroes[Random.Range(0, thisMatch.home.stable.heroes.Count)];
            Character awayChar = thisMatch.away.stable.heroes[Random.Range(0, thisMatch.away.stable.heroes.Count)];
            float homeRoll = 0;
            float awayRoll = 0;
            if (homeAggressor) {
                homeRoll = Random.Range(1f, homeChar.carrying + homeChar.runspeed);
                awayRoll = Random.Range(1f, awayChar.tackling + awayChar.runspeed);
                float thisRoll = homeRoll / awayRoll;
                Debug.Log("#MatchResult#Home Roll:" + thisRoll);
                if (thisRoll > 1.2f) {
                    homeGoals++;
                    homeChar.seasonStats.goals++;
                }
            }
            else {
                awayRoll = Random.Range(1f, awayChar.carrying + awayChar.runspeed);
                homeRoll = Random.Range(1f, homeChar.tackling + homeChar.runspeed);
                float thisRoll = awayRoll / homeRoll;
                Debug.Log("#MatchResult#Home Roll:" + thisRoll);
                if (thisRoll > 1.2f) {
                    awayGoals++;
                    awayChar.seasonStats.goals++;
                }
            }
            homeAggressor = !homeAggressor;
            if (awayGoals >= 3 || homeGoals >= 3) {
                break;
            }
        }
        if (awayGoals < 3 && homeGoals < 3) {
            homeGoals = 3;
        }
        thisMatch.awayGoals = awayGoals;
        thisMatch.homeGoals = homeGoals;
        thisMatch.final = true;
        thisMatch.ProcessResult();
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
