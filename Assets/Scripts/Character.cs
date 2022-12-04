using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StableMasterType { Warrior, Wizard, Rogue};
public enum Gender { Male, Female }
public enum Race { Human, Elf }
public enum SkinColor { White, Brown, Black, Elf }
public enum Elements { Yes, No }
public enum HeadCovering { HeadCoverings_Base_Hair, HeadCoverings_No_FacialHair, HeadCoverings_No_Hair }
public enum FacialHair { Yes, No }
public enum CharacterAttribute { shooting, passing, tackling, carrying, all}
[CreateAssetMenu]
[System.Serializable]
public class Character : Living {
    //Physical Attributes
    public int strength = 5;
    public int agility = 5;

    public int dexterity = 5;

    public int carrying = 5;

    public int tackling = 5;

    public int runspeed = 5;
    public int shooting = 5;
    public int passing = 5;
    public int catching = 5;
    public int melee = 10;
    public int ranged = 10;
    public int magic = 10;
    public int xp = 0;

    public int health; //dots - 0 = death

    //need some enums for armor type so we can have bonuses and penalties - dodge penalty for plate, etc.
    public CombatFocus combatFocus;

    //new combat system
    public int maxStamina = 5;
    public int maxBalance = 5;
    public int maxMind = 5;
    public int maxHealth = 1; //health defined above

    public List<Move> knownMoves = new List<Move>();
    public List<MoveSave> knownMovesSave = new List<MoveSave>();
    public List<Move> activeMeleeMoves = new List<Move>();
    public List<Move> activeRangedMoves = new List<Move>();
    public List<string> startingSpecialMoves = new List<string>();
    public List<Trait> startingTraits = new List<Trait>();
    public List<Trait> activeTraits = new List<Trait>();
    public List<SpecialMove> activeSpecialMoves = new List<SpecialMove>();
    public SportStats seasonStats = new SportStats();
    public SportStats careerStats = new SportStats();
    public QuestStats careerQuestStats = new QuestStats();
    public QuestStats thisQuestStats = new QuestStats();

    public Gender gender;
    public Race race = Race.Human;
    public SkinColor skinColor;
    public Elements elements;
    public HeadCovering headCovering;
    public FacialHair facialHair;
    [Range(0, 37)]
    public int hair;
    public CharacterGearSet myGearSet;
    public bool HasMove(string m) {
        Debug.Log("#TODO#HasMove called on Character. Need to Change This");
        foreach (Move move in knownMoves) {

            if (move.name.Contains(m)) { return true; }
        }
        return false; // knownMoves.Contains(m);
    }
    //Inventory
    public string startingArmor;
    public string startingMeleeWeapon = "FistsSO";
    public string startingRangedWeapon = "BowSO";
    public Armor armor;
    public Weapon meleeWeapon;
    public Weapon rangedWeapon;
    //public List<Item> inventory;
    public Weapon GetDefaultMeleeWeapon() {
        return Instantiate(Resources.Load<Weapon>(startingMeleeWeapon));
    }
    public Weapon GetDefaultRangedWeapon() {
        return Instantiate(Resources.Load<Weapon>(startingRangedWeapon));
    }
    //Visuals
    /// <summary>
    /// If blank, default mat
    /// </summary>
    public Material mat;

    //NEEDED: List of accolades, stats, and other historical information like missions run, crippling injuries, etc
    [SerializeField]
    public EmploymentContract contract = new EmploymentContract();

    [SerializeField]
    public Training currentTraining;

    public Game.GameDate returnDate = new Game.GameDate();

    public bool activeForNextMission;
    public bool activeInLineup;
    public Position currentPosition;
    public bool incapacitated;
    public string modelName;
    public int modelNum;
    public int skinNum = -1;

    public GameObject currentObject;
    public static string GenerateName(Archetype archetype) {

        switch (archetype) {
            case Archetype.Rogue:
                return Names.Rogue[Random.Range(0, Names.Rogue.Length)];
                break;
            case Archetype.Warrior:
                return Names.Warrior[Random.Range(0, Names.Warrior.Length)];
                break;
            case Archetype.Wizard:
                return Names.Wizard[Random.Range(0, Names.Wizard.Length)];
                break;
            default:
                return Names.Rogue[Random.Range(0, Names.Rogue.Length)];
                break;
        }
    }

    public int RandDist(float min, float max) {
        int roll = UnityEngine.Random.Range(0, 100);
        float interval = (max - min) / 6;
        int[] breaks = { 5, 27, 18, 18, 27, 5 };
        float[] array = new float[100];
        int count = 0;
        for (int x = 0; x < 6; x++) {
            for (int z = 0; z < breaks[x]; z++) {
                array[count++] = Mathf.Round(min + (x * interval));
            }
        }
        return (int)array[roll];
    }
    public void GenerateCharacter(Archetype thisArchetype, int level = 1) {
        int[] shootingRange = new int[2];
        int[] passingRange = new int[2];
        int[] tackleRange = new int[2];
        int[] carryingRange = new int[2];
        int[] meleeRange = new int[2];
        int[] rangedRange = new int[2];
        int[] magicRange = new int[2];
        int[] speedRange = new int[2];
        int[] dexRange = new int[2];
        int[] strRange = new int[2];
        int[] agiRange = new int[2];
        hair = Random.Range(0, 38);
        var leftjab = Resources.Load<Move>("LeftJab");
        var rightjab = Resources.Load<Move>("RightJab");
        knownMoves.Add(leftjab);
        knownMoves.Add(rightjab);
        activeMeleeMoves.Add(leftjab);
        activeMeleeMoves.Add(leftjab);
        activeMeleeMoves.Add(rightjab);
        switch (thisArchetype) {
            case Archetype.Striker:
                shootingRange[0] = 10; shootingRange[1] = 20;
                passingRange[0] = 4; passingRange[1] = 8;
                tackleRange[0] = 2; tackleRange[1] = 4;
                carryingRange[0] = 4; carryingRange[1] = 8;
                meleeRange[0] = 2; meleeRange[1] = 4;
                rangedRange[0] = 8; rangedRange[1] = 16;
                magicRange[0] = 10; magicRange[1] = 20;
                speedRange[0] = 9; speedRange[1] = 18;
                dexRange[0] = 10; dexRange[1] = 20;
                strRange[0] = 2; strRange[1] = 4;
                agiRange[0] = 4; agiRange[1] = 8;
                if (UnityEngine.Random.Range(0, 1f) >= .99f) {
                    shootingRange[1] = 40;
                    Debug.Log("#CreateHero#Striker Crit!");
                }
                knownMoves.Add(Resources.Load<Move>("OneTimerKick"));
                modelNum = 0;
                modelName = "SCUnit";
                break;
            case Archetype.Winger:
                shootingRange[0] = 8; shootingRange[1] = 16;
                passingRange[0] = 10; passingRange[1] = 20;
                tackleRange[0] = 2; tackleRange[1] = 4;
                carryingRange[0] = 8; carryingRange[1] = 16;
                meleeRange[0] = 4; meleeRange[1] = 8;
                rangedRange[0] = 4; rangedRange[1] = 8;
                magicRange[0] = 4; magicRange[1] = 8;
                speedRange[0] = 9; speedRange[1] = 12;
                dexRange[0] = 8; dexRange[1] = 16;
                strRange[0] = 4; strRange[1] = 8;
                agiRange[0] = 4; agiRange[1] = 8;
                modelNum = 1;
                modelName = "SCUnit";
                knownMoves.Add(Resources.Load<Move>("OneTimerKick"));
                break;
            case Archetype.Midfielder:
                shootingRange[0] = 4; shootingRange[1] = 8;
                passingRange[0] = 8; passingRange[1] = 16;
                tackleRange[0] = 8; tackleRange[1] = 16;
                carryingRange[0] = 4; carryingRange[1] = 8;
                meleeRange[0] = 8; meleeRange[1] = 16;
                rangedRange[0] = 4; rangedRange[1] = 8;
                magicRange[0] = 4; magicRange[1] = 8;
                speedRange[0] = 9; speedRange[1] = 11;
                dexRange[0] = 8; dexRange[1] = 16;
                strRange[0] = 8; strRange[1] = 16;
                agiRange[0] = 8; agiRange[1] = 16;
                modelNum = 2;
                modelName = "SCUnit";
                break;
            case Archetype.Defender:
                shootingRange[0] = 2; shootingRange[1] = 4;
                passingRange[0] = 4; passingRange[1] = 8;
                tackleRange[0] = 10; tackleRange[1] = 20;
                carryingRange[0] = 2; carryingRange[1] = 4;
                meleeRange[0] = 10; meleeRange[1] = 20;
                rangedRange[0] = 4; rangedRange[1] = 8;
                magicRange[0] = 8; magicRange[1] = 16;
                speedRange[0] = 9; speedRange[1] = 10;
                dexRange[0] = 4; dexRange[1] = 8;
                strRange[0] = 10; strRange[1] = 20;
                agiRange[0] = 2; agiRange[1] = 4;
                modelNum = 0;
                modelName = "SCUnit2";
                break;
            case Archetype.Goalkeeper:
                shootingRange[0] = 2; shootingRange[1] = 4;
                passingRange[0] = 4; passingRange[1] = 8;
                tackleRange[0] = 10; tackleRange[1] = 20;
                carryingRange[0] = 2; carryingRange[1] = 4;
                meleeRange[0] = 10; meleeRange[1] = 20;
                rangedRange[0] = 4; rangedRange[1] = 8;
                magicRange[0] = 8; magicRange[1] = 16;
                speedRange[0] = 11; speedRange[1] = 13;
                dexRange[0] = 4; dexRange[1] = 8;
                strRange[0] = 10; strRange[1] = 20;
                agiRange[0] = 2; agiRange[1] = 4;
                modelNum = 2;
                modelName = "SCUnit3";
                break;
        }
        shooting = RandDist(shootingRange[0], shootingRange[1]) + ((level - 1) * 20);
        passing = RandDist(passingRange[0], passingRange[1]) + ((level - 1) * 20);
        tackling = RandDist(tackleRange[0], tackleRange[1]) + ((level - 1) * 20);
        carrying = RandDist(carryingRange[0], carryingRange[1]) + ((level - 1) * 20);
        melee = RandDist(meleeRange[0], meleeRange[1]) + ((level - 1) * 20);
        ranged = RandDist(rangedRange[0], rangedRange[1]) + ((level - 1) * 20);
        magic = RandDist(magicRange[0], magicRange[1]) + ((level - 1) * 20);
        runspeed = RandDist(speedRange[0], speedRange[1]) + ((level - 1) * 20);
        dexterity = RandDist(dexRange[0], dexRange[1]) + ((level - 1) * 20);
        strength = RandDist(strRange[0], strRange[1]) + ((level - 1) * 20);
        agility = RandDist(agiRange[0], agiRange[1]) + ((level - 1) * 20);
        maxHealth = 5;
        maxMind = 5;
        maxStamina = 5;
        maxBalance = 5;
        startingMeleeWeapon = "FistsSO";
        startingRangedWeapon = "BowSO";
        archetype = thisArchetype;
        gender = Gender.Male;
        if (GetPercent(50)) {
            gender = Gender.Female;
        }
        elements = Elements.Yes;
        race = Race.Human;
        int skinColorRoll = UnityEngine.Random.Range(0, 100);
        skinColor = SkinColor.White;
        if (skinColorRoll > 33) {
            skinColor = SkinColor.Brown;
        }
        if (skinColorRoll > 66) {
            skinColor = SkinColor.Black;
        }
        facialHair = FacialHair.No;
        if (gender == Gender.Male) {
            if (GetPercent(50)) {
                facialHair = FacialHair.Yes;
            }
        }

        switch (archetype) {
            case Archetype.Goalkeeper:
                shooting = 6;
                passing = 12;
                tackling = 25;
                carrying = 5;
                catching = 15;
                melee = 20;
                ranged = 10;
                magic = 5;
                runspeed = 13;
                dexterity = 10;
                strength = 20;
                agility = 10;
                maxHealth = 3;
                maxMind = 15;
                maxStamina = 25;
                maxBalance = 15;
                modelName = "SCUnit3";
                modelNum = 0;
                break;
            case Archetype.Warrior:
                shooting = 15;
                passing = 15;
                tackling = 25;
                carrying = 5;
                catching = 15;
                melee = 20;
                ranged = 10;
                magic = 5;
                runspeed = 13;
                dexterity = 10;
                strength = 20;
                agility = 10;
                maxHealth = 3;
                maxMind = 15;
                maxStamina = 25;
                maxBalance = 15;
                startingSpecialMoves.Add("ShoulderBarge");
                startingSpecialMoves.Add("PowerSlam");
                startingSpecialMoves.Add("ClosingSpeed");
                startingMeleeWeapon = "LongswordSO";
                knownMoves.Add(Resources.Load<Move>("Sword Hit Left"));
                knownMoves.Add(Resources.Load<Move>("Sword Hit Right"));
                knownMoves.Add(Resources.Load<Move>("SwordOverheadHack"));
                activeMeleeMoves = new List<Move>();
                activeMeleeMoves.Add(Resources.Load<Move>("Sword Hit Left"));
                activeMeleeMoves.Add(Resources.Load<Move>("Sword Hit Right"));
                activeMeleeMoves.Add(Resources.Load<Move>("SwordOverheadHack"));

                modelName = "SCUnit3";
                
                modelNum = 0;

                break;
            case Archetype.Rogue:
                shooting = 25;
                passing = 5;
                tackling = 5;
                carrying = 25;
                catching = 15;
                melee = 10;
                ranged = 10;
                magic = 5;
                runspeed = 16;
                dexterity = 10;
                strength = 5;
                agility = 20;
                maxHealth = 2;
                maxMind = 5;
                maxStamina = 15;
                maxBalance = 20;
                startingSpecialMoves.Add("Backstab");
                startingSpecialMoves.Add("Flechettes");
                knownMoves.Add(Resources.Load<Move>("DaggerLeft"));
                knownMoves.Add(Resources.Load<Move>("DaggerRight"));
                knownMoves.Add(Resources.Load<Move>("DaggerDualThrust"));
                activeMeleeMoves = new List<Move>();
                activeMeleeMoves.Add(Resources.Load<Move>("DaggerLeft"));
                activeMeleeMoves.Add(Resources.Load<Move>("DaggerRight"));
                activeMeleeMoves.Add(Resources.Load<Move>("DaggerDualThrust"));
                startingMeleeWeapon = "DaggerSO";
                knownMoves.Add(Resources.Load<Move>("OneTimerKick"));
                modelName = "SCUnit3";
                modelNum = 2;
                
                break;
            case Archetype.Wizard:
                shooting = 5;
                passing = 25;
                tackling = 15;
                carrying = 10;
                catching = 20;
                melee = 5;
                ranged = 5;
                magic = 20;
                runspeed = 10;
                dexterity = 20;
                strength = 5;
                agility = 10;
                maxHealth = 1;
                maxMind = 25;
                maxStamina = 5;
                maxBalance = 15;
                startingSpecialMoves.Add("FlameCircle");
                startingSpecialMoves.Add("SummonFireGolem");
                startingSpecialMoves.Add("DeepBall");
                startingRangedWeapon = "MageGlovesSO";
                knownMoves.Add(Resources.Load<Move>("Fireball"));
                for (int i = 0; i < 3; i++) {
                    activeRangedMoves.Add(Resources.Load<Move>("Fireball"));
                }
                combatFocus = CombatFocus.Ranged;
                modelName = "SCUnit3";
                modelNum = 1;
                
                break;
            case Archetype.Amateur:
                shooting = 5;
                passing = 5;
                tackling = 5;
                carrying = 5;
                catching = 5;
                melee = 1;
                ranged = 1;
                magic = 1;
                runspeed = 7;
                dexterity = 5;
                strength = 5;
                agility = 5;
                maxHealth = 1;
                maxMind = 5;
                maxStamina = 5;
                maxBalance = 5;
                modelName = "SCUnit3";
                modelNum = 1;
                
                break;
        }
       // myGearSet = Resources.Load<CharacterGearSet>("GearSets/" + archetype.ToString());
        seasonStats = new SportStats();
        careerStats = new SportStats();
        name = Character.GenerateName(archetype);
        Init();
    }

    public void ModifyCharacter() {
        shooting += Random.Range(-5, 6);
        passing += Random.Range(-5, 6);
        tackling += Random.Range(-5, 6);
        carrying += Random.Range(-5, 6);
        runspeed += Random.Range(-3, 4);
    }

    public enum Archetype { Striker, Winger, Midfielder, Defender, Goalkeeper, Warrior, Rogue, Wizard, Swashbuckler, Assassin, Thief, Thug, Enforcer, Charlatan, DarkWizard, LightWizard, ImperialWizard, VoidWizard, ExiledWizard, HolyWizard, Soldier, Mercenary, DarkKnight, Paladin, Champion, Marauder, Amateur }
    public Archetype archetype;
    public UpgradeModifier mod;
    public void Awake() {

    }
    public void Start() {

    }
    public Character Init() {
        Armor startingArmorSO = Resources.Load<Armor>(startingArmor);
        Weapon startingMeleeWeaponSO = Resources.Load<Weapon>(startingMeleeWeapon);
        Weapon startingRangedWeaponSO = Resources.Load<Weapon>(startingRangedWeapon);
        mod = Resources.Load<UpgradeModifier>(archetype + "Upgrade");
        returnDate = new Game.GameDate();
        if (startingArmorSO == null) { armor = new Armor(); } else { armor = Instantiate(startingArmorSO); }
        if (startingMeleeWeaponSO == null) { Debug.Log("Character new melee weapon " + name); meleeWeapon = new Weapon(); } else { Debug.Log("Char inst weapon " + name); meleeWeapon = Instantiate(startingMeleeWeaponSO); }
        if (startingRangedWeaponSO == null) { Debug.Log("Character new ranged weapon " + name); rangedWeapon = new Weapon(); } else { Debug.Log("Char inst ranged weapon " + name); rangedWeapon = Instantiate(startingRangedWeaponSO); }
        if (currentTraining == null) { currentTraining = new Training(); }
        activeSpecialMoves = new List<SpecialMove>();
        foreach (string specialMove in startingSpecialMoves) {
            Type myType = Type.GetType(specialMove);
            Debug.Log("#CharInit#Special: " + specialMove);
            SpecialMove myObj = (SpecialMove)Activator.CreateInstance(myType);
            activeSpecialMoves.Add(myObj);
        }
        activeTraits = new List<Trait>();
        foreach (Trait t in startingTraits) {
            Trait tempTrait = Instantiate(t);
            t.level = 1;
            activeTraits.Add(tempTrait);
        }
        //myGearSet = Resources.Load<CharacterGearSet>("GearSets/" + archetype.ToString());
        return this;

    }

    public void StartTraining(Training t) {
        currentTraining = t;
        currentTraining.dateToTrain = Helper.Today().Add(currentTraining.duration);
        returnDate = currentTraining.dateToTrain;
    }
    public bool ProcessTraining() {
        if (Helper.Today().IsOnOrAfter(currentTraining.dateToTrain, true)) {
            switch (currentTraining.type) {
                case Training.Type.Attribute:
                    Debug.Log("Raise Attribute " + currentTraining.training);
                    int currentValue = (int)typeof(Character).GetField(currentTraining.training).GetValue(this);
                    typeof(Character).GetField(currentTraining.training).SetValue(this, currentValue + currentTraining.amount);
                    FindObjectOfType<DailyPopup>().Popup(name + " completed training: " + currentTraining.training.Title());
                    //raise the attribute
                    //add the skill
                    return true; //return true to remove this training as it is complete        
                    break;
                case Training.Type.Skill:
                    Debug.Log("Add Skill " + currentTraining.moveToTrain.name);
                    knownMoves.Add(currentTraining.moveToTrain);
                    FindObjectOfType<DailyPopup>().Popup(name + " completed training: " + currentTraining.moveToTrain.name.Title());
                    return true;
                    break;
            }


        }

        return false;
    }

    public void AddXP(int amount) {
        xp += amount;
    }

    bool GetPercent(int pct) {
        bool p = false;
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll <= pct) {
            p = true;
        }
        return p;
    }
    public Trait GetBestTrait(StepType step) {
        int bestScore = 0;
        Trait bestTrait = new Trait() { level = 0 };
        foreach (var t in activeTraits) {
            if (t.usableFor.Contains(step)) {
                if (t.level > bestScore) {
                    bestScore = t.level;
                    bestTrait = t;
                }
            }
        }
        return bestTrait;
    }
    public int UpgradeCost(CharacterAttribute attribute, int current = 0) {
        switch (attribute) {
            case CharacterAttribute.shooting:
                return (int)((shooting + current) * 10 * mod.shooting);
            case CharacterAttribute.passing:
                return (int)((passing + current) * 10 * mod.passing);
            case CharacterAttribute.carrying:
                return (int)((carrying + current) * 10 * mod.carrying);
            case CharacterAttribute.tackling:
                return (int)((tackling + current) * 10 * mod.tackling);
        }
        return 0;

    }
    public bool ReadyToPromote() {
        if (mod.shootingReq == -1) { return false; }
        if (shooting < mod.shootingReq) {
            return false;
        }
        if (passing < mod.passingReq) {
            return false;
        }
        if (carrying < mod.carryingReq) {
            return false;
        }
        if (tackling < mod.tacklingReq) {
            return false;
        }
        return true;
    }
    public string GetPromoteText() {
        if (mod.shootingReq == -1) {
            return "Highest Class Reached.";
        }
        string s = "Hero needs\n";
        if (mod.shooting > 0) {
            var p = mod.shootingReq - shooting;
            s += p > 0 ? p + " more points of shooting\n" : "";
        }
        if (mod.passingReq > 0) {
            var p = mod.passingReq - passing;
            s += p > 0 ? p + " more points of passing\n" : "";
        }

        if (mod.tackling > 0) {
            var p = mod.tacklingReq - tackling;
            s += p > 0 ? p + " more points of tackling\n" : "";
        }
        if (mod.carrying > 0) {
            var p = mod.carryingReq - carrying;
            s += p > 0 ? p + " more points of carrying\n" : "";
        }
        s += "to be promoted.";
        return s;
    }

    public void Promote(Archetype newArchetype) {
        List<string> specialsToAdd = new List<string>();
        switch (newArchetype) {
            case Archetype.Assassin:
                shooting += 10;
                tackling -= 10;
                runspeed += 1;
                maxHealth += 1;
                maxMind += 35;
                maxStamina += 25;
                maxBalance += 35;
                specialsToAdd.Add("Assassinate");
                modelName = "SCUnit3";
                modelNum = 0;
                break;
            case Archetype.Thief:
                modelNum = 0;
                shooting += 10;
                tackling -= 10;
                runspeed += 1;
                maxMind += 25;
                maxStamina += 15;
                maxBalance += 35;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("Knife1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("Knife2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("Knife3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("KnifeSO"));
                specialsToAdd.Add("BolaThrow");
                name += " the " + Names.RogueNick[Random.Range(0, Names.RogueNick.Length)];
                break;
            case Archetype.Thug:
                shooting -= 10;
                tackling += 10;
                runspeed -= 1;
                maxMind += 15;
                maxStamina += 35;
                maxBalance += 25;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("SpikedBat1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("SpikedBat2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("SpikedBat3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("SpikedBatSO"));

                specialsToAdd.Add("Kneecapper");
                name += " the " + Names.RogueNick[Random.Range(0, Names.RogueNick.Length)];
                break;
            case Archetype.Swashbuckler:
                shooting -= 10;
                tackling += 10;
                maxMind += 25;
                maxStamina += 35;
                maxBalance += 35;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("Rapier1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("Rapier2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("Rapier3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("RapierSO"));
                specialsToAdd.Add("UncannyDodge");
                break;
            case Archetype.Enforcer:
                shooting -= 10;
                tackling += 10;
                maxMind += 15;
                maxStamina += 35;
                maxBalance += 25;
                specialsToAdd.Add("FaceSmash");
                break;
            case Archetype.Charlatan:
                shooting += 10;
                tackling -= 10;
                maxMind += 35;
                maxStamina += 15;
                maxBalance += 25;
                specialsToAdd.Add("ViciousMockery");
                break;
            case Archetype.LightWizard:
                passing += 10;
                tackling -= 10;
                maxMind += 35;
                maxStamina += 15;
                maxBalance += 25;
                specialsToAdd.Add("BallOfPower");
                name += " the " + Names.Wizardnick[Random.Range(0, Names.Wizardnick.Length)];
                break;
            case Archetype.DarkWizard:
                passing -= 10;
                tackling += 10;
                maxMind += 25;
                maxStamina += 25;
                maxBalance += 25;
                specialsToAdd.Add("BallHawk");
                name += " the " + Names.Wizardnick[Random.Range(0, Names.Wizardnick.Length)];
                break;
            case Archetype.ExiledWizard:
                passing -= 10;
                tackling += 10;
                maxMind += 25;
                maxStamina += 25;
                maxBalance += 25;
                specialsToAdd.Add("SummonVoidspawn");
                break;
            case Archetype.HolyWizard:
                passing += 10;
                tackling -= 10;
                maxMind += 35;
                maxStamina += 15;
                maxBalance += 25;
                specialsToAdd.Add("BallOfDevotion");
                break;
            case Archetype.ImperialWizard:
                passing += 10;
                tackling -= 10;
                maxMind += 35;
                maxStamina += 15;
                maxBalance += 25;
                specialsToAdd.Add("Protection");
                break;
            case Archetype.VoidWizard:
                passing -= 10;
                tackling += 10;
                maxMind += 25;
                maxStamina += 25;
                maxBalance += 25;
                specialsToAdd.Add("TeleportTeammate");
                break;
            case Archetype.Mercenary:
                tackling -= 10;
                carrying += 10;
                maxMind += 10;
                maxStamina += 30;
                maxBalance += 20;
                specialsToAdd.Add("BullRush");
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("Halberd1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("Halberd2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("Halberd3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("HalberdSO"));

                name += " the " + Names.Warriornick[Random.Range(0, Names.Warriornick.Length)];
                break;
            case Archetype.Soldier:
                tackling += 10;
                carrying -= 10;
                maxMind += 25;
                maxStamina += 25;
                maxBalance += 25;
                specialsToAdd.Add("SwordFlurry");
                name += " the " + Names.Warriornick[Random.Range(0, Names.Warriornick.Length)];
                break;
            case Archetype.Champion:
                tackling += 10;
                carrying -= 10;
                maxMind += 30;
                maxStamina += 30;
                maxBalance += 30;
                specialsToAdd.Add("RallyingCry");

                break;
            case Archetype.DarkKnight:
                tackling -= 10;
                carrying += 10;
                maxMind += 40;
                maxStamina += 20;
                maxBalance += 20;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("DarkHammer1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("DarkHammer2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("DarkHammer3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("DarkHammerSO"));
                specialsToAdd.Add("SoulSteal");
                break;
            case Archetype.Marauder:
                tackling -= 10;
                carrying += 10;
                maxMind += 15;
                maxStamina += 35;
                maxBalance += 20;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("Starspear1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("Starspear2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("Starspear3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("StarspearSO"));
                specialsToAdd.Add("Execute");
                break;
            case Archetype.Paladin:
                tackling += 10;
                carrying -= 10;
                maxMind += 25;
                maxStamina += 25;
                maxBalance += 25;
                activeMeleeMoves[0] = Resources.Load<BaseMeleeMove>("HolyMace1");
                activeMeleeMoves[1] = Resources.Load<BaseMeleeMove>("HolyMace2");
                activeMeleeMoves[2] = Resources.Load<BaseMeleeMove>("HolyMace3");
                meleeWeapon = Instantiate(Resources.Load<Weapon>("HolyMaceSO"));
                specialsToAdd.Add("DivineIntervention");
                break;
           
        }
        foreach (string s in specialsToAdd) {
            Type myType = Type.GetType(s);
            SpecialMove myObj = (SpecialMove)Activator.CreateInstance(myType);
            activeSpecialMoves.Add(myObj);
        }
        
        archetype = newArchetype;
        mod = Resources.Load<UpgradeModifier>(archetype + "Upgrade");
        //myGearSet = Resources.Load<CharacterGearSet>("GearSets/" + archetype.ToString());
    }
}
[System.Serializable]
public class QuestStats {
    public int staminaDamage, balanceDamage, mindDamage, healthDamage = 0;
    public int kills;
    public void Add(QuestStats stats) {
        staminaDamage += stats.staminaDamage;
        balanceDamage += stats.balanceDamage;
        mindDamage += stats.mindDamage;
        healthDamage += stats.healthDamage;
        kills += stats.kills;
    }
}
[System.Serializable]
public class SportStats {
    public int games = 0;
    public int goals = 0;
    public int assists = 0;
    public int tackles = 0;
    public int kos = 0;

    public void AddStats(SportStats adds) {
        games += adds.games;
        goals += adds.goals;
        assists += adds.assists;
        tackles += adds.tackles;
        kos += adds.kos;
    }
}
