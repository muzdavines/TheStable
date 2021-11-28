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
    [SerializeField]
    public List<Move> knownMoves = new List<Move>();
    [SerializeField]
    public List<Move> activeMoves = new List<Move>();
    public bool HasMove (string m) {
        Debug.Log("HasMove called on Character. Need to Change This");
        foreach (Move move in knownMoves) {
            if (move.name == m) { return true; }
        }
        return false; // knownMoves.Contains(m);
    }
    //Inventory
    public string startingArmor;
    public string startingWeapon;
    [SerializeField]
    public Armor armor;
    [SerializeField]
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
    [SerializeField]
    public Game.GameDate returnDate = new Game.GameDate();

    public bool activeForNextMission;
    public bool incapacitated;
    public string modelName;

    public GameObject currentObject;
    [SerializeField]
    public MissionCharacter currentMissionCharacter;

    public void Awake() {
        Armor startingArmorSO = Resources.Load<Armor>(startingArmor);
        Weapon startingWeaponSO = Resources.Load<Weapon>(startingWeapon);
        if (startingArmorSO == null) { armor = new Armor(); } else { armor = Instantiate(startingArmorSO); }
        if (startingWeaponSO == null) { Debug.Log("Character new weapon " + name); weapon = new Weapon(); } else { Debug.Log("Char inst weapon " + name); weapon = Instantiate(startingWeaponSO); }
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
                    Debug.Log("Raise Attribute " + currentTraining.training);
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
}

