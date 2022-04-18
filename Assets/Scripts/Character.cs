using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StableMasterType { Warrior, Wizard, Rogue};
public enum Gender { Male, Female }
public enum Race { Human, Elf }
public enum SkinColor { White, Brown, Black, Elf }
public enum Elements { Yes, No }
public enum HeadCovering { HeadCoverings_Base_Hair, HeadCoverings_No_FacialHair, HeadCoverings_No_Hair }
public enum FacialHair { Yes, No }
[CreateAssetMenu]
[System.Serializable]
public class Character : Living 
{
    //Physical Attributes
    public int strength = 5;
    public int agility = 5;
    public int reaction = 5;
    public int dexterity = 5;
    
    public int carrying = 5;
    public int toughness = 10;
    public int tackling = 5;
    public int blocking = 5;
    public int runspeed = 5;
    public int shooting = 5;
    public int passing = 5;
    public int catching = 5;
    //Mental Attributes
    public int speech = 5;
    public int intelligence = 5;
    public int education = 5;
    public int motivation = 5;
    public int strategist = 5;
    public int economics = 5;
    public int negotiating = 5;
    public int insight = 5; //tell if people are lying
    public int deception = 5;
    public int intimidation = 5;
    public int persuasion = 5;
    public int gambling = 5;
    public int confidence = 5;

    //Technical Attributes
    public int lockpicking = 5;
    public int pickpocketing = 5;
    public int trapSetting = 5;
    public int trapDisarming = 5;
    public int pugilism = 5;
    public int martialarts = 2;
    public int melee = 10;
    public int ranged = 10;
    public int magic = 10;
    public int parry = 5;
    public int shieldDefense;
    public int musician;
    public int medicine;
    public int swordsmanship = 5;
    public int dualwielding = 5;
    public int archery = 5;
    public int pistols = 5;

    //Survival Attributes
    public int survivalist = 10;
    public int landNavigation = 10;
    public int hunting = 15;
    public int foraging;
    public int herbLore;
    public int camping;


    //Magical Attributes
    public int attackmagic, defensemagic, supportmagic;

    //Coaching Attributes

    //Current Condition
    public int condition; //current tiredness 0-100
    public int sharpness; //how well trained they are 0-100
    /// <summary>
    /// dots, 0 = death
    /// </summary>
    public int health; //dots - 0 = death

    //need some enums for armor type so we can have bonuses and penalties - dodge penalty for plate, etc.
    public CombatFocus combatFocus;

    //new combat system
    public int maxStamina;
    public int maxBalance;
    public int maxMind;
    public int maxHealth; //health defined above

    public List<Move> knownMoves = new List<Move>();
    public List<MoveSave> knownMovesSave = new List<MoveSave>();
    public List<Move> activeMeleeMoves = new List<Move>();
    public List<Move> activeRangedMoves = new List<Move>();
    public List<string> startingSpecialMoves = new List<string>();
    public List<SpecialMove> activeSpecialMoves = new List<SpecialMove>();
    public List<MoveSave> activeMovesSave = new List<MoveSave>();
    public SportStats seasonStats = new SportStats();
    public SportStats careerStats = new SportStats();

    public Gender gender;
    public Race race = Race.Human;
    public SkinColor skinColor;
    public Elements elements;
    public HeadCovering headCovering;
    public FacialHair facialHair;
    public CharacterGearSet myGearSet;
    public bool HasMove (string m) {
        Debug.Log("#TODO#HasMove called on Character. Need to Change This");
        foreach (Move move in knownMoves) {
           
            if (move.name.Contains(m)) { return true; }
        }
        return false; // knownMoves.Contains(m);
    }
    //Inventory
    public string startingArmor;
    public string startingMeleeWeapon;
    public string startingRangedWeapon;
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

    public GameObject currentObject;
    
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
                myGearSet = Resources.Load<CharacterGearSet>("GearSets/Warrior");
                modelNum = 0;
                break;
            case Archetype.Warrior:
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
                startingSpecialMoves.Add("ShoulderBarge");
                startingSpecialMoves.Add("PowerSlam");
                startingSpecialMoves.Add("ClosingSpeed");
                modelName = "SCUnit3";
                myGearSet = Resources.Load<CharacterGearSet>("GearSets/Warrior");
                modelNum = 0;

                break;
            case Archetype.Rogue:
                shooting = 20;
                passing = 12;
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
                knownMoves.Add(Resources.Load<Move>("OneTimerKick"));
                modelName = "SCUnit3";
                modelNum = 2;
                myGearSet = Resources.Load<CharacterGearSet>("GearSets/Rogue");
                break;
            case Archetype.Wizard:
                shooting = 5;
                passing = 20;
                tackling = 15;
                carrying = 15;
                catching = 15;
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
                modelName = "SCUnit3";
                modelNum = 1;
                myGearSet = Resources.Load<CharacterGearSet>("GearSets/Wizard");
                break;
        }
        
        seasonStats = new SportStats();
        careerStats = new SportStats();
        Init();
    }

   
    public enum Archetype { Striker, Winger, Midfielder, Defender, Goalkeeper, Warrior, Rogue, Wizard}
    public Archetype archetype;

    public void Awake() {

    }
    public void Start() {
        
    }
    public Character Init() {
        Armor startingArmorSO = Resources.Load<Armor>(startingArmor);
        Weapon startingMeleeWeaponSO = Resources.Load<Weapon>(startingMeleeWeapon);
        Weapon startingRangedWeaponSO = Resources.Load<Weapon>(startingRangedWeapon);
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
        return this;
    }

    public void StartTraining (Training t) {
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

    bool GetPercent(int pct) {
        bool p = false;
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll <= pct) {
            p = true;
        }
        return p;
    }
    public void CopyValues(Character source) {
        this.strength = source.strength;
        this.agility = source.agility;
        this.reaction = source.reaction;
        
        this.swordsmanship = source.swordsmanship;
        this.dualwielding = source.dualwielding;
        this.carrying = source.carrying;
        this.archery = source.archery;
        this.toughness = source.toughness;
        this.toughness = source.toughness;
        this.intelligence = source.intelligence;
        this.education = source.education;
        this.motivation = source.motivation;
        this.strategist = source.strategist;
        this.economics = source.economics;
        this.negotiating = source.negotiating;
        this.insight = source.insight;
        this.deception = source.deception;
        this.intimidation = source.intimidation;
        this.lockpicking = source.lockpicking;
        this.pickpocketing = source.pickpocketing;
        this.trapSetting = source.trapSetting;
        this.trapDisarming = source.trapDisarming;
        this.pugilism = source.pugilism;
        this.martialarts = source.martialarts;
        this.melee = source.melee;
        this.parry = source.parry;
        this.shieldDefense = source.shieldDefense;
        this.survivalist = source.survivalist;
        this.landNavigation = source.landNavigation;
        this.hunting = source.hunting;
        this.foraging = source.foraging;
        this.herbLore = source.herbLore;
        this.camping = source.camping;
        this.attackmagic = source.attackmagic;
        this.condition = source.condition;
        this.sharpness = source.sharpness;
        this.health = source.health;
        this.maxStamina = source.maxStamina;
        this.maxBalance = source.maxBalance;
        this.maxMind = source.maxMind;
        this.maxHealth = source.maxHealth;
        this.knownMoves = source.knownMoves;
        this.activeMeleeMoves = source.activeMeleeMoves;
        this.startingArmor = source.startingArmor;
        this.startingMeleeWeapon = source.startingMeleeWeapon;
        this.armor = source.armor;
        this.meleeWeapon = source.meleeWeapon;
        this.mat = source.mat;
        this.contract = source.contract;
        this.currentTraining = source.currentTraining;
        this.returnDate = source.returnDate;
        this.activeForNextMission = source.activeForNextMission;
        this.incapacitated = source.incapacitated;
        this.modelName = source.modelName;
        this.currentObject = source.currentObject;
        
        this.defensemagic = source.defensemagic;
        this.supportmagic = source.supportmagic;
        this.name = source.name;
    }
}
[System.Serializable]
public class CharacterSave {
    public string name;
    //Physical Attributes
    public int strength = 5;
    public int agility = 5;
    public int reaction = 5;
    public int running = 5;
    public int swordsmanship = 5;
    public int dualwielding = 5;
    public int dodging = 5;
    public int archery = 5;

    public int toughness = 10;

    //Mental Attributes
    public int speech = 5;
    public int intelligence = 5;
    public int education = 5;
    public int motivation = 5;
    public int strategist = 5;
    public int economics = 5;
    public int negotiating = 5;
    public int insight = 5; //tell if people are lying
    public int deception = 5;
    public int intimidation = 5;


    //Technical Attributes
    public int lockpicking = 5;
    public int pickpocketing = 5;
    public int trapSetting = 5;
    public int trapDisarming = 5;
    public int pugilism = 5;
    public int martialarts = 2;
    public int melee = 10;
    public int parry = 5;
    public int shieldDefense;

    //Survival Attributes
    public int survivalist = 10;
    public int landNavigation = 10;
    public int hunting = 15;
    public int foraging;
    public int herbLore;
    public int camping;


    //Magical Attributes
    public int attackMagic, defenseMagic, supportMagic;

    //Coaching Attributes

    //Current Condition
    public int condition; //current tiredness 0-100
    public int sharpness; //how well trained they are 0-100
    /// <summary>
    /// dots, 0 = death
    /// </summary>
    public int health; //dots - 0 = death

    //need some enums for armor type so we can have bonuses and penalties - dodge penalty for plate, etc.


    //new combat system
    public int maxStamina;
    public int maxBalance;
    public int maxMind;
    public int maxHealth; //health defined above

    public List<Move> knownMoves = new List<Move>();
    public List<MoveSave> knownMovesSave = new List<MoveSave>();
    public List<Move> activeMoves = new List<Move>();
    public List<MoveSave> activeMovesSave = new List<MoveSave>();
    
    //Inventory
    public string startingArmor;
    public string startingWeapon;
    public Armor armor;
    public Weapon weapon;

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

    public Game.GameDate returnDate;

    public bool activeForNextMission;
    public bool incapacitated;
    public string modelName;

    public GameObject currentObject;
    public MissionCharacter currentMissionCharacter;
    
    public CharacterSave CopyValues(Character source) {
        var activeMoveSaveList = new List<MoveSave>();
        var knownMoveSaveList = new List<MoveSave>();
        for (int i = 0; i < source.activeMeleeMoves.Count; i++) {
            activeMoveSaveList.Add(new MoveSave().CopyValues(source.activeMeleeMoves[i]));
        }
        this.activeMovesSave = activeMoveSaveList;
        for (int i = 0; i < source.knownMoves.Count; i++) {
            knownMoveSaveList.Add(new MoveSave().CopyValues(source.knownMoves[i]));
        }
        this.knownMovesSave = knownMoveSaveList;
        this.strength = source.strength;
        this.agility = source.agility;
        this.reaction = source.reaction;
        
        this.swordsmanship = source.swordsmanship;
        this.dualwielding = source.dualwielding;
        this.dodging = source.carrying;
        this.archery = source.archery;
        this.toughness = source.toughness;
        this.toughness = source.toughness;
        this.intelligence = source.intelligence;
        this.education = source.education;
        this.motivation = source.motivation;
        this.strategist = source.strategist;
        this.economics = source.economics;
        this.negotiating = source.negotiating;
        this.insight = source.insight;
        this.deception = source.deception;
        this.intimidation = source.intimidation;
        this.lockpicking = source.lockpicking;
        this.pickpocketing = source.pickpocketing;
        this.trapSetting = source.trapSetting;
        this.trapDisarming = source.trapDisarming;
        this.pugilism = source.pugilism;
        this.martialarts = source.martialarts;
        this.melee = source.melee;
        this.parry = source.parry;
        this.shieldDefense = source.shieldDefense;
        this.survivalist = source.survivalist;
        this.landNavigation = source.landNavigation;
        this.hunting = source.hunting;
        this.foraging = source.foraging;
        this.herbLore = source.herbLore;
        this.camping = source.camping;
        this.attackMagic = source.attackmagic;
        this.condition = source.condition;
        this.sharpness = source.sharpness;
        this.health = source.health;
        this.maxStamina = source.maxStamina;
        this.maxBalance = source.maxBalance;
        this.maxMind = source.maxMind;
        this.maxHealth = source.maxHealth;
        this.knownMoves = source.knownMoves;
        this.activeMoves = source.activeMeleeMoves;
        this.startingArmor = source.startingArmor;
        this.startingWeapon = source.startingMeleeWeapon;
        this.armor = source.armor;
        this.weapon = source.meleeWeapon;
        this.mat = source.mat;
        this.contract = source.contract;
        this.currentTraining = source.currentTraining;
        this.returnDate = source.returnDate;
        this.activeForNextMission = source.activeForNextMission;
        this.incapacitated = source.incapacitated;
        this.modelName = source.modelName;
        this.currentObject = source.currentObject;
        
        this.defenseMagic = source.defensemagic;
        this.supportMagic = source.supportmagic;
        this.name = source.name;
        return this;
    }
}

public class SportStats {
    public int games = 0;
    public int goals = 0;
    public int assists = 0;
    public int tackles = 0;
    public int kos = 0;
}
