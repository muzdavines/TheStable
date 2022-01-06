using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int team;
    public bool canScore;
    public void OnCollisionEnter(Collision collision) {
        if (collision.transform.GetComponent<Ball>()) {
            FindObjectOfType<MatchController>().ScoreGoal(team == 0 ? 1 : 0);
        }
    }
    public void OnTriggerEnter(Collider other) {
        if (!canScore) { return; }
        if (other.transform.GetComponent<Ball>()) {
            canScore = false;
            FindObjectOfType<MatchController>().ScoreGoal(team == 0 ? 1 : 0);
        }
    }
}

public static class GoalHelper {
    public static float Distance(this Goal goal, StableCombatChar other) {
        return Vector3.Distance(goal.transform.position, other.transform.position);
    }
}
