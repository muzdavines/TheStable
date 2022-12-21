using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_AddCellToField : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Add Cell to Field" : "Add New Cell to the Field Grid"; }
        public override string GetNodeTooltipDescription { get { return "Adding new cell at position to the Field Grid."; } }
        public override Color GetNodeColor() { return new Color(0.125f, 0.9f, 0.3f, 0.9f); }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override bool IsFoldable { get { return true; } }

        public override Vector2 NodeSize { get { return new Vector2(unhideVector ? 270 : 200, _EditorFoldout ? 124 : 106); } }

        [Port(EPortPinType.Input, 1)] public PGGVector3Port CellPosition;
        [Tooltip("If the added position should be read like in field's local position,rotation and scale space\n0,0,0 -> center : 1,0,0 it's first self-rotation right cell")] public Cells.PR_GetCellPosition.ESpace AddCellSpace = Cells.PR_GetCellPosition.ESpace.WorldPosition;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGPlannerPort ApplyTo;
        bool unhideVector = false;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        public override void OnCreated()
        {
            base.OnCreated();
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            if (CellPosition.IsNotConnected) return;

            ApplyTo.TriggerReadPort(true);
            CellPosition.TriggerReadPort(true);

            FieldPlanner plan = GetPlannerFromPort(ApplyTo, false);
            CheckerField3D myChe = ApplyTo.GetInputCheckerSafe;
            if (plan) myChe = plan.LatestResult.Checker;
            if (myChe == null) { return; }

            Vector3Int readPosition = CellPosition.GetInputValue.V3toV3Int();

            if (AddCellSpace == Cells.PR_GetCellPosition.ESpace.LocalCellPosition)
                myChe.AddLocal(readPosition);
            else
                myChe.AddWorld(readPosition);

            if (plan) plan.LatestResult.Checker = myChe;

        }

#if UNITY_EDITOR

        bool firstDraw = true;
        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if (firstDraw)
            {
                firstDraw = false;
                unhideVector = CellPosition.Value != Vector3.zero;
            }

            if (Event.current.type == EventType.MouseDown)
                if (Event.current.mousePosition.x > 10)
                    if (Event.current.mousePosition.x < 100)
                        if (Event.current.mousePosition.y > 9)
                            if (Event.current.mousePosition.y < 28)
                            {
                                unhideVector = !unhideVector;
                            }

            if (unhideVector) CellPosition.ValueDisplayMode = EPortValueDisplay.NotEditable;
            else CellPosition.ValueDisplayMode = EPortValueDisplay.Default;

            if (Event.current.type == EventType.MouseUp)
                CellPosition.Value = CellPosition.Value.V3toV3Int();

            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();

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

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            CheckerField3D chA = ApplyTo.GetInputCheckerSafe;
            if (chA != null) GUILayout.Label("Planner Cells: " + chA.ChildPositionsCount);
        }

#endif

    }
}