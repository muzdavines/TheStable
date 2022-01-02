using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public class StableCombatChar : MonoBehaviour, StableCombatCharStateOwner
{
    public StableCombatCharState state { get; set; }
    public StableCombatChar controller { get; set; }
    
    public NavMeshAgent agent;
    public Animator anim;
    public AnimancerController anima;
    public Transform _t;
    public Transform _rightHand;
    public bool debugState;
    public int team;
    public Ball ball;
    public float distToTrackBall;
    public float distToTrackEnemy;
    public float distToShoot;
    public float distToTrackBallCarrier;
    public Goal myGoal, enemyGoal;
    
    public Position fieldPosition;
    
    public Vector3 position { get { return _t.position; } }

    public int tackling = 10;
    public int dodging = 10;
    public int blocking = 10;

    public Coach coach;
    
    void Start()
    {
        controller = this;
        anima = GetComponent<AnimancerController>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        ball = FindObjectOfType<Ball>();

        Goal[] tempGoals = FindObjectsOfType<Goal>();
        foreach (var tg in tempGoals) {
            if (tg.team == team) {
                myGoal = tg;
            } else { enemyGoal = tg; }
        }
        _t = transform;
        StableCombatCharState initState = new SCIdle();
        initState.owner = this;
        this.state = initState;
        initState.thisChar = this;
        initState.EnterFrom(null);
        tackling = Random.Range(8, 18);
        dodging = Random.Range(8, 18);
        blocking = Random.Range(8, 18);
        var tempChars = FindObjectsOfType<StableCombatChar>();
        int teammateCount = 0;
        int enemycount = 0;
        foreach (var tc in tempChars) {
            if (tc == this) { continue; }
            if (tc.team == team) { teammateCount++; } else { enemycount++; }
        }
        foreach (var thisCoach in FindObjectsOfType<Coach>()) {
            if (thisCoach.team == team) {
                coach = thisCoach;
                break;
            }
        }
    }

   void Update()
    {
        state.Update();
    }
    
    public bool ShouldPursueBall() {
        if (ball == null) { return false; }
        if (ball.isHeld) { return false; }
        if (ball.Distance(transform.position) <= distToTrackBall) {
            return coach.AddBallPursuer(this);
        }
        return false;
    }
    public bool ShouldPursueEnemy() {
        return false;
    }
    public bool ShouldPursueBallCarrier() {
        if (ball.holder == null) { return false; }
        if (ball.holder.team == team) { return false; }
        if (Vector3.Distance(ball.holder.transform.position, transform.position) <= distToTrackBallCarrier) {
            return true;
        }
        return false;
    }
    public bool ShouldShoot() {
        if (enemyGoal.Distance(this) <= distToShoot) {
            return true;
        }
        return false;
    }

    public bool ShouldPass() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);
        foreach (var hitCollider in hitColliders) {
            var checkChar = hitCollider.GetComponent<StableCombatChar>();
            if (checkChar != null) {
                if (checkChar.team != team) {
                    return true;
                }
            }
        }
        foreach (var teammate in coach.players) {
            if (enemyGoal.Distance(this) > enemyGoal.Distance(teammate)) {
                return true;
            }
        }
        return false;
    }
    public StableCombatChar GetPassTarget() {
        int passTargetScore = 0;
        StableCombatChar currentTarget = null;
        foreach (var teammate in coach.players) {
            Debug.Log("#PassTargetEval#" + teammate.name + " current state is " + teammate.state.GetType().ToString());
            if (Vector3.Distance(teammate.transform.position, position) < 7) {
                Debug.Log("#PassTargetEval#" + teammate.name + " is too close");
                continue;
            }
            if (teammate.state.GetType() == typeof(SCKnockdown)) {
                Debug.Log("#PassTargetEval#" + teammate.name + " is too knocked down.");
                continue;
            }
            RaycastHit hit;
            if (Physics.Raycast(position, (teammate.position - position), out hit)){
                if (hit.transform.GetComponent<StableCombatChar>() != teammate) {
                    continue;
                }
            }
            //Debug.Log("#PassTargetEval# My Dist to Goal: " + enemyGoal.Distance(this) + "  Teammate Dist to Goal: " + enemyGoal.Distance(teammate));
            if (enemyGoal.Distance(this) > enemyGoal.Distance(teammate)) {
                if (passTargetScore <= 5) {
                    passTargetScore = 5;
                    currentTarget = teammate;
                    Debug.Log("#PassTargetEval#" + teammate.name + " is closer to the goal than me, marking for pass.");
                }
            }
            else { Debug.Log("#PassTargetEval#" + teammate.name + " is further from the goal than me."); continue; }
            if (teammate.state.GetType() == typeof(SCRunToGoalWithoutBall)) {
                if (passTargetScore <= 10) {
                    passTargetScore = 10;
                    currentTarget = teammate;
                    Debug.Log("#PassTargetEval#" + teammate.name + " is on a run. Marking.");
                }
            }
        }
        if (currentTarget == null) { Debug.Log("#PassTargetEvalReturn# return is null."); }
        else {
            Debug.Log("#PassTargetEvalReturn#" + currentTarget.name + " is my pass target.");
        }
        return currentTarget;
    }
    public bool ShouldSignalTeammateToRun() {

        return false;
    }

    public void Idle() {
        state.TransitionTo(new SCIdle());
    }
    public void IdleWithBall() {
        state.TransitionTo(new SCIdleWithBall());
    }
    public void IdleTeammateWithBall() {
        state.TransitionTo(new SCIdleTeammateWithBall());
    }
    public void PursueBall() {
        state.TransitionTo(new SCPursueBall());
    }
    public void GoToPosition() {
        state.TransitionTo(new SCGoToPosition());
    }

    public void PickupBall() {
        state.TransitionTo(new SCPickupBall());
    }
    public void Shoot() {
        state.TransitionTo(new SCShoot());
    }
    public void PursueBallCarrier() {
        state.TransitionTo(new SCPursueBallCarrier());
    }
    public void RunToGoalWithBall() {
        state.TransitionTo(new SCRunToGoalWithBall());
    }
    public void RunToGoalWithoutBall() {
        Debug.Log("#ThisChar#RunToGoalWithoutball" + state.GetType());
        if (state.GetType() != typeof(SCRunToGoalWithoutBall)) {
            state.TransitionTo(new SCRunToGoalWithoutBall());
        }
    }
    public void Tackle() {
        state.TransitionTo(new SCTackle());
    }
    public void GetTackled() {
        state.TransitionTo(new SCGetTackled());
    }
    public void DodgeTackle() {
        state.TransitionTo(new SCDodgeTackle());
    }
    public void MissTackle() {
        state.TransitionTo(new SCMissTackle());
    }
    public void Pass(StableCombatChar passTarget) {
        state.TransitionTo(new SCPass() { passTarget = passTarget });
    }
    public void Block() {
        state.TransitionTo(new SCBlockForTeammate());
    }
    float lastRunCalled;
    public void SendTeammateOnRun() {
       if (lastRunCalled + 3 < Time.time) {
            lastRunCalled = Time.time;
            int teammateToSend = Random.Range(0, coach.players.Length);
            state.SendMessage(coach.players[teammateToSend], "RunToOpposingGoal");
       }
    }
    public StableCombatChar GetNearestTeammate() {
        StableCombatChar[] allChars =FindObjectsOfType<StableCombatChar>();
        foreach (var c in allChars) {
            if (c.team == team) {
                return c;
            }
        }
        return null;
    }
    public Transform GetFieldPosition() {
        switch (fieldPosition) {
            case Position.ST:
                return coach.positions.ST;
                break;
            case Position.LW:
                return coach.positions.LW;
                break;
            case Position.RW:
                return coach.positions.RW;
                break;
            case Position.DC:
                return coach.positions.DC;
                break;
            case Position.DL:
                return coach.positions.DL;
                break;
            case Position.DR:
                return coach.positions.DR;
                break;
        }
        return null;
    }

    public void AnimEventReceiver(string message) {
        state.AnimEventReceiver(message);
    }

    void OnDrawGizmos() {
#if UNITY_EDITOR
        if (debugState && state!=null) {
            Handles.Label(transform.position+new Vector3(0,1,0), state.GetType().ToString() + "\nTackling: " + tackling + "\nDodging: " + dodging);
        }
#endif
    }

}

public enum Position { LW, ST, RW, DR, DC, DL }

public static class StableCombatCharHelper {
    public static void ResetAllTriggers(this Animator anim) {

        foreach (var trigger in anim.parameters) {
            if (trigger.type == AnimatorControllerParameterType.Trigger) {
                anim.ResetTrigger(trigger.name);
            }
        }
    }
    public static float Distance(this StableCombatChar thisChar, StableCombatChar otherChar) {
        return Vector3.Distance(thisChar.position, otherChar.position);
    }
}