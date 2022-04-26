using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class MissionList : ScriptableObject
{
    [System.Serializable]
   public class MissionListEntry {
        public MissionContract contract;
        public Game.GameDate availableAfter = new Game.GameDate();
    }
    [SerializeField]
    public List<MissionListEntry> list = new List<MissionListEntry>();

    public List<MissionContract> GetContracts() {
        List<MissionContract> missions = new List<MissionContract>();
        foreach (MissionListEntry e in list) {
            if (!Game.instance.gameDate.IsOnOrAfter(e.availableAfter)) { continue; }
            MissionContract m = Instantiate(e.contract);
            /*m.traitReq = e.traitReq;
            m.traitReq2 = e.traitReq2;
            m.traitLevelReq = e.traitLevelReq;
            m.traitLevelReq2 = e.traitLevelReq2;*/
            missions.Add(m);
        }
        return missions;
    }
}
