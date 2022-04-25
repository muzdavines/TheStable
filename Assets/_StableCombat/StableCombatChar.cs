using Animancer;
using MoreMountains.Feedbacks;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class StableCombatChar : MonoBehaviour, StableCombatCharStateOwner
{
    public StableCombatCharState state { get; set; }
    public StableCombatChar controller { get; set; }
    public const int maxTickets = 4;
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
    public MMFeedbacks playerHasBall;
    public MMFeedbacks shotAccuracy;
    public MMFeedbacks specialAbil;
    public Vector3 position { get { return _t.position; } }
    public Coach coach;
    
    //Combat
    public StableCombatChar myAttackTarget;
    [SerializeField]
    public CombatTicket myAttackTargetTicket;
    [SerializeField]
    public List<CombatTicket> myCombatTickets;
    public CombatFocus combatFocus;
    public PlayStyle playStyle;
    public CombatEngagementStatus combatEngagementStatus;
    public float aggroRadius = 100f;
    public float attackRange = 5f;
    public float lastAttack = 0f;
    public List<Move> meleeAttackMoves;
    public List<Move> rangedAttackMoves;
    public Transform RH, LH, LL, RL;
    public SCWeapon RHMWeapon, LHMWeapon, RLWeapon, LLWeapon;
    public SCWeapon RHRWeapon, LHRWeapon;
    
    public HeroFrame uiController;
    //Combat Attributes
    public float health, stamina, balance, mind, maxHealth, maxStamina, maxBalance, maxMind;
    [System.Serializable]
    public class Mod {
        public float timeEnd;
        public float modAmount;
        public GameObject modEffect;
        public ModType modType;
    }
    [SerializeField]
    public List<Mod> mods = new List<Mod>();

    //Status

    public bool isKnockedDown { get { return state.GetType() == typeof(SCGetTackled) || state.GetType() == typeof(SCKnockdown) || state.GetType() == typeof(SCCombatDowned); } }
    public bool isStateLocked { get { return state.GetType().GetInterfaces().Contains(typeof(CannotInterrupt)); } }
    //Sport
    public float tackleCooldown; //Time after which the player can tackle again

    public void Init()
    {
        controller = this;
        _t = transform;
        myAttackTargetTicket = new CombatTicket();
        myCombatTickets = new List<CombatTicket>();
        for (int c = 0; c<maxTickets; c++) {
            myCombatTickets.Add(new CombatTicket() { owner = this });
        }
        anima = GetComponent<AnimancerController>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        meleeAttackMoves = myCharacter.activeMeleeMoves;
        rangedAttackMoves = myCharacter.activeRangedMoves;
        agent.speed = myCharacter.runspeed * .4f;
        agent.radius = .6f;
        accumulatedCooldown = 4f;
        WeaponSetup();
        debugState = true;
        if (fieldSport) {
            int teammateCount = 0;
            int enemycount = 0;
            agent.radius = .2f;
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
            StableCombatCharState initState = new SCMissionIdle() { act = true } ;
            initState.owner = this;
            this.state = initState;
            initState.thisChar = this;
            initState.EnterFrom(null);
        }
        combatFocus = myCharacter.combatFocus;
        maxHealth = health = myCharacter.maxHealth;
        maxStamina = stamina = myCharacter.maxStamina;
        maxBalance = balance = myCharacter.maxBalance;
        maxMind = mind = myCharacter.maxMind;
        if (myCharacter.archetype == Character.Archetype.Goalkeeper) {
            var gkblocker = Instantiate(Resources.Load<GameObject>("GKBlocker"));
            gkblocker.transform.parent = transform;
            gkblocker.transform.localRotation = Quaternion.Euler(Vector3.zero);
            gkblocker.transform.localPosition = Vector3.zero;
        }
        switch (myCharacter.archetype) {
            case Character.Archetype.Warrior:
                distToShoot = 8;
                break;
        }
    }
    bool weaponsInited;
    void WeaponSetup() {
        
        if (weaponsInited) { return; }
        weaponsInited = true;
        Character character = myCharacter;
        if (character.meleeWeapon == null || character.meleeWeapon.name == "") {
            Weapon startingWeaponSO = Resources.Load<Weapon>(character.startingMeleeWeapon);
            character.meleeWeapon = Instantiate(startingWeaponSO);
        }
        Weapon weaponBlueprint = character.meleeWeapon;
        SCWeapon weaponPrefab = Resources.Load<GameObject>(weaponBlueprint.prefabName).GetComponent<SCWeapon>();
        SCWeapon defaultFists = Resources.Load<GameObject>("Fists").GetComponent<SCWeapon>();
        RHMWeapon = Instantiate<SCWeapon>(weaponPrefab);
        RHMWeapon.transform.parent = RH;
        RHMWeapon.transform.localPosition = Vector3.zero;
        RHMWeapon.transform.localRotation = Quaternion.identity;
        
        RHMWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        SCWeapon leftHandPrefab = weaponBlueprint.dualWield ? weaponPrefab : defaultFists;
        LHMWeapon = Instantiate<SCWeapon>(leftHandPrefab);
        LHMWeapon.transform.parent = LH;
        LHMWeapon.transform.localPosition = Vector3.zero;
        LHMWeapon.transform.localRotation = Quaternion.identity;
        LHMWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        SCWeapon legWeaponPrefab = weaponBlueprint.usesLegs ? Resources.Load<GameObject>(weaponBlueprint.prefabNameLegs).GetComponent<SCWeapon>() : defaultFists;
        LLWeapon = Instantiate<SCWeapon>(legWeaponPrefab, LL);
        LLWeapon.transform.localPosition = Vector3.zero;
        LLWeapon.transform.localRotation = Quaternion.identity;
        LLWeapon.transform.localScale = Vector3.one;
        LLWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        RLWeapon = Instantiate<SCWeapon>(legWeaponPrefab, RL);
        RLWeapon.transform.localPosition = Vector3.zero;
        RLWeapon.transform.localRotation = Quaternion.identity;
        RLWeapon.transform.localScale = Vector3.one;
        RLWeapon.GetComponent<SCWeapon>().Init(this, weaponBlueprint);

        //Ranged Weapon Setup
        if (character.rangedWeapon == null || character.rangedWeapon.name == "") {
            Weapon startingRangedWeaponSO = Resources.Load<Weapon>(character.startingRangedWeapon);
            character.rangedWeapon = Instantiate(startingRangedWeaponSO);
        }
        Weapon rangedWeaponBlueprint = character.rangedWeapon;
        SCWeapon rangedWeaponPrefab = Resources.Load<GameObject>(rangedWeaponBlueprint.prefabName).GetComponent<SCWeapon>();
        RHRWeapon = Instantiate<SCWeapon>(rangedWeaponPrefab, RH);
        RHRWeapon.transform.localPosition = Vector3.zero;
        RHRWeapon.transform.localRotation = Quaternion.identity;
        RHRWeapon.GetComponent<SCWeapon>().Init(this, rangedWeaponBlueprint);
        RHRWeapon.gameObject.SetActive(false);
        if (rangedWeaponBlueprint.dualWield) {
            LHRWeapon = Instantiate<SCWeapon>(rangedWeaponPrefab, LH);
            LHRWeapon.transform.localPosition = Vector3.zero;
            LHRWeapon.transform.localRotation = Quaternion.identity;
            LHRWeapon.GetComponent<SCWeapon>().Init(this, rangedWeaponBlueprint);
            LHRWeapon.gameObject.SetActive(false);
        }
        RHMWeapon.gameObject.SetActive(false);
        LHMWeapon.gameObject.SetActive(false);
        LLWeapon.gameObject.SetActive(false);
        RLWeapon.gameObject.SetActive(false);
    }

   void Update()
    {
        state.Update();
        for (int i = 0; i < mods.Count; i++) {
            if (mods[i] != null && Time.time >= mods[i].timeEnd) {
                EndMod(mods[i]);
                mods[i] = null;
            }
        }
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

    public GuardNetPosition ShouldGuardNet() {
        bool centerTaken = false;
        bool leftTaken = false;
        bool rightTaken = false;
        
        foreach (StableCombatChar teammate in coach.players) {
            if (teammate.state.GetType() != typeof(SCGuardNet)) { continue; }
            GuardNetPosition currentPos = ((SCGuardNet)teammate.state).guardPosition;
            if (currentPos == GuardNetPosition.Center) {
                centerTaken = true;
            }
            if (currentPos == GuardNetPosition.Left) {
                leftTaken = true;
            }
            if (currentPos == GuardNetPosition.Right) {
                rightTaken = true;
            }
        }
       if (!centerTaken) { return GuardNetPosition.Center; }
       if (!leftTaken) { return GuardNetPosition.Left; }
       if (!rightTaken) { return GuardNetPosition.Right; }
       return GuardNetPosition.None;
    }
    public bool ShouldPass() {
        Vector3 goalDirection = enemyGoal.transform.position - transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7f);
        foreach (var hitCollider in hitColliders) {
            var checkChar = hitCollider.GetComponent<StableCombatChar>();
            if (checkChar != null) {
                Vector3 hitDirection = hitCollider.transform.position - transform.position;

                if (checkChar.team != team && Vector3.Dot(hitDirection, goalDirection) > 4f)  {
                    return true;
                }
            }
        }
        foreach (var teammate in coach.players) {
            float distToGoal = enemyGoal.Distance(teammate);
            if (distToGoal < 15 && enemyGoal.Distance(this) > distToGoal) {
                return true;
            }
        }
        return false;
    }
   
    public StableCombatChar GetPassTarget(PassTargetLogic logic = 0) {
        int passTargetScore = 0;
        int tempTargetScore = 0;
        StableCombatChar currentTarget = null;
        foreach (var teammate in coach.players) {
#if UNITY_EDITOR
            Debug.Log("#PassTargetEval#" + teammate.name + " current state is " + teammate.state.GetType().ToString());
#endif
            if (teammate.isKnockedDown) {
                //Debug.Log("#PassTargetEval#" + teammate.name + " is too knocked down.");
                continue;
            }
            if (teammate.isStateLocked) {
                continue;
            }
            if (logic.HasFlag(PassTargetLogic.DeepBall)) {
                if (teammate.Distance(this) >= 30 && enemyGoal.Distance(teammate) < 30) {
                    return teammate;
                }
                else { continue; }
            }
            if (logic.HasFlag(PassTargetLogic.Wizard) && teammate.myCharacter.archetype != Character.Archetype.Wizard) {
                continue;
            }
            if (logic.HasFlag(PassTargetLogic.NearGoal) && enemyGoal.Distance(teammate)>20) {
                continue;
            } else { tempTargetScore += 100; }
            if (logic.HasFlag(PassTargetLogic.Rogue) && teammate.myCharacter.archetype != Character.Archetype.Rogue) {
                continue;
            } else {
                tempTargetScore += 100;
            }
            if (Vector3.Distance(teammate.transform.position, position) < 7) {
                Debug.Log("#PassTargetEval#" + teammate.name + " is too close");
                continue;
            }
            if (Vector3.Distance(teammate.transform.position, position) > 25) {
                Debug.Log("#PassTargetEval#" + teammate.name + " is too far");
                continue;
            }

            /*RaycastHit hit;
            if (Physics.Raycast(position, (teammate.position - position), out hit)){
                if (hit.transform.GetComponent<StableCombatChar>() != teammate) {
                    continue;
                }
            }*/
            //Debug.Log("#PassTargetEval# My Dist to Goal: " + enemyGoal.Distance(this) + "  Teammate Dist to Goal: " + enemyGoal.Distance(teammate));
            if (enemyGoal.Distance(this) > enemyGoal.Distance(teammate)) {
                tempTargetScore += 5;
            } else if (!logic.HasFlag(PassTargetLogic.BackwardOK)){
                continue;
            }
            if (teammate.state.GetType() == typeof(SCRunToGoalWithoutBall)) {
               tempTargetScore += 10;
            }
            if (logic.HasFlag(PassTargetLogic.Open)) {
                print("#TODO#Check how open a pass target is and add points");
            }
            if (logic.HasFlag(PassTargetLogic.Rogue)) {
                if (teammate.myCharacter.archetype == Character.Archetype.Rogue) {
                    tempTargetScore += 11;
                }
            }
            if (tempTargetScore > passTargetScore) {
                currentTarget = teammate;
                passTargetScore = tempTargetScore;
            }
        }
        if (currentTarget == null) {
#if UNITY_EDITOR
            Debug.Log("#PassTargetEvalReturn# return is null.");
#endif

        }
        else {
#if UNITY_EDITOR
            Debug.Log("#PassTargetEvalReturn#" + currentTarget.name + " is my pass target.");
#endif
        }
        return currentTarget;
    }
    public bool ShouldSignalTeammateToRun() {

        return false;
    }
    public void Reset() {
        state.TransitionTo(new SCReset());
    }
    public void Idle() {
        playStyle = PlayStyle.Play;
        combatEngagementStatus = CombatEngagementStatus.None;
        state.TransitionTo(new SCIdle());
    }
    public void IdleWithBall() {
        state.TransitionTo(new SCIdleWithBall());
    }
    public void IdleTeammateWithBall() {
        state.TransitionTo(new SCIdleTeammateWithBall());
    }
    public void MissionIdle() {
        Debug.Log("#Mission# Mission Idle");
        state.TransitionTo(new SCMissionIdle() { act = true });
    }
    public void MissionIdleDontAct() {
        state.TransitionTo(new SCMissionIdle() { act = false });
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
        playerHasBall.GetComponent<MMFeedbackFloatingText>().Value = myCharacter.name;
        playerHasBall.PlayFeedbacks();
        state.TransitionTo(new SCPickupBall());
    }
    public void Shoot() {
        state.TransitionTo(new SCShoot());
    }
    public void DisplayShotAccuracy(float accuracy) {
        shotAccuracy.GetComponent<MMFeedbackFloatingText>().Value = (accuracy.ToString("F1"));
        shotAccuracy.PlayFeedbacks();
    }
    public void DisplaySpecialAbilityFeedback(string ability) {
        specialAbil.GetComponent<MMFeedbackFloatingText>().Value = ability;
        specialAbil.PlayFeedbacks();
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
    public void GoNearEnemyGoal() {
        state.TransitionTo(new SCGoNearEnemyGoal());
    }
    public void OneTimerToGoal() {
        if (!myCharacter.HasMove("OneTimer")) {
            Idle();
            return;
        }
        state.TransitionTo(new SCOneTimerToGoal());
    }
    public void Tackle() {
        state.TransitionTo(new SCTackle());
    }
    public void GetTackled() {
        state.TransitionTo(new SCGetTackled());
    }
    public void DodgeTackle(StableCombatChar tackler) {
        state.TransitionTo(new SCDodgeTackle() { tackler = tackler });
    }
    public void BreakTackle(StableCombatChar tackler) {
        state.TransitionTo(new SCBreakTackle { tackler = tackler });
    }

    public void GetStripped(StableCombatChar tackler) {
        state.TransitionTo(new SCGetStripped() { tackler = tackler });
    }
    public void AvoidStrip(StableCombatChar tackler) {
        state.TransitionTo(new SCAvoidStrip() { tackler = tackler });
    }
    public void SuccessStrip() {
        state.TransitionTo(new SCSuccessStrip());
    }
    public void FailStrip() {
        state.TransitionTo(new SCFailStrip());
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
    public void BackOffCarrier(bool resetCooldowns = false) {
        if (isKnockedDown) { return; }
        if (resetCooldowns) { SetTackleCooldown(); }
        state.TransitionTo(new SCBackOffCarrier());
    }

    public void GKSwat() {
        if (isKnockedDown) {
            return;
        }
        state.TransitionTo(new SCGKSwat());
    }
    public void IntroState(Transform pos) {
        state.TransitionTo(new SCIntroState() { myPos = pos });
    }
    public void GoalScored() {
        state.TransitionTo(new SCGoalScored());
    }
    public void CombatIdle() {
        state.TransitionTo(new SCCombatIdle());
    }
    public void AggressorCombat() {
        combatEngagementStatus = CombatEngagementStatus.Aggressor;
        CombatIdle();
    }
    public void DefendCombat(StableCombatChar _attacker) {
        AcquireTarget(_attacker);
        combatEngagementStatus = CombatEngagementStatus.Defender;
        CombatIdle();
    }
    
    public void CombatAttack() {
        state.TransitionTo(new SCCombatAttack());
    }
    public void CombatPursueTarget() {
        state.TransitionTo(new SCCombatPursueTarget());
    }
    public void GetDowned() {
        state.TransitionTo(new SCCombatDowned());
    }

    public void GetRevived() {
        state.TransitionTo(new SCCombatRevive());
    }

    public void GuardNet(GuardNetPosition myPos) {
        state.TransitionTo(new SCGuardNet() { guardPosition = myPos });
    }
    public void ShoulderBarge() {
        state.TransitionTo(new SCShoulderBarge());
    }

    public void Backstab() {
        state.TransitionTo(new SCBackstab());
    }
    public void BackstabVictim() {
        state.TransitionTo(new SCBackstabVictim());
    }
    public void FlameCircle() {
        state.TransitionTo(new SCFlameCircle());
    }
    public void PowerSlam() {
        state.TransitionTo(new SCPowerSlam());
    }
    public void Flechettes() {
        state.TransitionTo(new SCFlechettes());
    }
    public void Summon(Character charToSummon) {

    }
    public void SummonFireGolem() {
        state.TransitionTo(new SCSummonFireGolem());
    }
    public void ClosingSpeed() {
        state.TransitionTo(new SCClosingSpeed());
    }
    public void DeepBall(StableCombatChar passTarget) {
        print("#DeepBall#DeepBall to " + passTarget.myCharacter.name);
        state.TransitionTo(new SCDeepBall() { passTarget = passTarget });
    }
    public void WallOfForce() {

    }
    public void Sniper() {

    }
    public void TeleportBall() {

    }
    public void Taunt() {

    }
    public void Blink() {

    }
    public void Freeze() {

    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="speedMod">speedMod should be in units of speed for the Character attribute. The method will convert the agent speed</param>
    public SCSpeedBuff SpeedBuff(float duration, float speedMod) {
        return gameObject.AddComponent<SCSpeedBuff>().Init(duration, speedMod);
    }

    public void GKDiveForBall() {

    }
    public void GKPursueBall() {
        state.TransitionTo(new SCPursueBall());
    }
    public void GKDefendNet() {
        state.TransitionTo(new SCGKDefendNet());
    }
    public void GKIdle() {
        state.TransitionTo(new SCGKIdle());
    }
    public void GKIdleWithBall() {
        state.TransitionTo(new SCGKIdleWithBall());
    }
    public void GKClearBall() {
        state.TransitionTo(new SCGKClearBall());
    }

    public void SetTackleCooldown() {
        tackleCooldown = Time.time + (4 - (myCharacter.tackling * .2f));
    }

    public void SetCombatFocus(CombatFocus newFocus) {
        combatFocus = newFocus;
    }
    public void SetPlayStyle(PlayStyle newStyle) {
        playStyle = newStyle;
    }

    public void MeleeScanDamage(string message) {
        switch (message) {
            case "BeginScanLH":
                LHMWeapon.Scan();
                break;
            case "BeginScanRH":
                RHMWeapon.Scan();
                break;
            case "BeginScanLL":
                LLWeapon.Scan();
                break;
            case "BeginScanRL":
                RLWeapon.Scan();
                break;
            case "EndScanRH":
                RHMWeapon.EndScan();
                break;
            case "EndScanLH":
                LHMWeapon.EndScan();
                break;
            case "EndScanRL":
                RLWeapon.EndScan();
                break;
            case "EndScanLL":
                LLWeapon.EndScan();
                break;
            case "EndAll":
                RHMWeapon?.EndScan();
                LHMWeapon?.EndScan();
                RLWeapon?.EndScan();
                LLWeapon?.EndScan();
                break;
        }
    }


    float lastRunCalled;
    public void SendTeammateOnRun() {
       if (lastRunCalled + 1.5f < Time.time) {
            lastRunCalled = Time.time;
            StableCombatChar nearest = GetNearestTeammate();
            nearest.GoNearEnemyGoal();
            
            //int teammateToSend = Random.Range(0, coach.players.Length);
            //state.SendMessage(coach.players[teammateToSend], "RunToOpposingGoal");
            //sendOnRun.PlayFeedbacks();
       }
    }
    public StableCombatChar GetNearestTeammate() {


        StableCombatChar[] allChars =FindObjectsOfType<StableCombatChar>();
        foreach (var c in allChars) {
            if (c.fieldPosition == Position.GK) { continue; }
            if (c == this) { continue; }
            if (c.isKnockedDown) { continue; }
            if (c.team == team) {
                return c;
            }
        }
        return null;
    }
    
    public StableCombatChar GetFarthestTeammateNearGoal() {
        float maxDist = 0;
        StableCombatChar returnChar = null;
        foreach (StableCombatChar c in coach.players) {
            if (c == this) { continue; }
            if (c.isKnockedDown) { continue; }
            if (enemyGoal.Distance(c) < 20 && c.Distance(this) > maxDist) {
                maxDist = c.Distance(this);
                returnChar = c;
            }
        }
        if (maxDist < 3) { returnChar = null; }
        return returnChar;
    }

    public bool MyTargetIsInAttackRange() {
        if (myAttackTarget == null) { Debug.LogError("myAttackTarget is null"); }
        return (Vector3.Distance(_t.position, myAttackTarget.transform.position) < attackRange);
    }
    public float TotalCooldown() {
        float f = 0;
        foreach (Move m in meleeAttackMoves) {
            f += m.cooldown;
        }
        return f;
    }
    public float accumulatedCooldown;
    public bool IsCoolingDown() {
        return (Time.time < lastAttack + accumulatedCooldown);
    }
    public Transform GetFieldPosition() {
        return coach.positions[(int)fieldPosition];
    }
    float lastTakeDamageAnim;
    public void TakeDamage(StableDamage damage, bool shouldAnimate = true) {
        if (health <= 0)
            return;
        takeDamage.PlayFeedbacks();
        if (shouldAnimate && Time.time > lastTakeDamageAnim + 3) {
            lastTakeDamageAnim = Time.time;
            state.TransitionTo(new SCTakeDamage());
        }
        if (stamina <= 0 || balance <= 0 || mind <= 0) {
            health -= damage.health;
            Debug.Log("TODO: more health damage adjustments needed");
        }
        stamina -= damage.stamina;
        balance -= damage.balance;
        mind -= damage.mind;
        uiController?.UpdateAll();
        if (health <= 0) {
            GetDowned();
        }
        if (damage.isKnockdown) {
            GetTackled();
        }
    }
    public void RestoreHealth() {
        health = maxHealth;
        stamina = maxStamina;
        balance = maxBalance;
        mind = maxMind;
        uiController?.UpdateAll();
    }

    public void MeleeWeaponsOn(bool on = true) {
        if (RHMWeapon.gameObject.activeInHierarchy==on) { return; }
        RHMWeapon.gameObject.SetActive(on);
        LHMWeapon.gameObject.SetActive(on);
        LLWeapon.gameObject.SetActive(on);
        RLWeapon.gameObject.SetActive(on);
        RHRWeapon.gameObject.SetActive(false);
        LHRWeapon?.gameObject.SetActive(false);
        if (on) {
            anima.TakeOutSword();
        }
    }

    public void RangedWeaponsOn() {
        if (RHRWeapon.gameObject.activeInHierarchy) { return; }
        RHMWeapon.gameObject.SetActive(false);
        LHMWeapon.gameObject.SetActive(false);
        LLWeapon.gameObject.SetActive(false);
        RLWeapon.gameObject.SetActive(false);
        RHRWeapon.gameObject.SetActive(true);
        LHRWeapon?.gameObject.SetActive(true);
        anima.TakeOutSword();
    }

    public void AnimEventReceiver(string message) {
        state.AnimEventReceiver(message);
    }

    //Mission Methods

    public void BroadcastNextStep() {
        Debug.Log("#Mission#"+transform.name + " Broadcasting Next Step");
        if (FindObjectOfType<MissionController>() == null) { return; }
        FindObjectOfType<MissionController>().SetAllHeroesToNextPOI();
    }

    public void FindNextStep() {
        if (FindObjectOfType<MissionController>() == null) { return; }
        Transform nextStep = FindObjectOfType<MissionController>().GetNextPOI();
        if (nextStep != null) {
            MissionMoveTo(nextStep);
        }
    }

    public void MissionMoveTo(Transform target) {
        Debug.Log("#Mission#Move to " + target.name + " by " + myCharacter.name);
        if (myCharacter.incapacitated) { Debug.Log("Can't Walk, Dead " + myCharacter.name); return; }
        state.TransitionTo(new SCMissionMoveTo() { target = target });
    }

    public void ActivateStep(MissionPOI poi) {
        print("#Mission#Activate " + poi.step.type.ToString());
         switch (poi.step.type) {
            case StepType.Lockpick:
               // state.TransitionTo(new MissionCharacterStateLockpick() { poi = poi });
                break;
            case StepType.Assassinate:
                //state.TransitionTo(new MissionCharacterStateAssassinate() { poi = poi, killTarget = poi.allPurposeTransforms[0] });
                break;
            case StepType.Hunt:
                state.TransitionTo(new SCHuntState() { poi = poi });
                break;
            case StepType.Camp:
               state.TransitionTo(new SCCampState() { poi = poi });
                break;
            case StepType.NavigateLand:
                state.TransitionTo(new SCNavigateLand() { poi = poi, badLocation = poi.allPurposeTransforms[0] });
                break;
            case StepType.Connection:
            case StepType.NegotiateBusiness:
            case StepType.Inspire:
                state.TransitionTo(new SCNegotiateState() { poi = poi, negotiateTarget = poi.allPurposeTransforms[1] });
                break;
            case StepType.Portal:
                poi.Resolve(true);
                break;
            case StepType.Gamble:
                //state.TransitionTo(new MissionCharacterStateGamble() { poi = poi, gambleTarget = poi.allPurposeTransforms[1] });
                break;
        }
        
    }

    public void AddMod(Mod thisMod) {
        Debug.Log("#MOD#AddMOD "+ thisMod.modAmount);
        switch (thisMod.modType) {
            case ModType.Speed:
                agent.speed += myCharacter.runspeed * .4f * thisMod.modAmount;
                break;
        }
        mods.Add(thisMod);
    }
    public void EndMod(Mod thisMod) {
        Debug.Log("#MOD#endMOD " + thisMod.modAmount);
        switch (thisMod.modType) {
            case ModType.Speed:
                agent.speed -= myCharacter.runspeed * .4f * thisMod.modAmount;
                break;
        }
    }
    public CombatTicket GetTargeted(StableCombatChar attacker) {
        return myCombatTickets.GetNextTicket(attacker);
    }
    public bool AcquireTarget(StableCombatChar target) {
        var a = target.GetTargeted(this);
        if (a != null) {
            if (myAttackTargetTicket != null) { ReleaseTarget(); }
            myAttackTargetTicket = a;
            myAttackTarget = target;
            return true;
        } else {
            return false;
        }
    }
    public void ReleaseTarget() {
        if (myAttackTargetTicket != null) {
            myAttackTargetTicket.attacker = null;
        }
        myAttackTargetTicket = new CombatTicket();
        myAttackTarget = null;
    }
    
    void OnDrawGizmos() {
#if UNITY_EDITOR
        if (debugState && state!=null) {
            Handles.Label(_t.position + new Vector3(0, 2, 0), state.GetType().ToString());
        }
#endif
    }
    

}
public enum ModType { Speed, DOT}
public enum Position { NA, LW, STR, STL, STC, RW, LM, LCM, MC, RCM, RM, DL, LDC, DC, RDC, DR, GK }
public enum TackleType { Tackle, Strip }
public enum CombatFocus { Melee, Ranged }
public enum PlayStyle { Play, Fight }
public enum CombatEngagementStatus { None, Aggressor, Defender }
public enum RunSpeed { VerySlow, Slow, Average, Fast, VeryFast, WorldClass}
[Flags]
public enum PassTargetLogic { None = 0, Nearest = 1, Farthest = 2, Rogue = 4, Open = 8, DeepBall = 16, BackwardOK = 32, NearGoal = 64, Wizard = 128 }
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

    public static bool IsForward(this Position pos) {
        return (pos == Position.LW || pos == Position.STR || pos == Position.STL || pos == Position.STC || pos == Position.RW);
    }
    public static StableCombatChar FindEnemyWithinRange(this StableCombatChar thisChar, float range) {
        List<StableCombatChar> enemies;
        if (!thisChar.fieldSport) {
            enemies = GameObject.FindObjectOfType<CombatController>().enemies;
        } else {
            enemies = thisChar.coach.otherTeamList;
        }

        foreach (var scc in enemies) {
            if (scc.isKnockedDown) {
                continue;
            }
            if (thisChar.Distance(scc) <= range) {
                return scc;
            }
        }
        return null;
    }
}

public interface CannotInterrupt {

}


static class Methods {
    public static void Inform(string parameter) {
        Console.WriteLine("Inform:parameter={0}", parameter);
    }
}
[System.Serializable]
public class CombatTicket {
    public StableCombatChar owner;
    public StableCombatChar attacker;
}
public static class CombatTicketHelper {
    public static CombatTicket GetNextTicket(this List<CombatTicket> thisList, StableCombatChar attacker) {
        foreach (var t in thisList) {
            if (t.attacker == null) {
                t.attacker = attacker;
                return t;
            }
        }
        return null;
    }
}
class Program {
    static void Main() {
        // Name of the method we want to call.
        string name = "Inform";

        // Call it with each of these parameters.
        string[] parameters = { "Sam", "Perls" };

        // Get MethodInfo.
        Type type = typeof(Methods);
        MethodInfo info = type.GetMethod(name);

        // Loop over parameters.
        foreach (string parameter in parameters) {
            info.Invoke(null, new object[] { parameter });
        }
    }
}