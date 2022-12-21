#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Modelling
{
    public abstract class SR_TileGenerateNodeBase : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Generating Node"; }
        public override string Tooltip() { return "Node which will generate new object to spawn instead of the selected prefab in spawner list"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }
        public override bool CanBeGlobal() { return false; }

        protected GameObject generatedTile = null;

        [Tooltip("Temporary replace the prefab in 'ToSpawn' for next spawners to gather this tile design result instead of the target prefab")]
        [HideInInspector] public bool ReplacePrefabToSpawn = false;

        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            if (callFrom.TemporaryPrefabOverride != null) return;
            if (generatedTile) { FGenerators.DestroyObject(generatedTile); }
            if (Enabled == false) return;

            generatedTile = GenerateTile();
            if (generatedTile == null) return;

            generatedTile.transform.position = new Vector3(10000, -10000, 10000);
            generatedTile.hideFlags = HideFlags.HideAndDontSave;

            callFrom.SetTemporaryPrefabToSpawn(generatedTile);

            if (ReplacePrefabToSpawn)
                if (callFrom.StampPrefabID >= 0)
                {
                    if (callFrom.Parent.PrefabsList.ContainsIndex(callFrom.StampPrefabID))
                    {
                        callFrom.Parent.PrefabsList[callFrom.StampPrefabID].TemporaryReplace(generatedTile);
                    }
                }
        }

        abstract protected GameObject GenerateTile();


        #region Editor GUI

#if UNITY_EDITOR

        SerializedProperty sp_ReplacePrefabToSpawn;

        /// <summary> Must be in #if UNITY_EDITOR ! </summary>
        protected void _EditorDrawReplacePrefabToSpawnToggle(SerializedObject so)
        {
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
        }

        public override void NodeBody(SerializedObject so)
        {
            _EditorDrawReplacePrefabToSpawnToggle(so);
            base.NodeBody(so);
        }

#endif

        #endregion


        #region Handling Preview Window

#if UNITY_EDITOR

        TilePreviewWindow combinedMeshDisplay = null;

        protected void PreviewWindow(Rect r, Mesh m, Material mat, Material subMeshMat = null)
        {
            if (combinedMeshDisplay == null)
            {
                combinedMeshDisplay = (TilePreviewWindow)Editor.CreateEditor(m, typeof(TilePreviewWindow));
            }

            if (combinedMeshDisplay == null)
            {
                return;
            }

            if (combinedMeshDisplay != null)
            {
                combinedMeshDisplay.selectedInstance = null;
                combinedMeshDisplay.SetMaterial(mat);
                combinedMeshDisplay.UpdateMesh(m);

                if (subMeshMat != null) combinedMeshDisplay.SetSubMeshMaterial(subMeshMat);

                combinedMeshDisplay.OnInteractivePreviewGUI(r, EditorStyles.textArea);
            }
        }

#endif

        #endregion


    }
}