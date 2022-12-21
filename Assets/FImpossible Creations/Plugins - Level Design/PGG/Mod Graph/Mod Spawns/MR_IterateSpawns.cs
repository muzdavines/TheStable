using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_IterateSpawns : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Iterate Spawns" : "Iterate Spawns"; }
        public override string GetNodeTooltipDescription { get { return "Iterating through spawns in cell or in provided multiple spawns if provided"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
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

        [Port(EPortPinType.Input, 1)] public PGGModCellPort InCell;
        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public BoolPort BreakIteration;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Iteration Spawn")] public PGGSpawnPort IterationSpawn;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGSpawnPort CustomSpawns;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            List<SpawnData> toIterate = null;
            if (CustomSpawns.PortState() == EPortPinState.Connected)
            {
                CustomSpawns.TriggerReadPort(true);
                toIterate = CustomSpawns.GetLocalSpawnsList;
            }

            if (toIterate == null || toIterate.Count == 0)
            {
                InCell.TriggerReadPort(true);
                var cell = InCell.GetInputCellValue;
                if (FGenerators.IsNull(cell)) cell = MG_Cell;

                if (cell == null) return;

                toIterate = cell.CollectSpawns();
            }

            if (toIterate == null || toIterate.Count == 0) return;

            BreakIteration.Value = false;
            IterationSpawn.Clear();
            
            for (int c = 0; c < toIterate.Count; c++)
            {
                IterationSpawn.FirstSpawnForOutputPort = toIterate[c];
                CallOtherExecutionWithConnector(1, print);

                BreakIteration.TriggerReadPort(true);
                if (BreakIteration.GetInputValue == true) break;
            }

            IterationSpawn.Clear();
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("CustomSpawns");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}