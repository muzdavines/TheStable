using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int team;
    public bool canScore;
    public MMFeedbacks goalScoredFeedback;
    public Transform topRight;
    public void OnCollisionEnter(Collision collision) {
        if (collision.transform.GetComponent<Ball>()) {
            FindObjectOfType<MatchController>().ScoreGoal(team == 0 ? 1 : 0);
        }
    }
    public void OnTriggerEnter(Collider other) {
        if (!canScore) { return; }
        Ball thisBall = other.transform.GetComponent<Ball>();
        if (other.transform.GetComponent<Ball>()) {
            canScore = false;
            Character thisChar = thisBall.lastHolder.myCharacter;
            FindObjectOfType<MatchController>().ScoreGoal(team == 0 ? 1 : 0);
            goalScoredFeedback.GetComponent<MMFeedbackFloatingText>().Value = "Goal by " + thisChar.name + "\nSeason Goals: " + thisChar.seasonStats.goals;
            goalScoredFeedback.PlayFeedbacks();
        }
    }
}

public static class GoalHelper {
    public static float Distance(this Goal goal, StableCombatChar other) {
        return Vector3.Distance(goal.transform.position, other.transform.position);
    }
}
