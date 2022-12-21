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

    public class MR_GenerateSpawn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("  Generate Spawn") : "Generate Spawn"; }
        public override string GetNodeTooltipDescription { get { return "Generating new spawn data to freely assign prefab etc.\nCan be triggered to reset generating new spawn data."; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(220, _EditorFoldout ? 120 : 102); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [HideInInspector] [Port(EPortPinType.Output)] public PGGSpawnPort Generated;

        public enum EApplyPrefabToSpawn
        {
            None, Reference, GetFromModificatorUsingIndex
        }

        [HideInInspector] public EApplyPrefabToSpawn ApplyPrefab = EApplyPrefabToSpawn.None;
        [HideInInspector] public GameObject SpawnPrefab = null;
        [HideInInspector] public int ModPrefab = 0;


        private FieldCell lastCopyCell = null;
        private SpawnData lastCopySpawn = null;

        public override void PreGeneratePrepare()
        {
            Generated.Clear();
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            base.Execute(print, newResult);
            lastCopySpawn = null;
            lastCopyCell = null;
        }

        public override void OnStartReadingNode()
        {

            if (MG_Mod == null) return;

            if (MG_Cell == lastCopyCell && MG_Spawn == lastCopySpawn) return; // Don't generate multiple copies on the same spawner in the same cell

            Generated.Clear();

            int pfId = -2;

            if (ApplyPrefab == EApplyPrefabToSpawn.GetFromModificatorUsingIndex)
            {
                pfId = ModPrefab;
            }

            SpawnData spawn = SpawnData.GenerateSpawn(MG_Spawner, MG_Mod, MG_Cell, pfId);
            Generated.FirstSpawnForOutputPort = spawn;

            if (ApplyPrefab == EApplyPrefabToSpawn.Reference)
            {
                spawn.Prefab = SpawnPrefab;
                spawn.TryDetectMeshInPrefab();
            }
            else if (ApplyPrefab == EApplyPrefabToSpawn.None)
            {
                spawn.Prefab = null;
                spawn.PreviewMesh = null;
            }

            lastCopyCell = MG_Cell;
            lastCopySpawn = MG_Spawn;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Generated");

            EditorGUILayout.PropertyField(sp);
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x-80));
            //GUILayout.Space(-40);
            //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Connecting trigger port is not required! Check node tooltip for more info"), EditorStyles.label, GUILayout.Width(16)))
            //{
            //    EditorUtility.DisplayDialog("Copy Spawn Node", "Generating copy of some spawn.\nCan be triggered to reset possibiltiy to generate copy.\nConnecting trigger port is not required but can be useful during iteration loops.", "Ok");
            //}
            //EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 90;

            SerializedProperty spc = sp.Copy();
            spc.Next(false); EditorGUILayout.PropertyField(spc);

            if (_EditorFoldout)
            {
                if (ApplyPrefab == EApplyPrefabToSpawn.None)
                {
                    EditorGUILayout.HelpBox("With 'None' generating empty SpawnData", MessageType.None);
                }
                else
                {

                    if (ApplyPrefab == EApplyPrefabToSpawn.GetFromModificatorUsingIndex)
                    {
                        spc.Next(false);
                        spc.Next(false); EditorGUILayout.PropertyField(spc);
                    }
                    else if (ApplyPrefab == EApplyPrefabToSpawn.Reference)
                    {
                        spc.Next(false); EditorGUILayout.PropertyField(spc);
                    }
                }
            }

            EditorGUIUtility.labelWidth = 0;

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}