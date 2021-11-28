using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
[System.Serializable]
public class MissionContractTest : ScriptableObject {
    /*public string description;
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
    [SerializeField]
    public Move moveReward;
    [SerializeField]
    public Game.GameDate executionDate;
    public int dayCost; //number of days heroes will be out of commission*/
    //need to add more robust rewards here, perhaps a subclass for things like establishing a recruitment station
    [ES3Serializable]
    public int minHeroes = 1;
    [ES3Serializable]
    public int maxHeroes = 1;
    [ES3Serializable]
    public string attributeReq = "None";
    [ES3Serializable]
    public int attributeReqAmount = 0;
    [ES3Serializable]
    public string attributeReq2 = "None";
    [ES3Serializable]
    public int attributeReqAmount2 = 0;
   
}