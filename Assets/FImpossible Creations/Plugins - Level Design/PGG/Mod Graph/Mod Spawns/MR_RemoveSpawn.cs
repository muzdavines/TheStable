using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_RemoveSpawn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("Remove Spawn") : "Remove Spawn"; }
        public override string GetNodeTooltipDescription { get { return "Removing Provided Spawn from Generating Queue"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 104 : 80); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [Port(EPortPinType.Input, 1)] public PGGSpawnPort SpawnToRemove;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGModCellPort KnownCell;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            SpawnToRemove.TriggerReadPort(true);
            var spawn = SpawnToRemove.GetFirstConnectedSpawn;
            if (FGenerators.IsNull(spawn)) { return; }
            if (spawn == MG_Spawn) { return; }

            var spawns = SpawnToRemove.GetConnectedSpawnsList;
            FieldCell inCell = null;

            if ( KnownCell.IsConnected)
            {
                KnownCell.TriggerReadPort(true);
                inCell = KnownCell.GetInputCellValue;
            }

            if (spawns == null || spawns.Count == 0) return;

            if (FGenerators.IsNull(inCell))
            {
                for (int s = 0; s < spawns.Count; s++)
                {
                    spawns[s].Enabled = false;
                }
            }
            else
            {
                for (int s = 0; s < spawns.Count; s++)
                {
                    inCell.RemoveSpawnFromCell(spawns[s]);
                }
            }
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
       
            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("KnownCell");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}