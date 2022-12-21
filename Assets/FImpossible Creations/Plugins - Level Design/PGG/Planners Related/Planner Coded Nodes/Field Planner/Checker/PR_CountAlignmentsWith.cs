using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Checker
{

    public class PR_CountAlignmentsWith : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Count Alignment Cells" : "Count Fields Alignment Cells"; }
        public override string GetNodeTooltipDescription { get { return "Counting how many cells are next to each other"; } }
        public override Color GetNodeColor() { return new Color(0.07f, 0.66f, 0.56f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(230, _EditorFoldout ? 126 : 102); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort AlignmentsWith;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue, 1)] [Tooltip("If collision occured then true, if no then false")] public IntPort Alignments;
        [HideInInspector] [Port(EPortPinType.Input, 1)] [Tooltip("Using self if no input")] public PGGPlannerPort FirstField;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void OnStartReadingNode()
        {
            FieldPlanner aPlanner = GetPlannerFromPort(FirstField);
            FieldPlanner bPlanner = GetPlannerFromPort(AlignmentsWith);

            if (aPlanner == null) return;
            if (aPlanner.LatestChecker == null) return;
            if (bPlanner == null) return;
            if (bPlanner.LatestChecker == null) return;

            Alignments.Value = 0;
            Alignments.Value = aPlanner.LatestChecker.CountAlignmentsWith(bPlanner.LatestChecker);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (!_EditorFoldout) return;

            if (_EditorFoldout)
            {
                FirstField.AllowDragWire = true;
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("FirstField");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc);
                baseSerializedObject.ApplyModifiedProperties();
            }
            else
            {
                FirstField.AllowDragWire = false;
            }

        }
#endif

    }
}