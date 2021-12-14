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
    public Transform _t;
    public bool debugState;
    public int team;
    public Ball ball;
    public float distToTrackBall;
    public float distToTrackEnemy;
    public float distToShoot;
    public float distToTrackBallCarrier;
    public Goal myGoal, enemyGoal;
    void Start()
    {
        controller = this;
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
    }

   void Update()
    {
        state.Update();
    }
    public void SendMessageToPlayer(string s) {

    }

    public void ReceiveMessageFromPlayer(StableCombatChar sender, string s) {

    }

    public bool ShouldPursueBall() {
        if (ball == null) { return false; }
        if (ball.isHeld) { return false; }
        if (ball.Distance(transform.position) <= distToTrackBall) { return true; }
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
                    //enemy in range
                    return true;
                }
            }
        }
        return false;
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
    public void PursueBall() {
        state.TransitionTo(new SCPursueBall());
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
    public void Tackle() {
        state.TransitionTo(new SCTackle());
    }
    public void GetTackled() {
        state.TransitionTo(new SCGetTackled());
    }
    public void Pass() {
        state.TransitionTo(new SCPass());
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

    public void AnimEventReceiver(string message) {
        state.AnimEventReceiver(message);
    }

    void OnDrawGizmos() {
        if (debugState && state!=null) {
            Handles.Label(transform.position, state.GetType().ToString());
        }
    }
}
