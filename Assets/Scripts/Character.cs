using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharClass { Warrior, Wizard, Rogue};
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
    public List<Move> activeSpecialMoves = new List<Move>();
    public List<MoveSave> activeMovesSave = new List<MoveSave>();
    public SportStats seasonStats = new SportStats();
    public SportStats careerStats = new SportStats();
    public bool HasMove (string m) {
        Debug.Log("HasMove called on Character. Need to Change This");
        foreach (Move move in knownMoves) {
            if (move.name == m) { return true; }
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
        int roll = Random.Range(0, 100);
        float interval = (max - min) / 6;
        int[] breaks = { 5, 27, 18, 18, 27, 5 };
        float[] array = new float[100];
        int count = 0;
        for (int x = 0; x < 6; x++) {
            for (int z = 0; z < breaks[x]; z++) {
                Debug.Log(count);
                array[count++] = Mathf.Round(min + (x * interval));
            }
        }
        return (int)array[roll];
    }
    public void GenerateCharacter(Archetype thisArchetype, int level = 1) {
        int[] dodgeRange = new int[2];
        int[] tackleRange = new int[2]; 
        int[] blockingRange = new int[2]; 
        int[] runSpeedRange = new int[2];
        switch (thisArchetype) {
            case Archetype.Striker:
                dodgeRange[0] = 12;
                dodgeRange[1] = 18;
                tackleRange[0] = 2;
                tackleRange[1] = 8;
                blockingRange[0] = 2;
                blockingRange[1] = 8;
                runSpeedRange[0] = 9;
                runSpeedRange[1] = 15;
                modelNum = 0;
                modelName = "SCUnit";
                break;
            case Archetype.Winger:
                dodgeRange[0] = 7;
                dodgeRange[1] = 13;
                tackleRange[0] = 4;
                tackleRange[1] = 10;
                blockingRange[0] = 7;
                blockingRange[1] = 13;
                runSpeedRange[0] = 9;
                runSpeedRange[1] = 12;
                modelNum = 1;
                modelName = "SCUnit";
                break;
            case Archetype.Midfielder:
                dodgeRange[0] = 4;
                dodgeRange[1] = 10;
                tackleRange[0] = 6;
                tackleRange[1] = 12;
                blockingRange[0] = 12;
                blockingRange[1] = 18;
                runSpeedRange[0] = 9;
                runSpeedRange[1] = 11;
                modelNum = 2;
                modelName = "SCUnit";
                break;
            case Archetype.Defender:
                dodgeRange[0] = 2;
                dodgeRange[1] = 8;
                tackleRange[0] = 15;
                tackleRange[1] = 30;
                blockingRange[0] = 9;
                blockingRange[1] = 15;
                runSpeedRange[0] = 9;
                runSpeedRange[1] = 10;
                modelNum = 0;
                modelName = "SCUnit2";
                break;
            case Archetype.Goalkeeper:
                dodgeRange[0] = 4;
                dodgeRange[1] = 10;
                tackleRange[0] = 6;
                tackleRange[1] = 12;
                blockingRange[0] = 12;
                blockingRange[1] = 18;
                runSpeedRange[0] = 9;
                runSpeedRange[1] = 11;
                modelNum = 2;
                modelName = "SCUnit";
                break;
        }
        carrying = RandDist(dodgeRange[0], dodgeRange[1]);
        tackling = RandDist(tackleRange[0], tackleRange[1]);
        blocking = RandDist(blockingRange[0], blockingRange[1]);
        runspeed = RandDist(runSpeedRange[0], runSpeedRange[1]);
        maxHealth = 5;
        maxMind = 5;
        maxStamina = 5;
        maxBalance = 5;
        startingMeleeWeapon = "FistsSO";
        startingRangedWeapon = "BowSO";
        archetype = thisArchetype;
        var leftjab = Resources.Load<Move>("LeftJab");
        var rightjab = Resources.Load<Move>("RightJab");
        knownMoves.Add(Instantiate(leftjab)); 
        knownMoves.Add(Instantiate(rightjab));
        activeMeleeMoves.Add(Instantiate(leftjab));
        activeMeleeMoves.Add(Instantiate(leftjab));
        activeMeleeMoves.Add(Instantiate(rightjab));
        seasonStats = new SportStats();
        careerStats = new SportStats();
    }

   
    public enum Archetype { Striker, Winger, Midfielder, Defender, Goalkeeper}
    public Archetype archetype;

    public void Awake() {
        Armor startingArmorSO = Resources.Load<Armor>(startingArmor);
        Weapon startingMeleeWeaponSO = Resources.Load<Weapon>(startingMeleeWeapon);
        Weapon startingRangedWeaponSO = Resources.Load<Weapon>(startingRangedWeapon);
        if (startingArmorSO == null) { armor = new Armor(); } else { armor = Instantiate(startingArmorSO); }
        if (startingMeleeWeaponSO == null) { Debug.Log("Character new melee weapon " + name); meleeWeapon = new Weapon(); } else { Debug.Log("Char inst weapon " + name); meleeWeapon = Instantiate(startingMeleeWeaponSO); }
        if (startingRangedWeaponSO == null) { Debug.Log("Character new ranged weapon " + name); rangedWeapon = new Weapon(); } else { Debug.Log("Char inst ranged weapon " + name); rangedWeapon = Instantiate(startingRangedWeaponSO); }
        if (currentTraining == null) { currentTraining = new Training(); }
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
                    Debug.Log("Rase Attribute " + currentTraining.training);
                    int currentValue = (int)typeof(Character).GetField(currentTraining.training).GetValue(this);
                    typeof(Character).GetField(currentTraining.training).SetValue(this, currentValue + 1);
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

    public Game.GameDate returnDate = new Game.GameDate();

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
