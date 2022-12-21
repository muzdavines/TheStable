using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldSelector : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Field" : "Get Field Planner from Selector"; }
        public override string GetNodeTooltipDescription { get { return "Getting choosed field index number or unfold to get field port"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 175 : 165, _EditorFoldout ? (DrawInstInd ? 124 : 104) : (84)); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Output, true)] public IntPort PlannerID;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Planner;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public IntPort InstanceID;

        bool DrawInstInd { get { return Planner.PortState() == EPortPinState.Connected; } }
        public override void OnStartReadingNode()
        {
            InstanceID.TriggerReadPort(true);
            int instId = 0;

            if (Planner.PortState() == EPortPinState.Connected)
            {
                instId = InstanceID.GetInputValue;
                if (instId < 0) instId = 0;

                if ( CurrentExecutingPlanner )
                    if ( CurrentExecutingPlanner.ParentBuildPlanner)
                        if ( CurrentExecutingPlanner.ParentBuildPlanner.BasePlanners.ContainsIndex(PlannerID.Value))
                        {
                            if ( instId >= CurrentExecutingPlanner.ParentBuildPlanner.BasePlanners[PlannerID.Value].Instances)
                            {
                                instId = CurrentExecutingPlanner.ParentBuildPlanner.BasePlanners[PlannerID.Value].Instances-1;
                            }
                        }
            }

            Planner.UniquePlannerID = PlannerID.Value;
            Planner.DuplicatePlannerID = instId - 1;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (ParentPlanner != null)
            {
                GUILayout.Space(-20);
                UnityEditor.EditorGUI.BeginChangeCheck();
                PlannerID.Value = EditorGUILayout.IntPopup(PlannerID.Value, ParentPlanner.GetPlannersNameList(), ParentPlanner.GetPlannersIDList(), GUILayout.Width(NodeSize.x - 80));
                if (UnityEditor.EditorGUI.EndChangeCheck()) _editorForceChanged = true;
            }

            InstanceID.AllowDragWire = false;
            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Planner");
                EditorGUILayout.PropertyField(sp, true);

                if (DrawInstInd)
                {
                    SerializedProperty spc = sp.Copy();
                    spc.Next(false);
                    EditorGUILayout.PropertyField(spc);
                    InstanceID.AllowDragWire = true;
                }
            }

        }


        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Planner.UniquePlannerID: " + Planner.UniquePlannerID);
            GUILayout.Label("Planner.DuplicatePlannerID: " + Planner.DuplicatePlannerID);
        }

#endif

    }
}