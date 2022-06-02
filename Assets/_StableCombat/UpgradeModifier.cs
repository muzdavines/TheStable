using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class UpgradeModifier : ScriptableObject {
    public float passing = 1;
    public float shooting = 1;
    public float tackling = 1;
    public float carrying = 1;
    public float runspeed = 1;

    public int passingReq = 0;
    public int shootingReq = 0;
    public int tacklingReq = 0;
    public int carryingReq = 0;
    public string promoteMainText;
    [System.Serializable]
    public class Promote {
        public string promoteDescription;
        public string promoteButtonLabel;
        public Character.Archetype promoteArchetype;
        
        public string selectionText;
    }
    [SerializeField]
    public Promote[] promoteOptions;
   


}
