using Animancer;
using MoreMountains.Feedbacks;
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
    public Character myCharacter;
    
    public Position fieldPosition;
    public bool fieldSport = false;
    
    //Feedbacks
    public MMFeedbacks goOnRun;
    public MMFeedbacks sendOnRun;
    public MMFeedbacks takeDamage;
    public Vector3 position { get { return _t.position; } }
    public Coach coach;
    
    //Combat
    public StableCombatChar myAttackTarget;
    public CombatFocus combatFocus;
    public float aggroRadius = 100f;
    public float attackRange = 5f;
    public float lastAttack = 0f;
    public List<Move> baseAttackMoves;
    public Transform RH, LH, LL, RL;
    public SCWeapon RHWeapon, LHWeapon, RLWeapon, LLWeapon;

    //Combat Attributes
    public float health, stamina, balance, mind, maxHealth, maxStamina, maxBalance, maxMind;

    public void Init()
    {
        controller = this;
        _t = transform;
        anima = GetComponent<AnimancerController>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        baseAttackMoves = myCharacter.activeMoves;
        agent.speed = myCharacter.runSpeed * .4f;
        if (fieldSport) {
            int teammateCount = 0;
            int enemycount = 0;
            ball = FindObjectOfType<Ball>();
            Goal[] tempGoals = FindObjectsOfType<Goal>();
            foreach (var tg in tempGoals) {
                if (tg.team == team) {
                    myGoal = tg;
                }
                else { enemyGoal = tg; }
            }
            var tempChars = FindObjectsOfType<StableCombatChar>();
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
            StableCombatCharState initState = new SCIdle();
            initState.owner = this;
            this.state = initState;
            initState.thisChar = this;
            initState.EnterFrom(null);
        } else {
            SCCombatStanceState initState = new SCCombatIdle();
            initState.owner = this;
            this.state = initState;
            initState.thisChar = this;
            initState.EnterFrom(null);
        }
        maxHealth = health = myCharacter.maxHealth;
        maxStamina = stamina = myCharacter.maxStamina;
        maxBalance = balance = myCharacter.maxBalance;
        maxMind = mind = myCharacter.maxMind;
        WeaponSetup();
    }
    bool weaponsInited;
    void WeaponSetup() {
        
        if (weaponsInited) { return; }
        weaponsInited = true;
        Character character = myCharacter;
        if (character.weapon == null || character.weapon.name == "") {
            Weapon startingWeaponSO = Resources.Load<Weapon>(character.startingWeapon);
            character.weapon = Instantiate(startingWeaponSO);
        }
        Weapon weaponBlueprint = character.weapon;
        SCWeapon weaponPrefab = Resources.Load<GameObject>(weaponBlueprint.prefabName).GetComponent<SCWeapon>();
        SCWeapon defaultFists = Resources.Load<GameObject>("Fists").GetComponent<SCWeapon>();
        RHWeapon = Instantiate<SCWeapon>(weaponPrefab, RH);
        RHWeapon.transform.localPosition = Vector3.zero;
        RHWeapon.transform.localRotation = Quaternion.identity;
        RHWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        SCWeapon leftHandPrefab = weaponBlueprint.dualWield ? weaponPrefab : defaultFists;
        LHWeapon = Instantiate<SCWeapon>(leftHandPrefab, LH);
        LHWeapon.transform.localPosition = Vector3.zero;
        LHWeapon.transform.localRotation = Quaternion.identity;
        LHWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        SCWeapon legWeaponPrefab = weaponBlueprint.usesLegs ? Resources.Load<GameObject>(weaponBlueprint.prefabNameLegs).GetComponent<SCWeapon>() : defaultFists;
        LLWeapon = Instantiate<SCWeapon>(legWeaponPrefab, LL);
        LLWeapon.transform.localPosition = Vector3.zero;
        LLWeapon.transform.localRotation = Quaternion.identity;
        LLWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        RLWeapon = Instantiate<SCWeapon>(legWeaponPrefab, RL);
        RLWeapon.transform.localPosition = Vector3.zero;
        RLWeapon.transform.localRotation = Quaternion.identity;
        RLWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);
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
    public void TryCatchPass() {
        state.TransitionTo(new SCTryCatchPass());
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
            goOnRun.PlayFeedbacks();
        }
    }
    public void OneTimerToGoal() {
        state.TransitionTo(new SCOneTimerToGoal());
    }
    public void Tackle() {
        state.TransitionTo(new SCTackle());
    }
    public void GetTackled(StableCombatChar tackler) {
        state.TransitionTo(new SCGetTackled());
    }
    public void DodgeTackle(StableCombatChar tackler) {
        state.TransitionTo(new SCDodgeTackle() { tackler = tackler });
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

    public void GoalScored() {
        state.TransitionTo(new SCGoalScored());
    }
    public void CombatIdle() {
        state.TransitionTo(new SCCombatIdle());
    }
    public void CombatAttack() {
        state.TransitionTo(new SCCombatAttack());
    }
    public void CombatPursueTarget() {
        state.TransitionTo(new SCCombatPursueTarget());
    }

    public void MeleeScanDamage(string message) {
        switch (message) {
            case "BeginScanLH":
                LHWeapon.Scan();
                break;
            case "BeginScanRH":
                RHWeapon.Scan();
                break;
            case "BeginScanLL":
                LLWeapon.Scan();
                break;
            case "BeginScanRL":
                RLWeapon.Scan();
                break;
            case "EndScanRH":
                RHWeapon.EndScan();
                break;
            case "EndScanLH":
                LHWeapon.EndScan();
                break;
            case "EndScanRL":
                RLWeapon.EndScan();
                break;
            case "EndScanLL":
                LLWeapon.EndScan();
                break;
            case "EndAll":
                RHWeapon?.EndScan();
                LHWeapon?.EndScan();
                RLWeapon?.EndScan();
                LLWeapon?.EndScan();
                break;
        }
    }


    float lastRunCalled;
    public void SendTeammateOnRun() {
       if (lastRunCalled + 3 < Time.time) {
            lastRunCalled = Time.time;
            int teammateToSend = Random.Range(0, coach.players.Length);
            state.SendMessage(coach.players[teammateToSend], "RunToOpposingGoal");
            sendOnRun.PlayFeedbacks();
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

    public bool MyTargetIsInAttackRange() {
        if (myAttackTarget == null) { Debug.LogError("myAttackTarget is null"); }
        return (Vector3.Distance(_t.position, myAttackTarget.transform.position) < attackRange);
    }
    public float TotalCooldown() {
        float f = 0;
        foreach (Move m in baseAttackMoves) {
            f += m.cooldown;
        }
        return f;
    }
    public bool IsCoolingDown() {
        return (Time.time < lastAttack + TotalCooldown());
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

    public void TakeDamage(StableDamage damage) {
        if (health <= 0)
            return;
        takeDamage.PlayFeedbacks();
        state.TransitionTo(new SCTakeDamage());
        if (stamina <= 0 || balance <= 0 || mind <= 0) {
            health -= damage.health;
            Debug.Log("TODO: more health damage adjustments needed");
        }
        stamina -= damage.stamina;
        balance -= damage.balance;
        mind -= damage.mind;
   
        if (health <= 0) {
            print("DEAD!!!!");
            //Die();
        }
    }

    public void AnimEventReceiver(string message) {
        state.AnimEventReceiver(message);
    }

    void OnDrawGizmos() {
#if UNITY_EDITOR
        if (debugState && state!=null) {
            Handles.Label(transform.position+new Vector3(0,2,0), state.GetType().ToString() + "\nTackling: " + myCharacter.tackling + "\nDodging: " + myCharacter.dodging + "\nType: "+myCharacter.archetype);
        }
#endif
    }

}

public enum Position { LW, ST, RW, DR, DC, DL }
public enum CombatFocus { Melee, Ranged }
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