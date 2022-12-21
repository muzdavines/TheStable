using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldInstance : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Field Instance" : "Get Field Instance by Iteration"; }
        public override string GetNodeTooltipDescription { get { return "Getting field planner instance by iteration number. 0 is first planner, 1 is instance of first planner (let's say first planner instances count = 2) 2 is second planner, 3 is second planner instance etc."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(210, 104); } }
        public override bool IsFoldable { get { return false; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, 1)] public IntPort Iteration;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Planner;

        bool DrawInstInd { get { return Planner.PortState() == EPortPinState.Connected; } }
        public override void OnStartReadingNode()
        {
            Iteration.TriggerReadPort(true);
            int instId = 0;

            if (Planner.PortState() == EPortPinState.Connected)
            {
                instId = Iteration.GetInputValue;
                if (instId < 0) instId = 0;
            }

            if (CurrentExecutingPlanner == null) return;
            if (CurrentExecutingPlanner.ParentBuildPlanner == null) return;

            FieldPlanner p = CurrentExecutingPlanner.ParentBuildPlanner.GetPlannerByIteration(instId);
            Planner.SetIDsOfPlanner(p);
        }

#if UNITY_EDITOR
        //SerializedProperty sp = null;
        //public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        //{
        //    base.Editor_OnNodeBodyGUI(setup);

        //    Iteration.AllowDragWire = false;
        //    if (_EditorFoldout)
        //    {
        //        if (sp == null) sp = baseSerializedObject.FindProperty("Planner");
        //        EditorGUILayout.PropertyField(sp, true);

        //        if (DrawInstInd)
        //        {
        //            SerializedProperty spc = sp.Copy();
        //            spc.Next(false);
        //            EditorGUILayout.PropertyField(spc);
        //            Iteration.AllowDragWire = true;
        //        }
        //    }

        //}

#endif

    }
}