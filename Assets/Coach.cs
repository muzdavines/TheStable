using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coach : MonoBehaviour
{
    public int team;
    public StableCombatChar[] players;
    public StableCombatChar[] otherTeam;
    public List<StableCombatChar> pursuingBall;
    Ball ball;
    public bool isPlayer;
    
    public Transform[] positions;
    public List<StableCombatChar> otherTeamList;
    private void Start() {
        
    }
    public void Init() {
        var tempChars = FindObjectsOfType<StableCombatChar>();
        int teammateCount = 0;
        int enemycount = 0;
        ball = FindObjectOfType<Ball>();
        foreach (var tc in tempChars) {
            if (tc == this) { continue; }
            if (tc.team == team) { teammateCount++; } else { enemycount++; }
        }
        players = new StableCombatChar[teammateCount];
        otherTeam = new StableCombatChar[enemycount];
        teammateCount = enemycount = 0;
        for (int i = 0; i < tempChars.Length; i++) {
            if (tempChars[i] == this) { continue; }
            if (tempChars[i].team == team) { players[teammateCount++] = tempChars[i]; } else { otherTeam[enemycount++] = tempChars[i]; }
        }
        otherTeamList = new List<StableCombatChar>();
        foreach (var s in otherTeam) {
            otherTeamList.Add(s);
        }
    }

    public void CheckDivineIntervention() {
        foreach (var p in players) {
            foreach (var a in p.myCharacter.activeSpecialMoves) {
                if (a.GetType() == typeof(DivineIntervention)) {
                    a.Check(p);
                }
            }
        }
    }

    public bool AddBallPursuer(StableCombatChar newPursuer) {
        for (int i = 0; i<players.Length; i++) {
            if (players[i].isKnockedDown) { continue; }
            if (ball.Distance(players[i]) < ball.Distance(newPursuer)) { return false; }
        }
        return true;
    }
   
    public void ProcessPursuers() {
        //use distance right now, but later check for skill too
        //let the two closest pursue
        foreach (var p in pursuingBall) {

        }
    }




}
