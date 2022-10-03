using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSportSoundManager : MonoBehaviour {
    public AudioSource ambient, oneShot, oneShotGoal;
    public AudioClip goal, nearGoal, ooh;
    private float lastNearGoalOneShot;
    private float nearGoalCooldown = 10f;
    private float oohCooldown = 3f;
    private float lastOoh;
    private MatchController match;
    private Ball ball;
    private List<Goal> goals;
    void Start() {
        match = FindObjectOfType<MatchController>();
        ball = match.ball;
        goals = new List<Goal>();
        foreach (Goal g in FindObjectsOfType<Goal>()) {
            goals.Add(g);
        }
        
    }
    void Update() {
        if (Time.time > lastNearGoalOneShot + nearGoalCooldown) {
            CheckIfNearGoal();
        }
    }

    public void Goal() {
        oneShotGoal.PlayOneShot(goal);
    }

    public void Ooh() {
        if (Time.time > lastOoh + oohCooldown) {
            lastOoh = Time.time;
            oneShot.PlayOneShot(ooh, 1);
        }
    }

    void CheckIfNearGoal() {
        foreach (var g in goals) {
            if (ball.Distance(g.transform) < 9) {
                lastNearGoalOneShot = Time.time;
                oneShot.PlayOneShot(nearGoal);
                return;
            }
        }
    }
}
