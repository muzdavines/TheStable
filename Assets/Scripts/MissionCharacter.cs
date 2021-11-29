using CoverShooter;
using EnergyBarToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]


public class MissionCharacter : MonoBehaviour, MissionCharacterStateOwner
{
    
    NavMeshAgent agent;
    public Animator anim;
    public Character character;
    public HealthBar healthBar;
    public CombatTestController.CombatCharacter combatChar;
    public Transform shoulderCam;
    public Transform rHand, lHand, rHandHolster, lHandHolster, rLeg, lLeg;
    public Move[] activeMoves = new Move[3];
    public int currentMoveIndex = 0;
    /// <summary>
    /// Interface Reqs
    /// </summary>
    /// 

    public MissionCharacterState state { get; set; }
    public MissionCharacter controller { get; set; }
    public bool generateRandom = false; //use this for an ad hoc instance that you create manually in a scene
    // Start is called before the first frame update
    void Start()
    {
        if (generateRandom) {
            Init(new Character() { toughness = 10, melee = Random.Range(0, 15), landNavigation = -100, strength = 10, parry = 20, health = 3, name = Names.Warrior[Random.Range(0, 10)] });
        }
        for (int i=0; i < 3; i++) {
            if (character.activeMoves[i] == null) { character.activeMoves[i] = new Move() { moveType = MoveType.None }; } 
            activeMoves[i] = character.activeMoves[i];
        }
    }

    public void SetCombatComponents(bool on) {
        Vector3 vec = new Vector3();
        vec = transform.position;
        Animator _animator = GetComponent<Animator>();
        RuntimeAnimatorController animControl = on ? Resources.Load<RuntimeAnimatorController>("CharacterAnimatorStable") : Resources.Load<RuntimeAnimatorController>("Character");
        if (healthBar != null) {
            healthBar.Hide(!on);
        }
        _animator.runtimeAnimatorController = animControl;
        _animator.Rebind();
        
        GetComponent<NavMeshAgent>().enabled = !on;
        GetComponent<FighterBrain>().enabled = on;
        GetComponent<AIMovement>().enabled = on;
        if (GetComponent<AIAssault>()) {
            GetComponent<AIAssault>().enabled = on;
        }
        GetComponent<CharacterMotor>().enabled = on;
        GetComponent<Rigidbody>().isKinematic = !on;
        transform.position = vec;
    }

    public void Init(Character c) {
        Debug.Log("Initializing " + name);
        SetCombatComponents(false);
        controller = this;
        anim = GetComponent<Animator>();
        shoulderCam = transform.Find("ShoulderCam");
        character = c;
        MissionCharacterState initState = new MissionCharacterStateIdle();
        initState.owner = this;
        this.state = initState;
        initState.EnterFrom(null);
        InitHealth();
        InitHealthBarAndVisuals();
        InitWeaponsAndArmor();
        transform.name = c.name;
        c.currentMissionCharacter = this;
        
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
        CharacterMotor motor = GetComponent<CharacterMotor>();
        AIAssault assault = GetComponent<AIAssault>();
        CharacterInventory inv = GetComponent<CharacterInventory>();
        AIFire fire = GetComponent<AIFire>();

        if (character.weapon == null || character.weapon.name == "") {
            Weapon startingWeaponSO = Resources.Load<Weapon>(character.startingWeapon);
            character.weapon = Instantiate(startingWeaponSO);
        }
        Weapon weaponBlueprint = character.weapon;
        GameObject weaponPrefab = Resources.Load<GameObject>(weaponBlueprint.prefabName);
        GameObject defaultFists = Resources.Load<GameObject>("Fists");
        GameObject rHandWeapon = Instantiate<GameObject>(weaponPrefab, rHand);
        rHandWeapon.transform.localPosition = Vector3.zero;
        rHandWeapon.transform.localRotation = Quaternion.identity;
        rHandWeapon.GetComponent<BaseWeapon>().Damage = weaponBlueprint.damage;
        rHandWeapon.GetComponent<BaseWeapon>().InitWeapon(this);
        motor.Weapon.RightItem = rHandWeapon;
        motor.Weapon.IsDualWielding = weaponBlueprint.dualWield;
        motor.Weapon.IsHeavy = weaponBlueprint.isHeavy;
        inv.Weapons = new WeaponDescription[1];
        inv.Weapons[0] = new WeaponDescription() { IsDualWielding = weaponBlueprint.dualWield, IsHeavy = weaponBlueprint.isHeavy, RightItem = rHandWeapon };
        print("Need assault settings here.");
        fire.AutoFindType = rHandWeapon.GetComponent<BaseWeapon>().Type;
        GameObject leftHandPrefab = weaponBlueprint.dualWield ? weaponPrefab : defaultFists;
        GameObject lHandWeapon = Instantiate<GameObject>(leftHandPrefab, lHand);
        lHandWeapon.transform.localPosition = Vector3.zero;
        lHandWeapon.transform.localRotation = Quaternion.identity;
        motor.Weapon.LeftItem = lHandWeapon;
        inv.Weapons[0].LeftItem = lHandWeapon;
        lHandWeapon.GetComponent<BaseWeapon>().Damage = weaponBlueprint.dualWield ? weaponBlueprint.damage : 5;

        GameObject legWeaponPrefab = weaponBlueprint.usesLegs ? Resources.Load<GameObject>(weaponBlueprint.prefabNameLegs) : defaultFists;
        GameObject lLegWeapon = Instantiate<GameObject>(legWeaponPrefab, lLeg);
        lLegWeapon.transform.localPosition = Vector3.zero;
        lLegWeapon.transform.localRotation = Quaternion.identity;
        motor.Weapon.LeftLegItem = lLegWeapon;
        inv.Weapons[0].LeftItem = lLegWeapon;
        lLegWeapon.GetComponent<BaseWeapon>().Damage = weaponBlueprint.damage;

        GameObject rLegWeapon = Instantiate<GameObject>(legWeaponPrefab, rLeg);
        rLegWeapon.transform.localPosition = Vector3.zero;
        rLegWeapon.transform.localRotation = Quaternion.identity;
        motor.Weapon.RightLegItem = rLegWeapon;
        inv.Weapons[0].RightItem = rLegWeapon;
        rLegWeapon.GetComponent<BaseWeapon>().Damage = weaponBlueprint.damage;
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
    // Update is called once per frame
    void Update()
    {
     if (state != null) {
            state.Update();
        }   
    }

    public void ControlCam(bool myControl = true, float time = 0) {
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
    public void DoCombatAnimation (CombatTestController.BattleEntry battleEntry, bool attacker) {
        print("#DoCombatAnimation# " + name + "  " + battleEntry.result.ToString() + "  " + attacker);
        if (!attacker && battleEntry.didLandKillingBlow) {
            CombatDeath();
            return;
        }
        switch (battleEntry.result) {
            case CombatTestController.BattleEntry.Result.MeleeHit:
                if (attacker) {
                    MeleeAttack(battleEntry.defender.go.transform, battleEntry.healthDamage);
                } else { DefendMeleeAttack(); }
                break;
            case CombatTestController.BattleEntry.Result.MeleeParry:
                if (attacker) {
                    MeleeAttack(battleEntry.defender.go.transform, battleEntry.healthDamage);
                }
                else { DefendMeleeAttack(1); }
                break;
            case CombatTestController.BattleEntry.Result.MeleeMiss:
                if (attacker) {
                    MeleeAttack(battleEntry.defender.go.transform, battleEntry.healthDamage);
                }
                else { DefendMeleeAttack(2); }
                break;
            default:
                if (attacker) {
                    MeleeAttack(battleEntry.defender.go.transform);
                }
                else { DefendMeleeAttack(battleEntry.result == CombatTestController.BattleEntry.Result.MeleeParry ? 1 : 0); }
                break;
        }
    }





    public void CombatDeath() {
        state.TransitionTo(new MissionCharacterStateDeath());
    }
    public void MeleeAttack(Transform target, int dotDamage = 0) {
        state.TransitionTo(new MissionCharacterStateMeleeAttack() { attackTarget = target, dotDamage = dotDamage });
    }
    public void DefendMeleeAttack(float defenseType = 0) {
        state.TransitionTo(new MissionCharacterStateDefendMeleeAttack() { defenseType = defenseType });
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
        }
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
