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

namespace FIMSpace.Generating.Rules.ModGraph.Nodes
{

    /// <summary>
    /// It's always sub-asset -> it's never project file asset
    /// </summary>
    public abstract partial class ModNodeBase : PGGPlanner_NodeBase
    {
        public static bool Debugging = false;

        /// <summary> Warning! Duplicate is refering to root project planner, In the nodes logics you can use CurrentExecutingPlanner for instance planner reference </summary>
        [HideInInspector] public SR_ModGraph ParentPlanner;
        [HideInInspector] public ScriptableObject ParentNodesContainer;
        public string DebuggingInfo { get; protected set; }
        public Action DebuggingGizmoEvent { get; protected set; }

        //public virtual string TitleName() { return GetType().Name; }
        public virtual string Tooltip() { string tooltipHelp = "(" + GetType().Name; return tooltipHelp + ")"; }


        public override Vector2 NodeSize { get { return new Vector2(232, 90); } }
        public override bool DrawInputConnector { get { return true; } }

        public bool GetPlannerPort_IsContainingMultiple(PGGPlannerPort port)
        {
            if (port.ContainsMultiplePlanners) return true;
            return false;
        }

        /// <summary> [Base is empty] </summary>
        public virtual void PreGeneratePrepare()
        {

        }

        /// <summary> [Base is not empty] Preparing initial debug message </summary>
        public virtual void Prepare()
        {
#if UNITY_EDITOR
            DebuggingInfo = "Debug Info not Assigned";
#endif
        }

        public virtual void Execute()
        {
            // Node Procedures Code
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

            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(ParentPlanner.Procedures, i, i - 1); return; }
            if (i < ParentPlanner.Procedures.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(ParentPlanner.Procedures, i, i + 1); return; }

            if (GUILayout.Button("X", FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(hh)))
            {
                ParentPlanner.RemoveNodeFromGraph(this);
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

#endif

        #endregion

    }
}
