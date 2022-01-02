using CoverShooter;
using EnergyBarToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class MissionCharacter : MonoBehaviour, MissionCharacterStateOwner {

    public NavMeshAgent agent;
    public Animator anim;
    public Character character;
    public HealthBar healthBar;
    public Transform shoulderCam;
    public Transform rHand, lHand, rHandHolster, lHandHolster, rLeg, lLeg;
    public Transform target;
    public Move[] activeMoves = new Move[3];
    public int currentMoveIndex = 0;
    public Posture posture;
    public int team;
    public MissionController missionController;
    public float detectRange = 100;
    public float minAttackRange = 0;
    public float maxAttackRange = 50;

    //weapons
    public GameObject leftHandWeapon, rightHandWeapon, leftLegWeapon, rightLegWeapon;
    /// <summary>
    /// Interface Reqs
    /// </summary>
    /// 

    public MissionCharacterState state { get; set; }
    public MissionCharacter controller { get; set; }
    public bool generateRandom = false; //use this for an ad hoc instance that you create manually in a scene
    // Start is called before the first frame update
    void Start() {
        if (generateRandom) {
            Init(new Character() { toughness = 10, melee = Random.Range(0, 15), landNavigation = -100, strength = 10, parry = 20, health = 3, name = Names.Warrior[Random.Range(0, 10)] });
        }
        for (int i = 0; i < 3; i++) {
            if (character.activeMoves[i] == null) { character.activeMoves[i] = new Move() { moveType = MoveType.None }; }
            activeMoves[i] = character.activeMoves[i];
        }
    }

    public void SetCombatComponents(bool on) {
        Vector3 vec = new Vector3();
        vec = transform.position;
        Animator _animator = GetComponent<Animator>();
        RuntimeAnimatorController animControl = on ? Resources.Load<RuntimeAnimatorController>("CharacterAnimatorStable2") : Resources.Load<RuntimeAnimatorController>("Character");
        if (healthBar != null) {
            healthBar.Hide(!on);
        }
        _animator.runtimeAnimatorController = animControl;
        _animator.Rebind();
        transform.position = vec;
        _animator.SetFloat("BodyValue0", 1);
        _animator.SetFloat("CrouchToStand", 1);
    }

    public void Init(Character c, MissionCharacterState initialState = null) {
        Debug.Log("Initializing " + name);
        SetCombatComponents(false);
        controller = this;
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        anim.SetBool("IsDead", false);
        shoulderCam = transform.Find("ShoulderCam");
        character = c;
        MissionCharacterState initState;
        if (initialState == null) {
            initState = new MissionCharacterStateIdle();
        } else {
            initState = initialState;
        }
        initState.owner = this;
        this.state = initState;
        initState.thisChar = this;
        initState.EnterFrom(null);
        InitHealth();
        InitHealthBarAndVisuals();
        InitWeaponsAndArmor();
        transform.name = c.name;
        c.currentMissionCharacter = this;
        missionController = FindObjectOfType<MissionController>();
        anim.SetFloat("BodyValue0", 1);
        anim.SetFloat("CrouchToStand", 1);
    }
    bool healthBarInitialized = false;
    bool weaponsInited = false;
    public void InitHealth() {
        CharacterHealth health = GetComponent<CharacterHealth>();
        health.stamina = health.maxStamina = character.maxStamina;
        health.balance = health.maxBalance = character.maxBalance;
        health.mind = health.maxMind = character.maxMind;
        health.Health = health.MaxHealth = character.maxHealth;

    }
    public void InitWeaponsAndArmor() {
        print("Initializing Weapons and Armor for " + character.name);
        if (weaponsInited) { return; }
        weaponsInited = true;

        if (character.weapon == null || character.weapon.name == "") {
            Weapon startingWeaponSO = Resources.Load<Weapon>(character.startingWeapon);
            character.weapon = Instantiate(startingWeaponSO);
        }
        Weapon weaponBlueprint = character.weapon;
        GameObject weaponPrefab = Resources.Load<GameObject>(weaponBlueprint.prefabName);
        GameObject defaultFists = Resources.Load<GameObject>("Fists");
        rightHandWeapon = Instantiate<GameObject>(weaponPrefab, rHand);
        rightHandWeapon.transform.localPosition = Vector3.zero;
        rightHandWeapon.transform.localRotation = Quaternion.identity;
        rightHandWeapon.GetComponent<BaseWeapon>().InitWeapon(this, weaponBlueprint);

        GameObject leftHandPrefab = weaponBlueprint.dualWield ? weaponPrefab : defaultFists;
        leftHandWeapon = Instantiate<GameObject>(leftHandPrefab, lHand);
        leftHandWeapon.transform.localPosition = Vector3.zero;
        leftHandWeapon.transform.localRotation = Quaternion.identity;
        leftHandWeapon.GetComponent<BaseWeapon>().InitWeapon(this, weaponBlueprint);

        GameObject legWeaponPrefab = weaponBlueprint.usesLegs ? Resources.Load<GameObject>(weaponBlueprint.prefabNameLegs) : defaultFists;
        leftLegWeapon = Instantiate<GameObject>(legWeaponPrefab, lLeg);
        leftLegWeapon.transform.localPosition = Vector3.zero;
        leftLegWeapon.transform.localRotation = Quaternion.identity;
        leftLegWeapon.GetComponent<BaseWeapon>().InitWeapon(this, weaponBlueprint);

        rightLegWeapon = Instantiate<GameObject>(legWeaponPrefab, lLeg);
        rightLegWeapon.transform.localPosition = Vector3.zero;
        rightLegWeapon.transform.localRotation = Quaternion.identity;
        rightLegWeapon.GetComponent<BaseWeapon>().InitWeapon(this, weaponBlueprint);
    }
    public void InitHealthBarAndVisuals() {
        if (healthBarInitialized) {
            return;
        }
        print("Changing Mat if available");
        print(character.name + " " + character.mat);
        if (character.mat != null) {
            int numChildren = transform.childCount;
            print("ChildCount: " + numChildren);
            for (int i = 0; i < numChildren; i++) {

                print(transform.GetChild(i).name);
                SkinnedMeshRenderer smr = transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (smr != null) { smr.material = character.mat; }
            }
        }

        print("Faking Health, TODO: Change");

        healthBarInitialized = true;
        GameObject bar = Instantiate(Resources.Load<GameObject>("HealthBar"), Helper.GetMainCanvas().transform);
        bar.name = character.name + " Health";
        healthBar = bar.GetComponent<HealthBar>();
        healthBar.gameObject.GetComponent<EnergyBarFollowObject>().followObject = gameObject;
        UpdateHealthBar();
        healthBar.Hide(true);
    }
    public bool debugState;
    void Update() {
        if (state != null) {
            state.Update();
        }
        anim.SetFloat("BodyValue0", 1);
        anim.SetFloat("CrouchToStand", 1);
    }
#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (debugState) {
            Handles.Label(transform.position, state.GetType().ToString());
        }
    }
#endif
    public void ControlCam(bool myControl = true, float time = 0) {
        print("Control Cam Called " + myControl);
        Camera.main.GetComponent<CameraController>().SetControl(!myControl, time);
        if (myControl) {
            Camera.main.transform.position = shoulderCam.position;
            Camera.main.transform.rotation = shoulderCam.rotation;
        }
    }
    public void UpdateHealthBar() {
        healthBar.SetDots(character.health);
        if (character.armor == null) { character.armor = new Armor(); }
        healthBar.SetArmor(character.armor.condition * .01f);
        healthBar.SetMeters(GetComponent<CharacterHealth>());
    }
    public void Idle() {
        state.TransitionTo(new MissionCharacterStateIdle());
    }
    public void IdleDontAct() {
        state.TransitionTo(new MissionCharacterStateIdleDontAct());
    }
    public void Walk(Transform target) {
        if (character.incapacitated) { Debug.Log("Can't Walk, Dead " + character.name); return; }
        state.TransitionTo(new MissionCharacterStateWalkTo() { target = target });
    }

    public void ChangePostureAttack() {
        print("Changeing Posture to Attack with Target: " + target.name);
        state.TransitionTo(new MissionCharacterStatePostureAttack());
    }
    public void StopAndFaceTarget() {
        agent.isStopped = true;
        transform.LookAt(target);
    }

    public void FindNextStep() {
        if (FindObjectOfType<MissionController>() == null) { return; }
        Transform nextStep = FindObjectOfType<MissionController>().GetNextPOI();
        if (nextStep != null) {
            Walk(nextStep);
        }
    }
    public void BroadcastNextStep() {
        print(transform.name + " Broadcasting Next Step");
        if (FindObjectOfType<MissionController>() == null) { return; }
        FindObjectOfType<MissionController>().SetAllHeroesToNextPOI();
    }
    public void BroadcastWalkTo(Transform walkTo, bool includeActiveChar = false) {
        if (FindObjectOfType<MissionController>() == null) { return; }
        FindObjectOfType<MissionController>().SetAllHeroesWalkTo(walkTo, includeActiveChar, this);
    }
    public float DistanceToTarget() {
        if (target == null) { Debug.Log("target null");  return 0; }
        return Vector3.Distance(transform.position, target.position);
    }
    public void RunAtTarget() {
        print("RunAtTarget");
        agent.SetDestination(target.position);
        agent.isStopped = false;
    }

    
    public void ActivateStep(MissionPOI poi) {
        switch (poi.step.type) {
            case StepType.Lockpick:
                state.TransitionTo(new MissionCharacterStateLockpick() { poi = poi });
                break;
            case StepType.Assassinate:
                state.TransitionTo(new MissionCharacterStateAssassinate() { poi = poi, killTarget = poi.allPurposeTransforms[0] });
                break;
            case StepType.Hunt:
                state.TransitionTo(new MissionCharacterStateHunt() { poi = poi});
                break;
            case StepType.Camp:
                state.TransitionTo(new MissionCharacterStateCamp() {poi = poi });
                break;
            case StepType.NavigateLand:
                state.TransitionTo(new MissionCharacterStateNavigateLand() { poi = poi, badLocation = poi.allPurposeTransforms[0] });
                break;
            case StepType.Connection:
            case StepType.NegotiateBusiness:
            case StepType.Inspire:
                state.TransitionTo(new MissionCharacterStateNegotiate() {poi = poi, negotiateTarget = poi.allPurposeTransforms[1]});
                break;
            case StepType.Portal:
                poi.Resolve(true);
                break;
            case StepType.Gamble:
                state.TransitionTo(new MissionCharacterStateGamble() { poi = poi, gambleTarget = poi.allPurposeTransforms[1] });
                break;
        }
    }

    public void Die() {
        anim.SetBool("IsDead", true);
    }

    public void AnimEvent(string message) {
        print("Anim " + message);
        state.AnimEventReceiver(message);
    }

    public void AnimSound (string soundName) {
        print(soundName + " Sound");
        GameObject.FindObjectOfType<AudioSource>().PlayOneShot(Resources.Load<AudioClip>(soundName));
    }

}


public enum Posture { Idle, Attack, Retreat, PursueBall, PursueBallCarrier, RunToGoal, DefendLocation, FollowLeader }