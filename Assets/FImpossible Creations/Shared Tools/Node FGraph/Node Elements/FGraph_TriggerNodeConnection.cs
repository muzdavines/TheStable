using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    /// <summary> Connection between whole nodes, NOT between node ports! </summary>
    public class FGraph_TriggerNodeConnection /*: ScriptableObject*/
    {
        /// <summary> Node ID</summary>
        public int ConnectionFromID = -1;
        /// <summary> Node ID</summary>
        public int ConnectionToID = -1;

        /// <summary> Can be used to support multiple connections </summary>
        public int ConnectionFrom_AlternativeID = -1;
        public int ConnectionTo_AlternativeID = -1;

        /// <summary> Can be used for displaying debugging progress on node from 0 to 1 for example </summary>
        public float DebuggingProgress { get; set; } = -1f;

        /// <summary> Assigning nodes references basing on the IDs </summary>
        public void RefreshReferences<T>(List<T> allNodes) where T : FGraph_NodeBase
        {
            if (allNodes == null) return;

            From = null;
            To = null;


            for (int i = allNodes.Count - 1; i >= 0; i--)
            {
                if (allNodes[i].IndividualID == ConnectionFromID)
                {
                    if (allNodes[i].IndividualID != -1)
                        From = allNodes[i];
                }
                else if (allNodes[i].IndividualID == ConnectionToID)
                {
                    if (allNodes[i].IndividualID != -1)
                        To = allNodes[i];
                }
            }
        }


        private FGraph_NodeBase ifrom = null;
        public FGraph_NodeBase From
        {
            get { if (ifrom == null) return null; else return ifrom; }
            set { ifrom = value; if (ifrom != null) { ConnectionFromID = ifrom.IndividualID; } }
        }

        private FGraph_NodeBase ito = null;
        public FGraph_NodeBase To
        {
            get { if (ito == null) return null; else return ito; }
            set { ito = value; if (ito != null) ConnectionToID = ito.IndividualID; }
        }

        /// <summary> Can be used to compute graph execution flow </summary>
        public bool Computing { get; set; } = false;
        public bool Launched { get; set; } = false;

        public FGraph_NodeBase GetOther(FGraph_NodeBase otherThan)
        {
            if (From == otherThan) return To; else return From;
        }

        public bool IsConnectedWith(FGraph_NodeBase otherNode)
        {
            if (From == otherNode) return true;
            if (To == otherNode) return true;
            return false;
        }

    }

}