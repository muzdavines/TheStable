using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MoreMountains.Feedbacks;
using UnityEngine.Playables;
using PsychoticLab;
using Unity.Mathematics;
using Random = UnityEngine.Random;

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
    public PlayableDirector introDirector;
    public PlayableDirector outroDirector;
    public HeroUIController heroUIController;
    public Transform oneTimerCam;
    public GameObject continuePanel;
    public TextMeshProUGUI announcer;
    public List<AnnouncerLine> announcerLines;
    public Button kickoffButton;
    public class AnnouncerLine {
        public string line;
        public float endTime;
    }

    public void Start() {
        //match = Game.instance.activeMatch;
        ball.transform.position = new Vector3(0,1,0);
        announcerLines = new List<AnnouncerLine>();
        AddAnnouncerLine("Welcome to the Match!");
        if (continuePanel != null) {
            continuePanel.SetActive(false);
        }

        if (debug) {
            DebugInit();
        } else { Init(); }
    }

    public void DebugInit() {
        Game game = Game.instance;
        Stable player = game.playerStable = Instantiate(Resources.Load<StableSO>("StartingStable")).stable;
        Move leftJab = Resources.Load<Move>("LeftJab");
        Move rightJab = Resources.Load<Move>("RightJab");
        Game.instance.Init();
        Game.instance.activeMatch = Game.instance.leagues[0].schedule[0];
        Init();
    }


    public void Init() {
        Physics.gravity = new Vector3(0, -9.81f, 0);
        match = Game.instance.activeMatch;
        if (oneTimerCam != null) {
            oneTimerCam.gameObject.SetActive(false);
        }

        SpawnPlayers();
        awayCoach.Init();
        homeCoach.Init();
        if (match.home.stable == Game.instance.playerStable) { homeCoach.isPlayer = true; } else { awayCoach.isPlayer = true; }
        UpdateScoreboard();
        UpdatePlayerUI();
        // StartCoroutine(DelayStart());
        introDirector.Play();
    }

    public void UpdatePlayerUI() {
        heroUIController = FindObjectOfType<HeroUIController>();
        if (heroUIController == null) {
            return;

        }
        heroUIController.Init(homeCoach.isPlayer ? homeCoach : awayCoach);
    }
    public void SpawnPlayers() {
        Game thisGame = Game.instance;
        List<Character>[] bothTeams = new List<Character>[2];
        bothTeams[0] = thisGame.activeMatch.home.stable.heroes;
        bothTeams[1] = thisGame.activeMatch.away.stable.heroes;
        int playersStable = thisGame.activeMatch.home.stable == Game.instance.playerStable ? 0 : 1;
        for (int thisTeam = 0; thisTeam<2; thisTeam++) {
            Color primaryColor = thisTeam == 0 ? thisGame.activeMatch.home.stable.primaryColor : thisGame.activeMatch.away.stable.primaryColor;
            Color secondaryColor = thisTeam == 0 ? thisGame.activeMatch.home.stable.secondaryColor : thisGame.activeMatch.away.stable.secondaryColor;
            for (int i = 0; i < bothTeams[thisTeam].Count; i++) {
                Character thisBaseChar = bothTeams[thisTeam][i];
                if (playersStable == thisTeam && Game.instance.playerStable.finance.gold < 0) {
                    if (thisBaseChar.archetype != Character.Archetype.Amateur && thisBaseChar.archetype != Character.Archetype.Goalkeeper) {
                        continue;
                    }
                }
                else {
                    if (!thisBaseChar.activeInLineup) { continue; }
                }
                GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), homeSpawns[0].position, Quaternion.identity);
                StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
                thisChar.team = thisTeam;
                thisChar.fieldSport = true;
                thisChar.myCharacter = thisBaseChar;
                thisChar.fieldPosition = thisBaseChar.currentPosition > 0 ? thisBaseChar.currentPosition : (Position)(i + 2);
                thisChar.GetComponent<CharacterRandomizer>()?.Init(thisBaseChar, primaryColor, secondaryColor);
                thisChar.Init();
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
            var homePlayer = homeCoach.players[i];
            var awayPlayer = awayCoach.players[i];
            homePlayer.transform.position = homeSpawns[(int)homePlayer.fieldPosition].position;
            awayPlayer.transform.position = awaySpawns[(int)awayPlayer.fieldPosition].position;
            homePlayer.Reset();
            awayPlayer.Reset();
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

    public void ContinueMatch() {
        SpawnPlayers();
        awayCoach.Init();
        homeCoach.Init();
        if (match.home.stable == Game.instance.playerStable) { homeCoach.isPlayer = true; } else { awayCoach.isPlayer = true; }
        UpdateScoreboard();
        UpdatePlayerUI();
        Kickoff();
    }

    
    public void ZoomCam(StableCombatChar thisChar) {
        if (oneTimerCam == null) {
            return;
        }
        Transform cam = oneTimerCam;
        cam.position = thisChar.position + (thisChar._t.forward * -8) + new Vector3(-2.5f, 4f, 0);
        cam.LookAt(thisChar._t);
        cam.gameObject.SetActive(true);
        StartCoroutine(ZoomCamOffTimer());
    }

    IEnumerator ZoomCamOffTimer() {
        yield return new WaitForSeconds(3.0f);
        ZoomCamOff();
    }

    public void ZoomCamOff() {
        if (oneTimerCam == null) {
            return;
        }
        oneTimerCam.gameObject.SetActive(false);
    }


    public void ScoreGoal(int team) {
        FindObjectOfType<FieldSportSoundManager>()?.Goal();
        if (team == 0) { homeScore++; } else { awayScore++; }
        lastTeamToScore = team;
        goal.ShakeCamera(2, .5f, 5, 1, 1, 1, false);
        if (team == ball.lastHolder.team) {
            announcerLines = new List<AnnouncerLine>();
            ball.lastHolder.myCharacter.seasonStats.goals++;
            ball.lastHolder.myCharacter.xp += Game.XPGoal;
            if (ball.lastlastHolder?.team == lastTeamToScore && ball.lastlastHolder != ball.lastHolder) {
                ball.lastlastHolder.myCharacter.seasonStats.assists++;
                ball.lastlastHolder.myCharacter.xp += Game.XPAssist;
                AddAnnouncerLine("It was set up with the assist from " + ball.lastlastHolder.myCharacter.name);
            }
            string goalString = announcerGoalLines[Random.Range(0, announcerGoalLines.Length)].Replace("XYZ", ball.lastHolder.myCharacter.name);
            AddAnnouncerLine(goalString);
            
            ball.lastlastHolder = null;
            ball.lastHolder = null;
        }
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

        }
        else {
            if (continuePanel != null) {
                continuePanel.SetActive(true);
            }

            var allPlayers = FindObjectsOfType<StableCombatChar>();
            for (int i = 0; i < allPlayers.Length; i++) {
                allPlayers[i].transform.position = new Vector3(1000, 1000, 1000);
                Destroy(allPlayers[i].gameObject);
            }

            StartCoroutine(DelayContinueKickoff());
        }

    }

    public void CancelDelayContinueKickoff() {
        StopAllCoroutines();
        continueCountdown.gameObject.SetActive(false);
    }

    public Image continueCountdown;
    IEnumerator DelayContinueKickoff() {
        if (continueCountdown == null) {
            kickoffButton.onClick.Invoke();
            yield break;
        }
        continueCountdown.gameObject.SetActive(true);
        float countdown = 5.0f;
        while (countdown >= 0.0f) {
            continueCountdown.fillAmount = countdown / 5.0f;
            countdown -= Time.deltaTime;
            yield return null;
        }
        kickoffButton.onClick.Invoke();
    }
    void UpdateScoreboard() {
        scoreboard.text = match.home.stable.stableName+": " + homeScore + "  "+ match.away.stable.stableName + ": " + awayScore;
    }
    bool gameOver;
    private void Update() {
        if (gameOver) { return; }
        if (Time.frameCount % 30 != 0) { return; }
        UpdateAnnouncerLines();
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
        if (match.home.stable == Game.instance.playerStable) {
            Game.instance.playerStable.finance.AddRevenue(500, "Match Income", "Match");
        }

        foreach (StableCombatChar homePlayer in homeCoach.players) {
            homePlayer.myCharacter.seasonStats.games++;
            homePlayer.myCharacter.xp += Game.XPGame;
        }
        foreach (StableCombatChar awayPlayer in awayCoach.players) {
            awayPlayer.myCharacter.seasonStats.games++;
            awayPlayer.myCharacter.xp += Game.XPGame;
        }
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
        Game.instance.managementScreenToLoadOnStartup = "league";
        SceneManager.LoadScene("StableManagement");
    }

    //Announcer

    public void AddAnnouncerLine(string _line) {
        Debug.Log("#Announcer#Adding " + _line);
        announcerLines.Add(new AnnouncerLine() { line = _line, endTime = Time.time + 5f });
        UpdateAnnouncerLines(true);
    }

    public void UpdateAnnouncerLines(bool changed = false) {
        if (announcer == null) {
            return;}
            for (int i = 0; i < announcerLines.Count; i++){
            if (announcerLines[i].endTime > Time.time) {
                break;
            }
            else {
                announcerLines[i] = null;
                changed = true;
            }
        }
        announcerLines.RemoveAll(x => x == null);
        if (changed) {
            announcer.text = "";
            for (int x = announcerLines.Count-1; x >= 0; x--) {
                announcer.text += announcerLines[x].line + "\n";
            }
        }
    }
    public static readonly string[] announcerGoalLines = {
"And that's a goal for XYZ!",
"What a great goal by XYZ!",
"That was an amazing goal by XYZ!",
"Wow, what a great goal by XYZ!",
"That was an incredible goal by XYZ!",
"XYZ scores a great goal!",
"An amazing goal by XYZ!",
"That's a goal for XYZ!",
"Wow, XYZ is on fire!",
"What a great goal for XYZ!",
"XYZ is really turning it on now!",
"XYZ is absolutely dominating now!",
"XYZ is just too good today!",
"XYZ is just unstoppable today!",
"What a performance by XYZ!",
};
}
