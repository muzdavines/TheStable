using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_GetSpawnsInCell : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Get Spawns In Cell" :  "Get Spawns In Cell"; }
        public override string GetNodeTooltipDescription { get { return "Collecting all spawns in provided cell"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 100 : 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [Port(EPortPinType.Output)] public PGGSpawnPort Spawns;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort Cell;

        public override void OnStartReadingNode()
        {
            Cell.TriggerReadPort(true);
            var cell = Cell.GetInputCellValue;
            if (FGenerators.IsNull(cell)) cell = MG_Cell;

            if (cell == null) return;

            var spawns = cell.CollectSpawns();

            //for (int s = 0; s < spawns.Count; s++)
            //{
            //    if (spawns[s].OwnerMod == null) continue; // unknown spawn
            //    if (spawns[s] == Rules.QuickSolutions.SR_ModGraph.Graph_SpawnData) continue; // if it's currently being computed spawn then ignore it
            //}

            Spawns.ApplySpawnsGroup(spawns);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Cell");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}