using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MatchController : MonoBehaviour
{
    public League.Match match;
    public TextMeshProUGUI matchResults;
    public TextMeshProUGUI scoreboard;
    public int homeScore, awayScore;
    public Transform[] homeSpawns;
    public Transform[] awaySpawns;
    public Transform[] ballSpawns;
    public Coach homeCoach;
    public Coach awayCoach;
    public Ball ball;
    int lastTeamToScore = -1;
    public bool debug;
    public GameObject debugPlayers;
    public void Start() {
        //match = Game.instance.activeMatch;
        ball.transform.position = Vector3.zero;
        
    }

    public void Init() {
        if (debug) {
            debugPlayers.SetActive(true);
            StartCoroutine(DelayStart());
        } else {
            SpawnPlayers();
        }
        awayCoach.Init();
        homeCoach.Init();
        StartCoroutine(DelayStart());
    }
    public void SpawnPlayers() {
        Game thisGame = Game.instance;
       
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
    bool gameOver;
    private void Update() {
        if (gameOver) { return; }
        if (homeScore >= 7 || awayScore >= 7) {
            gameOver = true;
            FinalizeResult(homeScore, awayScore);
            DisplayResults();
        }
    }

    //Call this with results
    public void FinalizeResult(int homeGoals, int awayGoals) {
        match.awayGoals = awayGoals;
        match.homeGoals = homeGoals;
        match.final = true;
        match.ProcessResult();
        Destroy(ball.gameObject);
        var players = FindObjectsOfType<StableCombatChar>();
        for (int i = 0; i < players.Length; i++) {
            Destroy(players[i].gameObject);
        }
    }

    public void DisplayResults() {
        matchResults.text = match.home.stable.stableName + ": " + match.homeGoals + "    " + match.away.stable.stableName + ": " + match.awayGoals;
        matchResults.transform.parent.gameObject.SetActive(true);
    }

    public void ExitMatch() {
        SceneManager.LoadScene("StableManagement");
    }
}
