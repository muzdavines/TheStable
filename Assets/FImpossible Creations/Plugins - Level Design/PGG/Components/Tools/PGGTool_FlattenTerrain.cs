#if UNITY_EDITOR
using FIMSpace.FEditor;
#endif
using FIMSpace.Generating.Rules.PostEvents;
using System;
using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Tools/PGG Flatten Terrain")]
    public class PGGTool_FlattenTerrain : MonoBehaviour, IGenerating
    {
        public bool FlattenOnGameStart = true;
        [HideInInspector] public bool AllowPostGenerator = true;

        [Header("Detecting Terrain Object")]
        public LayerMask GroundRaycastMask = 1 << 0;
        [Tooltip("Most cases it will be 0,-1,0 so straight down")]
        public Vector3 RaycastDirection = Vector3.down;
        [Tooltip("How far collision raycast can go")]
        public float RaycastLength = 7f;
        [Tooltip("Casting ray from upper or lower position of the object")]
        public Vector3 OffsetRaycastOrigin = Vector3.up;

        [Header("Terrain Shaping Parameters")]
        [Range(0f, 1f)]
        [Header("How much terrain should be flattened like opacity")]
        public float FlattenAmount = 1f;
        [Header("How far flatten-falloff should go")]
        public float BrushRadius = 3f;
        [Header("If ground should be leveled with object's origin or with some offset")]
        public Vector3 OffsetGround = Vector3.zero;
        [Header("Spherical falloff for flattening terrain level")]
        [FPD_FixedCurveWindow(0f,0f,1f,1f)]
        public AnimationCurve Falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);


        private void Start()
        {
            if (FlattenOnGameStart)
            {
                FlattenTerrain();
            }
        }

        [NonSerialized] public Terrain backupTerrain = null;
        [NonSerialized] public float[,] backupHeights = null;

        public void FlattenTerrain()
        {
            backupTerrain = SR_FlattenTerrain.DetectTerrain(gameObject, GroundRaycastMask, RaycastDirection, RaycastLength, OffsetRaycastOrigin);
            backupHeights = SR_FlattenTerrain.DetectTerrainAndFlattenGroundLevel(gameObject, backupTerrain, FlattenAmount, BrushRadius, OffsetGround, Falloff);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            SR_FlattenTerrain.DrawTerrainFlatteningGizmos(gameObject, GroundRaycastMask, RaycastDirection, RaycastLength, OffsetRaycastOrigin, FlattenAmount, BrushRadius, OffsetGround, Falloff);
        }

#endif

        public void Generate()
        {
            if (!AllowPostGenerator) return;
            FlattenTerrain();
        }

        public void PreviewGenerate()
        {
            if (!AllowPostGenerator) return;
            FlattenTerrain();
        }

        public void IG_CallAfterGenerated() { }

    }

    #region Drawing 'Test Align' button inside inspector window


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGGTool_FlattenTerrain))]
    public class PGGTool_FlattenTerrainEditor : UnityEditor.Editor
    {
        public PGGTool_FlattenTerrain Get { get { if (_get == null) _get = (PGGTool_FlattenTerrain)target; return _get; } }
        private PGGTool_FlattenTerrain _get;
        //bool displayEvent = false;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("This component is just simple utility tool for aligning unity terrain level below the object", UnityEditor.MessageType.Info);

            FGUI_Inspector.LastGameObjectSelected = Get.gameObject;

            DrawDefaultInspector();

            GUILayout.Space(4);
            if (GUILayout.Button("Test Flatten")) Get.FlattenTerrain();
            if ( Get.backupHeights != null && Get.backupHeights.Length > 10 && Get.backupTerrain != null)
            {
                if (GUILayout.Button("Undo Flatten"))
                {
                    Get.backupTerrain.terrainData.SetHeights(0, 0, Get.backupHeights);
                    Get.backupTerrain = null;
                }
            }
        }
    }
#endif


    #endregion

}