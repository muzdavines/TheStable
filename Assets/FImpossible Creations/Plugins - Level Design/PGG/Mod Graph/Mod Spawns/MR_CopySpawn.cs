using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_CopySpawn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("Copy Spawn") : "Copy Spawn"; }
        public override string GetNodeTooltipDescription { get { return "Generating copy of some spawn.\nCan be triggered to reset possibiltiy to generate copy."; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 146 : 86); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGSpawnPort ToCopy;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGSpawnPort Copied;

        private FieldCell lastCopyCell = null;
        private SpawnData lastCopySpawn = null;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            base.Execute(print, newResult);
            lastCopySpawn = null;
            lastCopyCell = null;
        }

        public override void OnStartReadingNode()
        {
            if (MG_Cell == lastCopyCell && MG_Spawn == lastCopySpawn) return; // Don't generate multiple copies on the same spawner in the same cell

            Copied.Clear();
            ToCopy.TriggerReadPort(true);

            var spawn = ToCopy.GetFirstConnectedSpawn;
            if (FGenerators.IsNull(spawn)) if (ToCopy.IsConnected == false) spawn = MG_Spawn;
            if (FGenerators.IsNull(spawn)) { return; }

            spawn = spawn.Copy();
            spawn.DontSpawnMainPrefab = false;
            Copied.FirstSpawnForOutputPort = spawn;

            lastCopyCell = MG_Cell;
            lastCopySpawn = MG_Spawn;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("ToCopy");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 106));
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Connecting trigger port is not required! Check node tooltip for more info"), EditorStyles.label, GUILayout.Width(16)))
            {
                EditorUtility.DisplayDialog("Copy Spawn Node", "Generating copy of some spawn.\nCan be triggered to reset possibiltiy to generate copy.\nConnecting trigger port is not required but can be useful during iteration loops.", "Ok");
            }
            EditorGUILayout.EndHorizontal();

            SerializedProperty spc = sp.Copy();
            spc.Next(false);
            GUILayout.Space(-19);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(NodeSize.x - 82);
            EditorGUILayout.PropertyField(spc, GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}