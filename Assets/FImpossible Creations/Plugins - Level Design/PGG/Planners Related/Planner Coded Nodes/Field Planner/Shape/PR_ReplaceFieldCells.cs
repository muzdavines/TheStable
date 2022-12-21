using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Shape
{

    public class PR_ReplaceFieldCells : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Replace Field Cells" : "Replace Field Cells"; }
        public override bool IsFoldable { get { return true; } }
        public override string GetNodeTooltipDescription { get { return "Replacing cells of one Field Shape with another."; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }

        public override Vector2 NodeSize { get { return new Vector2(214, _EditorFoldout ? 104 : 86); } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort ReplaceWith;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGPlannerPort ApplyTo;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            ApplyTo.TriggerReadPort(true);
            ReplaceWith.TriggerReadPort(true);

            FieldPlanner plan = GetPlannerFromPort(ApplyTo, false);
            CheckerField3D myChe = ApplyTo.GetInputCheckerSafe;
            if (plan) myChe = plan.LatestResult.Checker;
            if (myChe == null) { return; }

            CheckerField3D oChe = ReplaceWith.GetInputCheckerSafe;
            if (oChe == null) { return; }

            myChe.ClearAllCells();
            myChe.Join(oChe);

            if (plan) plan.LatestResult.Checker = myChe;
        }

#if UNITY_EDITOR
        private UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                ApplyTo.AllowDragWire = true;
                GUILayout.Space(1);

                if (sp == null) sp = baseSerializedObject.FindProperty("ApplyTo");
                UnityEditor.SerializedProperty scp = sp.Copy();
                UnityEditor.EditorGUILayout.PropertyField(scp);
            }
            else
            {
                ApplyTo.AllowDragWire = false;
            }
        }
#endif

    }
}