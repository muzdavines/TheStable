using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Special
{

    public class PR_AlignTo : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Align Self To"; }
        public override string GetNodeTooltipDescription { get { return "Finding alignment position on 'AlignTo' field, algorithm is checking multiple placement and choosing one which results in smallest bounds of 'AlignTo' and own field bounds."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        public override Vector2 NodeSize { get { return new Vector2(222, _EditorFoldout ? 144 : 104); } }
        public override bool IsFoldable { get { return true; } }
        public override Color GetNodeColor() { return new Color(0.1f, 0.7f, 1f, 0.95f); }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort AlignTo;
        [Port(EPortPinType.Input, 1)] public IntPort WantAlignPoints;

        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGPlannerPort ToMove;
        //[HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort ContactCell;
        //[HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort AlignedToCell;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            FieldPlanner selfPlanner = GetPlannerFromPort(ToMove);
            if (selfPlanner == null) return;

            FieldPlanner targetPlanner = GetPlannerFromPort(AlignTo);

            if (targetPlanner != null)
            {
                int alignPoints = WantAlignPoints.GetInputValue;
                if (alignPoints < 1) alignPoints = 1;

                var oChecker = targetPlanner.LatestChecker;

                if (selfPlanner.LatestChecker.IsCollidingWith(oChecker))
                {
                    selfPlanner.LatestChecker.PushOutAway(oChecker, true);
                }

                selfPlanner.LatestChecker.AlignTo(oChecker, alignPoints);
            }

            if (Debugging)
            {
                if (targetPlanner == null) DebuggingInfo = "Not found target to align or trying aligning to self, '" + AlignTo.ToString() + ".";
                else
                    DebuggingInfo = "Found target to align and moved checker to " + targetPlanner.name + " [" + AlignTo.GetPlannerIndex() + "] [" + AlignTo.GetPlannerDuplicateIndex() + "]";
            }
        }

#if UNITY_EDITOR

        private UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            if (WantAlignPoints.Value < 1) WantAlignPoints.Value = 1;

            if (_EditorFoldout)
            {
                GUILayout.Space(1);

                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("ToMove");
                UnityEditor.SerializedProperty scp = sp.Copy();
                UnityEditor.EditorGUILayout.PropertyField(scp); 
                //scp.Next(false); UnityEditor.EditorGUILayout.PropertyField(scp);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

#endif

    }
}