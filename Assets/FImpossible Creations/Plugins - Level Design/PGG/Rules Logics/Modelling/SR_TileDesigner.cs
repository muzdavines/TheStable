#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_TileDesigner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Tile Designer"; }
        public override string Tooltip() { return "Generating object to generate with tile designer"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }
        //public EProcedureType Type { get { return EProcedureType.Coded; } }

        [HideInInspector] public TileDesign Design;
        private GameObject generatedDesign = null;

        [Tooltip("Temporary replace the prefab in 'ToSpawn' for next spawners to gather this tile design result instead of the target prefab")]
        [HideInInspector] public bool ReplacePrefabToSpawn = false;



        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {

            if (callFrom.TemporaryPrefabOverride != null)
            {
                return;
            }

            if (generatedDesign) { FGenerators.DestroyObject(generatedDesign); }

            if (Enabled == false) return;

            Design.FullGenerateStack();
            generatedDesign = Design.GeneratePrefab();
            generatedDesign.transform.position = new Vector3(10000, -10000, 10000);
            generatedDesign.hideFlags = HideFlags.HideAndDontSave;

            callFrom.SetTemporaryPrefabToSpawn(generatedDesign);

            if (ReplacePrefabToSpawn)
                if (callFrom.StampPrefabID >= 0)
                {
                    if (callFrom.Parent.PrefabsList.ContainsIndex(callFrom.StampPrefabID))
                    {
                        callFrom.Parent.PrefabsList[callFrom.StampPrefabID].TemporaryReplace(generatedDesign);
                    }
                }
        }


        #region Editor GUI

#if UNITY_EDITOR


        public override void NodeHeader()
        {
            base.NodeHeader();

            if (_editor_drawRule == false)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(20)))
                {
                    TileDesignerWindow.Init(Design, this, true);
                }
        }


        SerializedProperty sp_mat;
        SerializedProperty sp_mat2;
        SerializedProperty sp_ReplacePrefabToSpawn;

        public override void NodeBody(SerializedObject so)
        {
            if (Design != null)
            {
                if (Design.DesignName == "New Tile")
                {
                    Design.DesignName = OwnerSpawner.Name;
                    EditorUtility.SetDirty(this);
                }
            }

            EditorGUILayout.HelpBox(" Replacing object to spawn with Tile Design", MessageType.Info);
            base.NodeBody(so);

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("  Open Tile Designer", FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Height(24)))
            {
                TileDesignerWindow.Init(Design, this);
            }

            if (Design.TileMeshes.Count == 1)
            {
                if (GUILayout.Button("Quick Edit", FGUI_Resources.ButtonStyle, GUILayout.Width(72), GUILayout.Height(24)))
                {
                    TileDesignerWindow.Init(Design, this, true);
                }
            }

            GUILayout.EndHorizontal();

            if (Design._LatestGen_Meshes > 0)
            {

                EditorGUILayout.LabelField("Target mesh tris: " + Design._LatestGen_Tris, EditorStyles.centeredGreyMiniLabel);

                if (sp_mat == null) sp_mat = so.FindProperty("Design").FindPropertyRelative("DefaultMaterial");
                SerializedProperty sp_draw = sp_mat;

                if (sp_mat.objectReferenceValue == null)
                {
                    if ( Design.TileMeshes.Count > 0)
                    {
                        if (Design.TileMeshes[0].Material != null)
                        {
                            sp_draw = so.FindProperty("Design").FindPropertyRelative("TileMeshes").GetArrayElementAtIndex(0).FindPropertyRelative("Material");
                        }
                    }
                }

                GUILayout.Space(3);
                EditorGUILayout.PropertyField(sp_draw);
                GUILayout.Space(3);

            }

            if (OwnerSpawner.StampPrefabID >= 0)
            {
                EditorGUIUtility.labelWidth = 170;
                if (sp_ReplacePrefabToSpawn == null) sp_ReplacePrefabToSpawn = so.FindProperty("ReplacePrefabToSpawn");
                EditorGUILayout.PropertyField(sp_ReplacePrefabToSpawn);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                ReplacePrefabToSpawn = false;
            }

            GUILayout.Space(6);
        }

        //public override void NodeFooter(SerializedObject so, FieldModification mod)
        //{
        //    if (ReplaceSelectedSpawn)
        //    {
        //        EditorGUILayout.HelpBox("All other spawners using same 'To Spawn' will generate Tile Design Object", MessageType.None);
        //    }

        //    base.NodeFooter(so, mod);
        //}

#endif

        #endregion

    }
}