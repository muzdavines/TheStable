#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Graph;
using System;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules.QuickSolutions;
using FIMSpace.Generating.Checker;

namespace FIMSpace.Generating.Planning.PlannerNodes
{

    /// <summary>
    /// It's always sub-asset -> it's never project file asset
    /// </summary>
    public abstract partial class PlannerRuleBase : PGGPlanner_NodeBase
    {
        public static bool Debugging = false;

        /// <summary> Warning! Duplicate is refering to root project planner, In the nodes logics you can use CurrentExecutingPlanner for instance planner reference </summary>
        [HideInInspector] public FieldPlanner ParentPlanner;
        public FieldPlanner CurrentExecutingPlanner { get { return FieldPlanner.CurrentGraphExecutingPlanner; } }

        [HideInInspector] public ScriptableObject ParentNodesContainer;

        public string DebuggingInfo { get; protected set; }
        public Action DebuggingGizmoEvent { get; protected set; }

        //public virtual string TitleName() { return GetType().Name; }
        public virtual string Tooltip() { string tooltipHelp = "(" + GetType().Name; return tooltipHelp + ")"; }

        public override Vector2 NodeSize { get { return new Vector2(232, 90); } }
        /// <summary> PlannerRuleBase by default is true </summary>
        public override bool DrawInputConnector { get { return true; } }

        public bool GetPlannerPort_IsContainingMultiple(PGGPlannerPort port)
        {
            if (port.ContainsMultiplePlanners) return true;
            return false;
        }

        /// <summary>
        /// You can use port.ContainsMultiplePlanners to identify multiple planners port transport
        /// </summary>
        private static List<FieldPlanner> _multiplePlannersContainer = new List<FieldPlanner>();
        public List<FieldPlanner> GetPlannersFromPort(PGGPlannerPort port, bool nullIfNoMultiple = false, bool callRead = true, bool newListInstance = true)
        {
            List<FieldPlanner> list;
            if (newListInstance) list = new List<FieldPlanner>();
            else
            {
                _multiplePlannersContainer.Clear();
                list = _multiplePlannersContainer;
            }

            for (int c = 0; c < port.Connections.Count; c++)
            {
                var conn = port.Connections[c];

                PGGPlannerPort comm = null;
                if (conn != null)
                {
                    if (conn.PortReference != null)
                    {
                        if (conn.PortReference == port) UnityEngine.Debug.Log("(Same port - it's wrong)");
                        else
                        {
                            if (conn.PortReference is PGGPlannerPort) comm = conn.PortReference as PGGPlannerPort;
                        }
                    }
                }

                if (comm != null)
                {
                    if (comm.ContainsMultiplePlanners)
                    {
                        for (int p = 0; p < comm.PlannersList.Count; p++)
                        {
                            if (comm.PlannersList[p].Available)
                                list.Add(comm.PlannersList[p]);
                        }

                        var pl = GetPlannerFromPort(port, callRead);
                        if (pl.Available)
                            if (!list.Contains(pl))
                            {
                                list.Add(pl);
                            }
                    }
                    else
                    {
                        var pl = GetPlannerFromPort(port, callRead);
                        if (pl)
                            if (pl.Available)
                                list.Add(pl);
                    }
                }
                else
                {
                    var pl = GetPlannerFromPort(port, callRead);
                    if (pl)
                        if (pl.Available)
                            list.Add(pl);
                }
            }

            if (port.Connections.Count == 0)
            {
                var pl = GetPlannerFromPort(port, callRead);
                if (pl) if (pl.Available) list.Add(pl);
            }

            port.AssignPlannersList(list);
            if (list.Count > 0) return list;

            if (nullIfNoMultiple)
                return null;
            else
                return list;
        }

        public static FieldPlanner GetPlannerFromPortS(PGGPlannerPort port, bool callRead = true)
        {
            if (callRead) port.GetPortValueCall();
            int plannerId = port.GetPlannerIndex();
            int duplicateId = port.GetPlannerDuplicateIndex();

            FieldPlanner portPlanner = port.GetPlannerFromPort();
            if (portPlanner != null) return portPlanner;

            return GetFieldPlannerByID(plannerId, duplicateId);
        }


        public FieldPlanner GetPlannerFromPort(PGGPlannerPort port, bool callRead = true)
        {
            if (callRead) port.TriggerReadPort(true);

            FieldPlanner portPlanner = port.GetPlannerFromPort();
            if (portPlanner != null)
            {
                return portPlanner;
            }

            int plannerId = port.GetPlannerIndex();
            int duplicateId = port.GetPlannerDuplicateIndex();

            return GetPlannerByID(plannerId, duplicateId);
        }


        public CheckerField3D GetCheckerFromPort(PGGPlannerPort port, bool callRead = true)
        {
            if (callRead) port.TriggerReadPort(true);

            CheckerField3D portPlanner = port.GetInputCheckerSafe;
            if (portPlanner != null)
            {
                return portPlanner;
            }

            FieldPlanner planner = GetPlannerFromPort(port, callRead);
            if (planner == null) return null;
            return planner.LatestChecker;
        }

        public FieldPlanner GetPlannerByID(int plannerId, int duplicateId = -1)
        {
            FieldPlanner planner = GetFieldPlannerByID(plannerId, duplicateId);
            if (planner == null) return ParentPlanner;
            return planner;
        }

        public static bool _debug = false;
        public static FieldPlanner GetFieldPlannerByID(int plannerId, int duplicateId = -1)
        {

            FieldPlanner planner = FieldPlanner.CurrentGraphExecutingPlanner;
            if (planner == null) { planner = null; }
            BuildPlannerPreset build = null;
            if (planner != null) build = planner.ParentBuildPlanner;

            if (build == null)
            {
                return null;
            }

            if (plannerId >= 0 && plannerId < build.BasePlanners.Count)
            {
                planner = build.BasePlanners[plannerId];

                if (planner.IsDuplicate == false) if (duplicateId >= 0) if (planner.GetDuplicatesPlannersList() != null)
                        {
                            planner = planner.GetDuplicatesPlannersList()[duplicateId];
                        }
            }

            if (planner.Discarded)
            {
                FieldPlanner getPl = planner;

                if (duplicateId == -1) // if discarded then get first not discarded duplicate planner
                {
                    if (planner.IsDuplicate == false)
                        for (int i = 0; i < planner.GetDuplicatesPlannersList().Count; i++)
                        {
                            var plan = planner.GetDuplicatesPlannersList()[i];
                            if (plan == null) continue;
                            if (plan.Available == false) continue;
                            getPl = plan;
                            break;
                        }
                }

                return getPl;
            }

            return planner;
        }

        /// <summary> [Base is empty] </summary>
        public virtual void PreGeneratePrepare()
        {

        }

        /// <summary> [Base is not empty] Preparing initial debug message </summary>
        public virtual void Prepare(PlanGenerationPrint print)
        {
#if UNITY_EDITOR
            DebuggingInfo = "Debug Info not Assigned";
#endif
        }

        /// <summary> [Base is empty] </summary>
        public virtual void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            // Node Procedures Code
        }


        protected void CallOtherExecution(FGraph_NodeBase otherNode, PlanGenerationPrint print)
        {
            if (otherNode == null) return;

            if (ParentPlanner == null)
            {
                if (print == null)
                {
                    if (otherNode is PlannerRuleBase)
                        MG_ModGraph.CallExecution(otherNode as PlannerRuleBase);
                }

                return;
            }

            if (otherNode is PlannerRuleBase)
                ParentPlanner.CallExecution(otherNode as PlannerRuleBase, print);
        }

        protected void CallOtherExecutionWithConnector(int altId, PlanGenerationPrint print)
        {
            for (int c = 0; c < OutputConnections.Count; c++)
            {
                if (OutputConnections[c].ConnectionFrom_AlternativeID == altId)
                {
                    CallOtherExecution(OutputConnections[c].GetOther(this), print);
                }
            }
        }


        #region Editor related


#if UNITY_EDITOR

        public virtual void OnGUIModify()
        {

        }

        [HideInInspector]
        public bool _editor_drawRule = true;
        protected UnityEditor.SerializedObject inspectorViewSO = null;

        protected virtual void DrawGUIHeader(int i)
        {
            if (inspectorViewSO == null) inspectorViewSO = new UnityEditor.SerializedObject(this);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxLightStyle, GUILayout.Height(20)); // 1

            Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));


            string foldout = FGUI_Resources.GetFoldSimbol(_editor_drawRule);
            string tip = Tooltip();


            if (GUILayout.Button(new GUIContent(foldout + "  " + GetDisplayName() + "  " + foldout, tip), FGUI_Resources.HeaderStyle))
            {
                bool rmb = false;
                if (rmb == false) _editor_drawRule = !_editor_drawRule;
            }

            int hh = 18;

            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(ParentPlanner.FProcedures, i, i - 1); return; }
            if (i < ParentPlanner.FProcedures.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(ParentPlanner.FProcedures, i, i + 1); return; }

            if (GUILayout.Button("X", FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(hh)))
            {
                ParentPlanner.RemoveRuleFromPlanner(this);
                return;
            }

            EditorGUILayout.EndHorizontal(); // 1
        }

        protected virtual void DrawGUIFooter()
        {
            EditorGUILayout.EndVertical();

            if (inspectorViewSO.ApplyModifiedProperties())
            {
                OnStartReadingNode();
            }
        }


        //public void DrawGUIStack(int i)
        //{
        //    DrawGUIHeader(i);

        //    Color preColor = GUI.color;

        //    if (inspectorViewSO != null)
        //        if (inspectorViewSO.targetObject != null)
        //            if (_editor_drawRule)
        //            {
        //                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
        //                if (Enabled == false) GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.7f);
        //                inspectorViewSO.Update();
        //                DrawGUIBody();
        //                DrawGUIFooter();
        //            }

        //    GUI.color = preColor;
        //}

        /// <summary>
        /// Returns true if something changed in GUI - using EditorGUI.BeginChangeCheck();
        /// </summary>
        //protected virtual void DrawGUIBody(/*int i*/)
        //{
        //    UnityEditor.SerializedProperty sp = inspectorViewSO.GetIterator();
        //    sp.Next(true);
        //    sp.NextVisible(false);
        //    bool can = sp.NextVisible(false);

        //    if (can)
        //    {
        //        do
        //        {
        //            bool cont = false;
        //            if (cont) continue;

        //            UnityEditor.EditorGUILayout.PropertyField(sp);
        //        }
        //        while (sp.NextVisible(false) == true);
        //    }

        //    //    EditorGUILayout.EndVertical();

        //    //    so.ApplyModifiedProperties();
        //    //}


        //}

#endif

        #endregion


        #region Mod Graph Related

        /// <summary>  Current executing mod graph (for field modification graph) </summary>
        public SR_ModGraph MG_ModGraph { get { return SR_ModGraph.Graph_ModGraph; } }
        /// <summary> Current executing field mod (for field modification graph) </summary>
        public FieldSpawner MG_Spawner { get { return SR_ModGraph.Graph_Spawner; } }
        /// <summary> Current executing field modificator's spawner </summary>
        public FieldModification MG_Mod { get { return SR_ModGraph.Graph_Mod; } }
        /// <summary> Current executing mod spawner's spawn (for field modification graph) </summary>
        public SpawnData MG_Spawn { get { return SR_ModGraph.Graph_SpawnData; } }
        /// <summary> Current executing field setup preset (for field modification graph) </summary>
        public FieldSetup MG_Preset { get { return SR_ModGraph.Graph_Preset; } }
        /// <summary> Current executing field grid cell (for field modification graph) </summary>
        public FieldCell MG_Cell { get { return SR_ModGraph.Graph_Cell; } }
        /// <summary> Current executing field gridd (for field modification graph) </summary>
        public FGenGraph<FieldCell, FGenPoint> MG_Grid { get { return SR_ModGraph.Graph_Grid; } }
        ///// <summary> Current executing field mod (for field modification graph) </summary>
        //public Vector3? Graph_RestrictDir { get { return SR_ModGraph.Graph_Mod; } }
        

        public ModificatorsPack MGGetParentPack()
        {
            SR_ModGraph owner = ParentNodesContainer as SR_ModGraph;
            if (owner == null) return null;
            return owner.TryGetParentModPack();
        }

        public UnityEngine.Object MGGetFieldSetup()
        {
            SR_ModGraph owner = ParentNodesContainer as SR_ModGraph;
            if (owner == null) return null;

            var fs = owner.TryGetParentFieldSetup();
            if (fs == null) return null;

            //if (fs)
            //{
            //    if (fs.InstantiatedOutOf) return fs.InstantiatedOutOf;
            //}

            return fs;
        }

        protected List<FieldVariable> MGGetVariables(UnityEngine.Object tgt)
        {
            if (tgt == null) return null;

            FieldSetup fs = tgt as FieldSetup;
            if (fs)
            {
                return fs.Variables;
            }

            ModificatorsPack mp = tgt as ModificatorsPack;
            if (mp)
            {
                return mp.Variables;
            }

            return null;
        }

        protected FieldVariable MGGetVariable(UnityEngine.Object tgt, int index)
        {
            var variables = MGGetVariables(tgt);
            if (variables == null) return null;
            if (variables.ContainsIndex(index)) return variables[index];
            return null;
        }


        #endregion
    }
}
