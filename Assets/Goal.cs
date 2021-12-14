using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int team;
}

public static class GoalHelper {
    public static float Distance(this Goal goal, StableCombatChar other) {
        return Vector3.Distance(goal.transform.position, other.transform.position);
    }
}
