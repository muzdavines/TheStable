using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_AddSpawnStigma : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("  Add Spawn Stigma") : "Add Spawn Stigma"; }
        public override string GetNodeTooltipDescription { get { return "Adding string stigma to the provided spawn which can be used by other spawners logics for identifying"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(196, _EditorFoldout ? 106 : 84); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [Port(EPortPinType.Input, 1)] public PGGStringPort Stigma;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGSpawnPort Spawn;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Spawn.Clear();
            Spawn.TriggerReadPort(true);
            var spawn = Spawn.GetFirstConnectedSpawn;
            if (FGenerators.IsNull(spawn)) { spawn = MG_Spawn; }
            if (FGenerators.IsNull(spawn)) { return; }

            Stigma.TriggerReadPort(true);
            string str = Stigma.GetInputValue;
            if (!string.IsNullOrEmpty(str))
            {
                spawn.AddCustomStigma(str);
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
                if (sp == null) sp = baseSerializedObject.FindProperty("Spawn");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif
    }
}