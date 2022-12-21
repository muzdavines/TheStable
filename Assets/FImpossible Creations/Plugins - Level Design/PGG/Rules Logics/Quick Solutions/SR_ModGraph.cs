#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules.ModGraph.Nodes;
using System;
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Graph;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_ModGraph : SpawnRuleBase, ISpawnProcedureType, IPlanNodesContainer
    {
        public override string TitleName() { return "Mod Node Graph"; }
        public override string Tooltip() { return " Using node graph to call some logics and changes on the spawns\n" + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Coded; } }
        internal override bool AllowDuplicate()
        {
            return false;
        }

        public override bool CanBeGlobal() { return false; }
        [HideInInspector, SerializeField] public bool _editor_wasRenaming = false;

        /// <summary> If you want to call logics of some other mod graph saved in project files </summary>
        [HideInInspector] public SR_ModGraph ExternalModGraph = null;
        private SR_ModGraph CallMod { get { if (ExternalModGraph != null) return ExternalModGraph; else return this; } }


        #region Graph Interface

        [SerializeField, HideInInspector] public List<PGGPlanner_NodeBase> Nodes = new List<PGGPlanner_NodeBase>();
        [SerializeField, HideInInspector] private List<FieldVariable> _graphVariables = new List<FieldVariable>();
        [SerializeField, HideInInspector] private FieldPlanner.LocalVariables _graphLocalVariables;
        public List<PGGPlanner_NodeBase> Procedures { get { return Nodes; } }
        public List<PGGPlanner_NodeBase> PostProcedures { get { return Nodes; } }
        public List<FieldVariable> Variables { get { return Variables; } }
        public ScriptableObject ScrObj { get { return this; } }
        public FieldPlanner.LocalVariables GraphLocalVariables { get { return _graphLocalVariables; } }



        public PE_Start ProceduresBegin { get { return proceduresBegin; } }

        [SerializeField, HideInInspector] private PE_Start proceduresBegin;




        #endregion

        #region Drawing Inspector Window

#if UNITY_EDITOR

        [NonSerialized] public bool _editor_DrawGraph = false;
        private SerializedProperty sp_CallDuring = null;
        public override void NodeBody(SerializedObject so)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(22));
            EditorGUILayout.LabelField("Call logics with more freedom using node graph", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(22));
            Color preC = GUI.color;

            GUI.color = new Color(1f, 1f, 1f, hideFlags == HideFlags.HideInHierarchy ? 0.4f : 1f);
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_AB, "Switch exposing visibility of the asset for search by 'Call Rules Logics Of' node."), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(22)))
            {
                if (hideFlags == HideFlags.HideInHierarchy) hideFlags = HideFlags.None;
                else hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.SaveAssets();
            }
            GUI.color = preC;
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Display helper name change dialog"), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(22)))
            {
                string preName = name;
                FGenerators.RenamePopup(this, name, false);

                if (name != preName)
                {
                    _editor_wasRenaming = true;
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.EndHorizontal();
            //EditorGUILayout.HelpBox(" Call logics with more freedom using node graph", MessageType.Info);
            GUILayout.Space(5);

            if (sp_CallDuring == null) sp_CallDuring = so.FindProperty("CallDuring");
            EditorGUILayout.PropertyField(sp_CallDuring);
            GUILayout.Space(3);

            base.NodeBody(so);

            // Code for drawing buttons to open node graph are under Field Modification Editor class
            // to be able to call editor code for graph
        }

#endif

        #endregion


        public enum ECallGraphOn
        {
            [Tooltip("Executed before spawning allow.")]
            OnChecking,
            [Tooltip("Executed after initial checking rules to allow spawning. If you want to break spawning, OnInfluence it will not work!")]
            OnInfluence
        }

        [HideInInspector]
        public ECallGraphOn CallDuring = ECallGraphOn.OnChecking;



        public override void Refresh()
        {
            if (ExternalModGraph == this) ExternalModGraph = null;
            base.Refresh();
            CellAllow = true;
        }

        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            if (ExternalModGraph == this) ExternalModGraph = null;

            base.PreGenerateResetRule(grid, preset, callFrom);

            if (Nodes != null)
                for (int i = 0; i < Nodes.Count; i++)
                {
                    PGGPlanner_NodeBase node = Nodes[i];
                    if (node == null) continue;
                    PlannerRuleBase noder = node as PlannerRuleBase;
                    if (noder == null) continue;
                    noder.ParentNodesContainer = this;
                }

            CallMod.ModGraphPreGenerateCall();
        }

        private void ModGraphPreGenerateCall()
        {
            PGGUtils.CheckForNulls(Procedures);

            for (int i = 0; i < Procedures.Count; i++)
            {
                if (Procedures[i].Enabled == false) continue;
                Procedures[i].ToRB().PreGeneratePrepare();
            }

            FGraph_RunHandler.RefreshConnections(Procedures);
        }

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            if (CallDuring == ECallGraphOn.OnChecking)
            {
                SetGraphParams(mod, ref spawn, OwnerSpawner, preset, cell, grid, restrictDirection);
                CallMod.ModGraphCheckRules();
                if (Graph_SpawnData != null) spawn = Graph_SpawnData;
            }
        }

        void ModGraphCheckRules()
        {
            if (proceduresBegin) CallExecution(proceduresBegin);
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CellInfluence(preset, mod, cell, ref spawn, grid);

            if (CallDuring == ECallGraphOn.OnInfluence)
            {
                SetGraphParams(mod, ref spawn, OwnerSpawner, preset, cell, grid, restrictDirection);
                CallMod.ModGraphCheckRules();
                if (Graph_SpawnData != null) spawn = Graph_SpawnData;
            }
            //SetGraphParams(mod, ref spawn, preset, cell, grid, null);
            //if (proceduresBegin) CallExecution(proceduresBegin);
            //if (Graph_SpawnData != null) spawn = Graph_SpawnData;
        }


        #region Graph Logics Related


        void SetGraphParams(FieldModification mod, ref SpawnData spawn, FieldSpawner spawner, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Graph_Mod = mod;
            Graph_Spawner = spawner;
            Graph_SpawnData = spawn;
            Graph_Preset = preset;
            Graph_Cell = cell;
            Graph_Grid = grid;
            Graph_RestrictDir = restrictDirection;
            Graph_ModGraph = this;
        }

        public static SR_ModGraph Graph_ModGraph;
        public static FieldSpawner Graph_Spawner;
        public static FieldModification Graph_Mod;
        public static SpawnData Graph_SpawnData;
        public static FieldSetup Graph_Preset;
        public static FieldCell Graph_Cell;
        public static FGenGraph<FieldCell, FGenPoint> Graph_Grid;
        public static Vector3? Graph_RestrictDir;


        internal void CallExecution(PlannerRuleBase rule)
        {
            rule.Execute(null, null);
            if (rule.FirstOutputConnection == null) return;

            if (rule.AllowedOutputConnectionIndex > -1)
            {
                for (int c = 0; c < rule.OutputConnections.Count; c++)
                {
                    if (rule.OutputConnections[c].ConnectionFrom_AlternativeID != rule.AllowedOutputConnectionIndex) continue;
                    CallExecution(rule.OutputConnections[c].GetOther(rule) as PlannerRuleBase);
                }
            }
            else
            {
                for (int c = 0; c < rule.OutputConnections.Count; c++)
                {
                    CallExecution(rule.OutputConnections[c].GetOther(rule) as PlannerRuleBase);
                }
            }
        }


        #endregion


        #region Editor Related
#if UNITY_EDITOR

        internal void RemoveNodeFromGraph(ModNodeBase modNodeBase)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.Log("[PGG] Not allowed removing rules in playmode");
                return;
            }

            PGGPlanner_NodeBase plBase = (PGGPlanner_NodeBase)modNodeBase;

            Procedures.Remove(plBase);
            DestroyImmediate(modNodeBase, true);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void OnNodeRemove(FGraph_NodeBase node)
        {
            DestroyImmediate(node, true);
            EditorUtility.SetDirty(this);
        }


        public void AddRuleToPlanner(PlannerRuleBase rule)
        {
            //rule.ParentPlanner = this;
            rule.hideFlags = HideFlags.HideInHierarchy;

            Nodes.Add(rule);

            FGenerators.AddScriptableTo(rule, this, false, false);
            rule.ParentNodesContainer = this;
            EditorUtility.SetDirty(rule);
            EditorUtility.SetDirty(this);
        }

        public void RefreshStartGraphNodes()
        {
            //proceduresBegin = null;

            for (int p = 0; p < Procedures.Count; p++)
            {
                if (Procedures[p] is PE_Start)
                {
                    proceduresBegin = Procedures[p] as PE_Start;
                    break;
                }
            }

        }

#endif
        #endregion


    }

}