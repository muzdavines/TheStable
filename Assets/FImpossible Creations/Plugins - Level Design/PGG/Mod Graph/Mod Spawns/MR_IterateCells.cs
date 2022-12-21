using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_IterateCells : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Iterate Cells" : "Iterate Cells"; }
        public override string GetNodeTooltipDescription { get { return "Iterating through Cells list"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(194, _EditorFoldout ? 152 : 134); } }

        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override int AllowedOutputConnectionIndex { get { return 0; } }
        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Finish";
            return "Iteration";
        }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Input, 1)] public PGGModCellPort CellsList;
        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public BoolPort BreakIteration;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Iteration Cell")] public PGGModCellPort IterationCell;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            List<FieldCell> toIterate = null;
            if (CellsList.PortState() == EPortPinState.Connected)
            {
                CellsList.TriggerReadPort(true);
                toIterate = CellsList.GetAllConnectedCellsList();
            }

            if (toIterate == null || toIterate.Count == 0) return;

            BreakIteration.Value = false;
            IterationCell.Clear();
            
            for (int c = 0; c < toIterate.Count; c++)
            {
                IterationCell.ProvideFullCellData(toIterate[c]);
                CallOtherExecutionWithConnector(1, print);

                BreakIteration.TriggerReadPort(true);
                if (BreakIteration.GetInputValue == true) break;
            }

            IterationCell.Clear();
        }


#if UNITY_EDITOR
        //SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();
            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}