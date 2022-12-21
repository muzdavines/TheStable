
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [CreateAssetMenu(fileName = "OSM_", menuName = "FImpossible Creations/Procedural Generation/Object Stamper Multi Preset", order = 10101)]
    public partial class OStamperMultiSet : ScriptableObject
    {
        public List<OStamperSet> PrefabsSets;
        public bool _editorDrawStamps = true;
        public int FocusOn = -1;

        //[Tooltip("Minimum target spawn count for emitter")]
        //public int MinTotalSpawnCount = 6;
        //[Tooltip("Maximum target spawn count for emitter")]
        //public int MaxTotalSpawnCount = 14;


        public bool SetHashExists(int hashCode)
        {
            for (int i = 0; i < PrefabSetSettings.Count; i++)
                if (PrefabSetSettings[i].TargetSet != null)
                    if (PrefabSetSettings[i].TargetSet.GetInstanceID() == hashCode) return true;

            return false;
        }


        // Rest of the code inside partial classes -------------------
    }
}