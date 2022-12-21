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

    public class MR_SpawnIsTagged : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("  Spawn is " + GetModeStr()) : "Check if Spawn is Tagged"; }
        public override string GetNodeTooltipDescription { get { return "Check if provided spawn is tagged with provided string"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 144 : 122); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [Port(EPortPinType.Input)] public PGGStringPort Tag;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public BoolPort Contains;
        [Port(EPortPinType.Input, 1)] public PGGSpawnPort Spawn;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        string GetModeStr()
        {
            switch (CheckMode)
            {
                case ESR_Details.Tag: return "Tagged";
                case ESR_Details.SpawnStigma: return "Stigmed";
                case ESR_Details.CellData: return "Cell Data";
                case ESR_Details.Name: return "Named";
            }

            return "Tagged";
        }

        public override void OnStartReadingNode()
        {
            Tag.TriggerReadPort(true);
            string tag = Tag.GetInputValue;

            if (string.IsNullOrEmpty(tag))
            {
                return; // No tag so no checking
            }

            Spawn.TriggerReadPort(true);

            if (Spawn.IsConnected == false) // Check self spawn
            {
                SpawnData spawn = MG_Spawn;
                if (FGenerators.IsNull(spawn)) return;
                Contains.Value = SpawnRuleBase.SpawnHaveSpecifics(spawn, Tag.GetInputValue, CheckMode);
            }
            else
            {
                var spawns = Spawn.GetConnectedSpawnsList;

                bool have = false;
                for (int s = 0; s < spawns.Count; s++)
                {
                    if (spawns[s].OwnerMod == null) continue; // unknown spawn
                    if (spawns[s] == Rules.QuickSolutions.SR_ModGraph.Graph_SpawnData) continue; // if it's currently being computed spawn then ignore it

                    if (SpawnRuleBase.SpawnHaveSpecifics(spawns[s], Tag.GetInputValue, CheckMode)) { have = true; break; }
                }

                Contains.Value = have;
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
                if (sp == null) sp = baseSerializedObject.FindProperty("CheckMode");
                EditorGUIUtility.labelWidth = 86;
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}