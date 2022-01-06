using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MoreMountains.Feedbacks;

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
    public MMCameraShaker goal;
    public void Start() {
        //match = Game.instance.activeMatch;
        ball.transform.position = new Vector3(0,1,0);
        if (debug) {
            DebugInit();
        } else { Init(); }
    }

    public void DebugInit() {
        Game game = Game.instance;
        Stable player = game.playerStable = new Stable();

        for (int i = 0; i < 8; i++) {
            Character thisHero = new Character();
            thisHero.name = "Player " + i.ToString();
            thisHero.tackling = Random.Range(8, 18);
            thisHero.dodging = Random.Range(8, 18);
            thisHero.blocking = Random.Range(8, 18);
            thisHero.modelName = "SCRogue";
            player.heroes.Add(thisHero);
        }
        Game.instance.Init();
        Game.instance.activeMatch = Game.instance.leagues[0].schedule[0];
        Init();

    }


    public void Init() {
        match = Game.instance.activeMatch;
        SpawnPlayers();
        awayCoach.Init();
        homeCoach.Init();
        UpdateScoreboard();
        StartCoroutine(DelayStart());
    }
    public void SpawnPlayers() {
        Game thisGame = Game.instance;
        List<Character>[] bothTeams = new List<Character>[2];
        bothTeams[0] = thisGame.activeMatch.home.stable.heroes;
        bothTeams[1] = thisGame.activeMatch.away.stable.heroes;
        for (int thisTeam = 0; thisTeam<2; thisTeam++) {
            for (int i = 0; i < 5; i++) {
                Character thisBaseChar = bothTeams[thisTeam][i];
                GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), homeSpawns[0].position, Quaternion.identity);
                StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
                thisChar.team = thisTeam;
                thisChar.blocking = thisBaseChar.blocking;
                thisChar.dodging = thisBaseChar.dodging;
                thisChar.tackling = thisBaseChar.tackling;
                thisChar.runSpeed = thisBaseChar.runSpeed;
                thisChar.fieldPosition = (Position)i;
            }
        }
    }
    IEnumerator DelayStart() {
        yield return new WaitForSeconds(2.0f);
        Kickoff();
    }
    public void Kickoff() {
        print("Kickoff");
        for (int i = 0; i<homeCoach.players.Length; i++) {
            homeCoach.players[i].transform.position = homeSpawns[i].position;
            awayCoach.players[i].transform.position = awaySpawns[i].position;
            homeCoach.players[i].Idle();
            awayCoach.players[i].Idle();
        }
        if (lastTeamToScore == -1) {
            lastTeamToScore = Random.Range(0, 2);
        }
        foreach (var g in FindObjectsOfType<Goal>()) {
            g.canScore = true;
        }
        ball.StopBall();
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
        goal.ShakeCamera(2, .5f, 5, 1, 1, 1, false);
        
        UpdateScoreboard();
        foreach (StableCombatChar awayPlayer in awayCoach.players) {
            awayPlayer.GoalScored();
        }
        foreach (StableCombatChar homePlayer in homeCoach.players) {
            homePlayer.GoalScored();
        }
        if (homeScore >= 3 || awayScore >= 3) {
            gameOver = true;
            StartCoroutine(DelayGameOver());
            
        } else { StartCoroutine(DelayStart()); }
    }
    void UpdateScoreboard() {
        scoreboard.text = match.home.stable.stableName+": " + homeScore + "  "+ match.away.stable.stableName + ": " + awayScore;
    }
    bool gameOver;
    private void Update() {
        if (gameOver) { return; }
        
        
        
    }
    IEnumerator DelayGameOver() {
        yield return new WaitForSeconds(3.0f);
        FinalizeResult(homeScore, awayScore);
        DisplayResults();
    }

    //Call this with results
    public void FinalizeResult(int homeGoals, int awayGoals) {
        Game.instance.activeMatch.awayGoals = awayGoals;
        Game.instance.activeMatch.homeGoals = homeGoals;
        Game.instance.activeMatch.final = true;
        Game.instance.activeMatch.ProcessResult();
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
