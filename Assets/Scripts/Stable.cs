using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
[System.Serializable]
public class Stable
{
    //Everything for the player


    //NEEDED: List of all Buildings
    public static string[] stableNameList = {"The Clumsy Dragon Tavern","The Full Piano Inn","The Royal Hyena Pub", "The Calm Plate Pub", "The Mixing Ducks Pub","The Hot Elephant Pub","The Chunky Hog Tavern" };

    public string stableName;
   //Needed: List of all accolades
    public int reputation; //Quality of the Stable
    public float alignment; //Good, Neutral, Evil (1,0,-1)
    public int[] favor; //how does each faction like them? map faction id to const enum
    [SerializeField]
    public Warlord warlord = new Warlord();
    public List<Character> heroes = new List<Character>();
    public List<Character> coaches = new List<Character>();
    public List<StableBuilding> buildings = new List<StableBuilding>();
    public List<MissionContract> contracts = new List<MissionContract>();
    public List<Trait> availableTrainings = new List<Trait>();
    
    [SerializeField]
    public Finance finance = new Finance();
    public int coachPoints;
    public List<Item> inventory = new List<Item>();
    public MissionContract activeContract;
    public int leagueLevel = 0;
    public Color primaryColor;
    public Color secondaryColor;
    public bool PurchaseHero(Character hero) {
        if (hero.contract.signingBonus > finance.gold) {
            Debug.Log("Player does not have enough money to sign " + hero.name);
            return false;
        }
        finance.AddExpense(hero.contract.signingBonus, LedgerAccount.Personnel, "Signing Bonus for " + hero.name);
        heroes.Add(hero);
        SortHeroes();
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

    public void SortHeroes() {
        heroes.Sort((x, y) => (x.archetype).CompareTo(y.archetype));
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

    public int NumberHeroesInLineup() {
        int count = 0;
        foreach (Character c in heroes) {
            if (c.activeInLineup) { count++; }
        }
        return count;
    }
    
}


public enum Faction { Empire = 0, Enclave = 1, Zhentarim = 2 };

