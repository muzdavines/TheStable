using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionFinalDetails : MonoBehaviour
{
    public bool successful;
    public List<string> narrative = new List<string>();
    public int goldReward;
    public Finance.Business businessReward;
    public Move moveReward;
    public List<Item> itemRewards;
    public float finalMod;
    public class DamageDetails {
        public QuestStats stats;
        public Character character;
    }
    public List<DamageDetails> damageDetails = new List<DamageDetails>();

    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    
}
