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

    public class MR_CellContainsTag : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Cell Contains Tag" : "Cell Contains Tag"; }
        public override string GetNodeTooltipDescription { get { return "Check if cell contains spawn with defined tag"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 208 : 188, _EditorFoldout ? 145 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [Port(EPortPinType.Input)] public PGGStringPort Tag;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public BoolPort Contains;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort Cell;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public override void OnStartReadingNode()
        {
            Tag.TriggerReadPort(true);

            string tag = Tag.GetInputValue;

            if (string.IsNullOrEmpty(tag))
            {
                return; // No tag so no checking
            }

            Cell.TriggerReadPort(true);
            var cell = Cell.GetInputCellValue;
            if (FGenerators.IsNull(cell)) cell = MG_Cell;

            if (cell == null) return;

            var spawns = cell.CollectSpawns();
            bool have = false;

            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue; // unknown spawn
                if (spawns[s] == Rules.QuickSolutions.SR_ModGraph.Graph_SpawnData) continue; // if it's currently being computed spawn then ignore it

                if (SpawnRuleBase.SpawnHaveSpecifics(spawns[s], Tag.GetInputValue, CheckMode)) { have = true; break; }
            }

            Contains.Value = have;
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
                SerializedProperty spc = sp.Copy(); spc.Next(false);

                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}