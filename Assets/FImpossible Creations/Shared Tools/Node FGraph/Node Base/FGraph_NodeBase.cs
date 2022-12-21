using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraph_NodeBase : ScriptableObject
    {
        [HideInInspector] public int IndividualID = -1;
        /// <summary> Custom name </summary>
        [HideInInspector] public string NameID = "";
        /// <summary> Mostly used for header title for search bar </summary>
        [HideInInspector][SerializeField] protected bool wasCreated = false;

        /// <summary>
        /// You can call it when you create node on any graph
        /// </summary>
        public virtual void OnCreated() { wasCreated = true; }

        /// <summary> Refreshing node position changes </summary>
        public virtual void OnEndDrag()
        {
#if UNITY_EDITOR
            if (baseSerializedObject == null) return;
            baseSerializedObject.ApplyModifiedProperties();
            baseSerializedObject.Update();
#endif
        }


        public virtual void OnCursorExitNode()
        {

            #region Just disabling default wires draw on node cursor exit

            for (int p = 0; p < inputPorts.Count; p++)
            {
                NodePortBase prt = inputPorts[p] as NodePortBase;
                if (prt != null) prt._EditorForceDrawDefaultWires = false;
            }

            for (int p = 0; p < outputPorts.Count; p++)
            {
                NodePortBase prt = outputPorts[p] as NodePortBase;
                if (prt != null) prt._EditorForceDrawDefaultWires = false;
            }

#if UNITY_EDITOR
            _editorForceDrawDefaultTriggerWires = false;
#endif

            #endregion

        }

        public virtual void OnCursorEnterOnNode()
        {
        }


        /// <summary> Can be used for displaying debugging progress on node from 0 to 1 for example </summary>
        public float DebuggingProgress { get; set; } = -1f;

        [HideInInspector] /*[SerializeReference]*/ public List<FGraph_TriggerNodeConnection> OutputConnections = new List<FGraph_TriggerNodeConnection>();
        [HideInInspector] /*[SerializeReference]*/ public List<FGraph_TriggerNodeConnection> InputConnections = new List<FGraph_TriggerNodeConnection>();

        public FGraph_NodeBase FirstOutputConnection
        {
            get
            {
                if (OutputConnections.Count < 1) return null;
                if (AllowedOutputConnectionIndex < -1) return null;
                if (AllowedOutputConnectionIndex == -1) return OutputConnections[0].GetOther(this);
                //if (OutputConnections[0] == null) return null;

                if (AllowedOutputConnectionIndex >= OutputConnections.Count)
                {
                    for (int i = 0; i < OutputConnections.Count; i++)
                    {
                        if (OutputConnections[i].ConnectionFrom_AlternativeID == AllowedOutputConnectionIndex)
                            return OutputConnections[i].GetOther(this);
                    }
                }

                if (AllowedOutputConnectionIndex < OutputConnections.Count)
                    return OutputConnections[AllowedOutputConnectionIndex].GetOther(this);

                else
                    return null;
            }
        }

        /// <summary> If you want to call only on one of the outputs, return value greater than -1 </summary>
        public virtual int AllowedOutputConnectionIndex { get { return -1; } }

        /// <summary> Used just for auto-connecting trigger nodes </summary>
        public virtual int HotOutputConnectionIndex { get { return -1; } }

        /// <summary> By default is false </summary>
        public virtual bool DrawInputConnector { get { return false; } }
        /// <summary> By default is true </summary>
        public virtual bool DrawOutputConnector { get { return true; } }


        #region Multiple connectors to support custom implementations

        /// <summary> Used only with custom implementations for multiple connectors </summary>
        public virtual int InputConnectorsCount { get { return 1; } }
        /// <summary> Used only with custom implementations for multiple connectors </summary>
        public virtual string GetInputHelperText(int outputId = 0) { return ""; }
        /// <summary> Used only with custom implementations for multiple connectors </summary>
        public virtual int OutputConnectorsCount { get { return 1; } }
        /// <summary> Used only with custom implementations for multiple connectors </summary>
        public virtual string GetOutputHelperText(int outputId = 0) { return ""; }

        #endregion


        #region Node Related (Graph+Playmode)

        /// <summary> (base is empty) Triggered after adding node to graph display </summary>
        public virtual void RefreshNodeParams() { }

        /// <summary> Clear connections of other ports with this node </summary>
        public virtual void OnRemoving()
        {
            MarkConnnectedNodesForRefresh(inputPorts);
            MarkConnnectedNodesForRefresh(outputPorts);
        }

        /// <summary> Marking connected nodes for ports refresh </summary>
        void MarkConnnectedNodesForRefresh(List<IFGraphPort> ports)
        {
            for (int i = 0; i < ports.Count; i++)
            {
                NodePortBase port = NodePortBase.ToNodePortBase(ports[i]);
                if (port == null) continue;

                for (int c = 0; c < port.Connections.Count; c++)
                {
                    var conn = port.Connections[c];
                    if (conn.NodeReference) conn.NodeReference.forceRefreshPorts = true;
                }
            }
        }

        /// <summary> Connected nodes using ports / connectors </summary>
        //public List<FGraph_NodeBase> GetConnectedNodes()
        //{
        //    List<FGraph_NodeBase> nodes = new List<FGraph_NodeBase>();
        //    for (int i = 0; i < inputPorts.Count; i++)
        //    {
        //        NodePortBase port = NodePortBase.ToNodePortBase( inputPorts[i]);
        //        if (port == null) continue;

        //    }

        //}

        #endregion


        #region Editor Draw Node Bases

        public Rect _E_LatestRect { get; set; }
        public virtual Vector2 NodeSize { get { return new Vector2(200, 100); } }

        [HideInInspector] public Vector2 NodePosition = Vector2.zero;
        [HideInInspector] public Vector2 NodeDrawOffset = new Vector2(0, 0);

        #endregion


        #region Styling Utilities


        public static void CheckForNulls<T>(List<T> classes)
        {
            for (int i = classes.Count - 1; i >= 0; i--)
            {
                if (classes[i] == null) classes.RemoveAt(i);
            }
        }

        public virtual string GetNodeSubName { get { return NameID; } }
        public virtual Texture GetNodeIcon { get { return null; } }
        public virtual string GetNodeTooltipDescription { get { return string.Empty; } }

        /// <summary> By default is false </summary>
        public virtual bool IsFoldable { get { return false; } }

        /// <summary> Drawing resize handle in right bottom corner and implementing resizing feature </summary>
        public virtual bool IsResizable { get { return false; } }
        /// <summary> Resize scale of node using IsResizable handle </summary>
        [HideInInspector] public Vector2 ResizedScale = new Vector2(200, 140);
        /// <summary> Different handling inputs for the node if is containable </summary>
        public virtual bool IsContainable { get { return false; } }

#if UNITY_EDITOR
        /// <summary> Containable node is holding reference data about nodes inside it </summary>
        [HideInInspector] public List<FGraph_NodeBase> _EditorInContainedRange = new List<FGraph_NodeBase>();
        /// <summary> If node is contained inside some containable node </summary>
        [HideInInspector] public FGraph_NodeBase IsContainedBy = null;
        /// <summary> If node was drawn on the graph or was culled / hidden by some other node </summary>
        [NonSerialized] public bool _EditorWasDrawn = false;
#endif

        /// <summary> Used on comment node, for some reason unity is not saving some serialization changes when folding/unfolding  </summary>
        public virtual bool IsFoldableFix { get { return false; } }
        /// <summary> Helper variable for editor use </summary>
        [HideInInspector]/*[NonSerialized]*/ public bool _EditorFoldout = false;

        public bool IsDrawingGUIInNodeMode { get; set; }

        public virtual Color GetNodeColor() { return Color.white; }
        /// <summary> Executed when node is started to be draw by graph drawer </summary>
        public virtual void OnCreatedDrawer() { RefreshNodeParams(); }
        public virtual string GetDisplayName(float maxWidth = 120)
        {
#if UNITY_EDITOR
            Vector2 size = UnityEditor.EditorStyles.largeLabel.CalcSize(new GUIContent(NameID));
            string dName = NameID;

            if (dName.Length > 2)
                if (size.x > maxWidth)
                    dName = dName.Substring(0, Mathf.Min(11, dName.Length - 1)) + "...";

            return dName;
#else
            return NameID;
#endif
        }


        #endregion


        #region ID Handling

        public virtual void GenerateID(List<FGraph_NodeBase> allNodes)
        {
#if UNITY_EDITOR
            if (baseSerializedObject.targetObject == null) return;
            baseSerializedObject.Update();
#endif

            int targetId = int.MinValue + allNodes.Count;
            bool unique = false;

            while (!unique)
            {
                if (targetId == -1) targetId = 0;

                bool found = false;
                for (int i = 0; i < allNodes.Count; i++)
                {
                    if ((allNodes[i] is null)) continue;
                    if (allNodes[i].IndividualID == targetId) { found = true; break; }
                }

                if (found == false) { unique = true; break; }
                targetId += 1;
            }

            IndividualID = targetId;

#if UNITY_EDITOR
            RefreshPorts();
            baseSerializedObject.ApplyModifiedProperties();
#endif
            _E_SetDirty();
        }



        #endregion


    }
}
