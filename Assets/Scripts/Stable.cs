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
    [SerializeField] public Warlord warlord;
    public List<Character> heroes = new List<Character>();
    public List<string> heroSaves = new List<string>();
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

    public void ProcessKnockouts() {
        foreach (Character c in heroes) {
            for (int i = 0; i < c.knockoutsToProcess; i++) {
                float ageThreshold = 25;
                float agePenalty = .01f;
                float knockoutThreshold = 0;
                float knockoutPenalty = .02f;
                float pct = Mathf.Clamp(c.age - ageThreshold, 0, Mathf.Infinity) * agePenalty +
                            Mathf.Clamp(c.careerKnockouts - knockoutThreshold, 0, Mathf.Infinity) * knockoutPenalty;
                float roll = Random.Range(0f, 1f);
                Debug.Log("#Knockout#" + c.name + " " + pct + " " + roll);
                if (roll < pct) {
                    Debug.Log("#Knockout#Failed Roll " + c.name);
                    c.maxHealth--;
                }
                else {
                    Debug.Log("#Knockout#Succeed Roll " + c.name);
                }
                c.careerKnockouts++;
            }
            c.knockoutsToProcess = 0;
        }
    }
    public void SortHeroes() {
        heroes.Sort((x, y) => (x.archetype).CompareTo(y.archetype));
    }

    bool IsDead(Character c) {
        if (c.maxHealth <= 0) {
            Debug.Log("Dead: " + c.name + " Removing.");
            return true;
        } else { return false; }
    }
    public void ProcessDeadHeroes() {
        Debug.Log("Add something here to account for dead heroes in the narrative. Maybe do a funeral for a legend or something.");
        foreach (var c in heroes) {
            if (c.maxHealth <= 0) {
                Game.instance.news.Add(new NewsItem() { body = c.name + " has suffered a career ending injury. This occurs when a hero suffers too many knockouts in missions or playing Flonkball. Each time a hero is downed, there is an increasingly likely chance they will suffer this kind of injury.", date = Game.instance.gameDate, sender = "The Boss", subject = "A Hero Has Retired" });
            }
        }
        
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

