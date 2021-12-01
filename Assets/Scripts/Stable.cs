using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
[System.Serializable]
public class Stable
{
    //Everything for the player


    //NEEDED: List of all Buildings
    public static string[] stableNameList = {"The Calm Plate Pub","The Clumsy Dragon Tavern","The Full Piano Inn","The Royal Hyena Pub","The Cheap Dragonfruit Tavern","The Mixing Ducks Pub","The Hot Elephant Pub","The Chunky Hog Pub" };

    public string stableName;
   //Needed: List of all accolades
    public int reputation; //Quality of the Stable
    public float alignment; //Good, Neutral, Evil (1,0,-1)
    public int[] favor; //how does each faction like them? map faction id to const enum
    [SerializeField]
    public Warlord warlord = new Warlord();
    public List<Character> heroes = new List<Character>();
    public List<CharacterSave> heroesSave = new List<CharacterSave>();
    public List<Character> coaches = new List<Character>();
    public List<StableBuilding> buildings = new List<StableBuilding>();
    public List<MissionContract> contracts = new List<MissionContract>();
    public List<Training> availableTrainings = new List<Training>();
    public List<TrainingSave> availableTrainingsSave = new List<TrainingSave>();
    [SerializeField]
    public Finance finance = new Finance();
    public List<Item> inventory = new List<Item>();
    public MissionContract activeContract;
    public int leagueLevel = 0;
    public bool PurchaseHero(Character hero) {
        if (hero.contract.signingBonus > finance.gold) {
            Debug.Log("Player does not have enough money to sign " + hero.name);
            return false;
        }
        finance.AddExpense(hero.contract.signingBonus, LedgerAccount.Personnel, "Signing Bonus for " + hero.name);
        heroes.Add(hero);
        return true;
    }
    public void AcceptContract(MissionContract c) {
        contracts.Add(c);
    }
    public void ExpireContracts() {
        contracts.RemoveAll(Helper.IsExpired);
        StableContractsScrollerController scsc = GameObject.FindObjectOfType<StableContractsScrollerController>();
        if (scsc != null) {
            scsc.OnEnable();
        }
    }
    public void SetAllHeroesInactive() {
        foreach (Character c in heroes) {
           
            c.activeForNextMission = false;
        }
    }

    bool IsDead(Character c) {
        if (c.health <= 0) {
            Debug.Log("Dead: " + c.name + " But ignoring permadeath.");
            c.health = 4;
            return false;
            return true;
        } else { return false; }
    }
    public void ProcessDeadHeroes() {
        Debug.Log("Add something here to account for dead heroes in the narrative. Maybe do a funeral for a legend or something.");
        heroes.RemoveAll(IsDead);
    }

    public int NumberActiveHeroes() {
        int count = 0;
        foreach (Character c in heroes) {
            if (c.activeForNextMission) { count++; }
        }
        return count;
    }
    public void PrepForSave() {
        PrepAvailableTrainingForSave();
        PrepHeroesForSave();
    }
    public void OnLoad() {
        LoadAvailableTraining();
        LoadHeroes();
    }

    public void PrepAvailableTrainingForSave() {
        var saveList = new List<TrainingSave>();
        for (int i = 0; i < availableTrainings.Count; i++) {
            saveList.Add(new TrainingSave().CopyValues(availableTrainings[i]));
        }
        availableTrainingsSave = saveList;
    }
    public void LoadAvailableTraining() {
        if (availableTrainingsSave != null && availableTrainingsSave.Count > 0) {
            availableTrainings = availableTrainingsSave.LoadTrainings();
        }
        availableTrainingsSave = new List<TrainingSave>();
    }
    public void PrepHeroesForSave() {
        var saveList = new List<CharacterSave>();
        for (int i = 0; i < heroes.Count; i++) {
            saveList.Add(new CharacterSave().CopyValues(heroes[i]));
        }
        heroesSave = saveList;
    }
    public void LoadHeroes() {
        if (heroesSave != null && heroesSave.Count > 0) {
            heroes = heroesSave.LoadCharacters();
        }
        heroesSave = new List<CharacterSave>();
    }

}


public enum Faction { Empire = 0, Enclave = 1, Zhentarim = 2 };

