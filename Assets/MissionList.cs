using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class MissionList : ScriptableObject
{
    [System.Serializable]
   public class MissionListEntry {
        public MissionContract contract;
        public string attributeReq = "None";
        public int attributeReqAmount = 0;
        public string attributeReq2 = "None";
        public int attributeReqAmount2 = 0;
        public int minHeroes = 1;
        public Game.GameDate availableAfter = new Game.GameDate();
    }
    [SerializeField]
    public List<MissionListEntry> list = new List<MissionListEntry>();

    public List<MissionContract> GetContracts() {
        List<MissionContract> missions = new List<MissionContract>();
        foreach (MissionListEntry e in list) {
            MissionContract m = Instantiate(e.contract);
            m.attributeReq = e.attributeReq;
            m.attributeReq2 = e.attributeReq2;
            m.attributeReqAmount = e.attributeReqAmount;
            m.attributeReqAmount2 = e.attributeReqAmount2;
            missions.Add(m);
        }
        return missions;
    }
}
