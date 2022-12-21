using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_GetSpawnPrefab : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("   Get Spawn Prefab") : "Get Spawn Prefab"; }
        public override string GetNodeTooltipDescription { get { return "Getting reference to the spawned prefab"; } }
        public override Color GetNodeColor() { return new Color(0.45f, 0.55f, 0.95f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 200 : 184, _EditorFoldout ? 104 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Output, 1)] public PGGUniversalPort Prefab;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGSpawnPort TargetSpawn;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            TargetSpawn.TriggerReadPort(true);
            var spawn = TargetSpawn.GetFirstConnectedSpawn;

            if (TargetSpawn.IsConnected == false) spawn = MG_Spawn;
            if (FGenerators.IsNull(spawn)) { return; }

            Prefab.Variable.SetValue(spawn.Prefab);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("TargetSpawn");
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                TargetSpawn.AllowDragWire = true;
            }
            else
            {
                TargetSpawn.AllowDragWire = false;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}