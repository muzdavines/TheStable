using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MatchController : MonoBehaviour
{
    public League.Match match;
    public Text matchResults;
    public TextMeshProUGUI scoreboard;
    public int homeScore, awayScore;
    public Transform[] homeSpawns;
    public Transform[] awaySpawns;
    public Transform[] ballSpawns;
    public Coach homeCoach;
    public Coach awayCoach;
    public Ball ball;
    int lastTeamToScore = -1;
    public void Start() {
        //match = Game.instance.activeMatch;
        ball.transform.position = Vector3.zero;
        StartCoroutine(DelayStart());
    }
    IEnumerator DelayStart() {
        yield return new WaitForSeconds(1.0f);
        Kickoff();
    }
    public void Kickoff() {
        for (int i = 0; i<homeCoach.players.Length; i++) {
            homeCoach.players[i].transform.position = homeSpawns[i].position;
            awayCoach.players[i].transform.position = awaySpawns[i].position;
            homeCoach.players[i].Idle();
            awayCoach.players[i].Idle();
        }
        if (lastTeamToScore == -1) {
            lastTeamToScore = Random.Range(0, 2);
        }
        ball.body.velocity = Vector3.zero;
        ball.transform.position = lastTeamToScore == 0 ? ballSpawns[1].position : ballSpawns[0].position;
        ball.body.AddForce(new Vector3(Random.Range(-2, 2), 0, 0));
    }
    public void SimMatch() {
        if (match.final) { print("Already Simmed!"); return; }
        var homeGoals = Random.Range(0, 8);
        var awayGoals = Random.Range(0, 8);
        FinalizeResult(homeGoals, awayGoals);
        DisplayResults();
    }

    public void ScoreGoal(int team) {
        if (team == 0) { homeScore++; } else { awayScore++; }
        lastTeamToScore = team;
        UpdateScoreboard();
        Kickoff();
    }
    void UpdateScoreboard() {
        scoreboard.text = "Red: " + homeScore + "  Green: " + awayScore;
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
