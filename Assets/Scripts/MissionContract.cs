using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class MissionContract : ScriptableObject
{
    public string description;
    public int ID;
    public ContractType contractType;
    
    public int difficulty;
    [SerializeField]
    public List<Stage> stages;
    /// <summary>
    /// One time gold reward.
    /// </summary>
    public int goldReward;
    [SerializeField]
    /// <summary>
    /// Adds this business to the stable
    /// </summary>
    /// 
    public Finance.Business businessReward;
    public Move moveReward;
    public List<Item> itemRewards;
    public Game.GameDate executionDate;
    public int dayCost; //number of days heroes will be out of commission
    //need to add more robust rewards here, perhaps a subclass for things like establishing a recruitment station
    public int minHeroes = 1;
    public int maxHeroes = 1;
    public string attributeReq = "None";
    public int attributeReqAmount = 0;
    public string attributeReq2 = "None";
    public int attributeReqAmount2 = 0;
    public virtual void GiveRewards() {

    }
    

    public void Init() {
        if (ID == 0) {
            MissionContract[] contracts = Resources.LoadAll<MissionContract>("");
            int max = 0;
            foreach (MissionContract c in contracts) {
                if (c.ID > max) { max = c.ID; }
            }
            ID = max + 1;
        }
        foreach (Stage s in stages) {
            s.Init();
        }
    }

    public void GenerateRandom(int level = 1) {
        //contractType = (ContractType)Random.Range(0, 8);
        contractType = ContractType.Collection;
        MissionContract blueprint = Resources.Load<MissionContract>("Contract" + contractType.ToString());
       
        if (blueprint != null) {
            MissionContract instance = Instantiate<MissionContract>(blueprint);
            description = instance.description;
            stages = instance.stages;
            foreach (Stage s in stages) {
                s.Init();
            }
            maxHeroes = instance.maxHeroes;
            businessReward = instance.businessReward;
            goldReward = instance.goldReward;
            moveReward = instance.moveReward;
        } else {
            description = "Random Description " + Random.Range(0, 100000);
            stages = new List<Stage>();
            int numStages = Random.Range(1, 4);
            for (int i = 0; i < numStages; i++) {
                Stage s = new Stage();
                s.difficulty = level;
                s.type = (StageType)Random.Range(0, 6);
                s.description = "Stage Description " + Random.Range(0, 10000);
                s.Init();
                stages.Add(s);
            }
            maxHeroes = Random.Range(1, 5);
            goldReward = Random.Range(0, 1001);
        }

        difficulty = level;
        
        executionDate = Game.instance.gameDate.Add(11);
        
    }
}
public enum ContractType { Assassinate, Heist, Recruitment, MercantileExpansion, Intimidate, Interrogate, Defend, Protect, MercenaryBattle, Collection, Persuade, Passage}
[System.Serializable]
public class MissionContractSave {
    public string description;
    public int ID;
    public ContractType contractType;

    public int difficulty;
    [SerializeField]
    public List<Stage> stages;
    /// <summary>
    /// One time gold reward.
    /// </summary>
    public int goldReward;
    [SerializeField]
    /// <summary>
    /// Adds this business to the stable
    /// </summary>
    /// 
    public Finance.Business businessReward;
    public Move moveReward;
    public List<Item> itemsRewards;
    public Game.GameDate executionDate;
    public int dayCost; //number of days heroes will be out of commission
    //need to add more robust rewards here, perhaps a subclass for things like establishing a recruitment station
    public int minHeroes = 1;
    public int maxHeroes = 1;
    public string attributeReq = "None";
    public int attributeReqAmount = 0;
    public string attributeReq2 = "None";
    public int attributeReqAmount2 = 0;

    public MissionContractSave CopyValues (MissionContract source) {
        this.description = source.description;
        this.ID = source.ID;
        this.contractType = source.contractType;
        this.difficulty = source.difficulty;
        this.stages = source.stages;
        this.goldReward = source.goldReward;
        this.businessReward = source.businessReward;
        this.moveReward = source.moveReward;
        this.executionDate = source.executionDate;
        this.dayCost = source.dayCost;
        this.minHeroes = source.minHeroes;
        this.maxHeroes = source.maxHeroes;
        this.attributeReq = source.attributeReq;
        this.attributeReqAmount = source.attributeReqAmount;
        this.attributeReq2 = source.attributeReq2;
        this.attributeReqAmount2 = source.attributeReqAmount2;
        this.itemsRewards = source.itemRewards;
        return this;
    }
}