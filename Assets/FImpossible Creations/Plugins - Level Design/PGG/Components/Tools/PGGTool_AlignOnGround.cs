#if UNITY_EDITOR
using FIMSpace.FEditor;
#endif
using FIMSpace.Generating.Rules.PostEvents;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Tools/PGG Align On Ground")]
    public class PGGTool_AlignOnGround : MonoBehaviour, IGenerating
    {
        public bool AlignOnGameStart = true;
        [HideInInspector] public bool AllowPostGenerator = true;

        [Header("Detecting Ground")]
        public LayerMask GroundRaycastMask = 1 << 0;
        [Tooltip("Most cases it will be 0,-1,0 so straight down")]
        public Vector3 RaycastDirection = Vector3.down;
        [Tooltip("How far collision raycast can go")]
        public float RaycastLength = 7f;
        [Tooltip("Casting ray from upper or lower position of the object")]
        public Vector3 OffsetRaycastOrigin = Vector3.up;

        [Header("Placing on Ground")]
        [Range(0f, 1f)]
        public float AlignRotationAmount = 1f;
        [Tooltip("If origin of the object is not on the bottom you can move it higher/lower with this value")]
        public Vector3 OffsetOnGround = Vector3.zero;
        [Tooltip("Offset on slope, like when ground have very big angle then object will snap-offset to side of the slope")]
        [Range(0f, 1f)] public float OffsetWithSlopeDirection = 1f;


        private void Start()
        {
            if ( AlignOnGameStart)
            {
                AlignObject();
            }
        }

        public void AlignObject()
        {
            List<Collider> selfColliders = FTransformMethods.FindComponentsInAllChildren<Collider>(transform);
            List<bool> wasEnabled = new List<bool>();

            for (int i = 0; i < selfColliders.Count; i++) wasEnabled.Add(selfColliders[i].enabled);
            for (int i = 0; i < selfColliders.Count; i++)
                selfColliders[i].enabled = false; // Ignoring self colliders

            Physics.SyncTransforms(); 

            SR_AlignToGround.AlignObjectOnGround(gameObject, GroundRaycastMask, RaycastDirection, RaycastLength, OffsetRaycastOrigin, AlignRotationAmount, OffsetOnGround, OffsetWithSlopeDirection);

            for (int i = 0; i < selfColliders.Count; i++) selfColliders[i].enabled = wasEnabled[i];
            Physics.SyncTransforms(); // Restoring self colliders
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            SR_AlignToGround.DrawGroundAligningGizmos(gameObject, RaycastDirection, RaycastLength, OffsetRaycastOrigin);
        }
#endif

        public void Generate()
        {
            if (!AllowPostGenerator) return;
            AlignObject();
        }

        public void PreviewGenerate()
        {
            if (!AllowPostGenerator) return;
            AlignObject();
        }

        public void IG_CallAfterGenerated() { }
    }

    #region Drawing 'Test Align' button inside inspector window


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGGTool_AlignOnGround))]
    public class PGGTool_AlignOnGroundEditor : UnityEditor.Editor
    {
        public PGGTool_AlignOnGround Get { get { if (_get == null) _get = (PGGTool_AlignOnGround)target; return _get; } }
        private PGGTool_AlignOnGround _get;
        //bool displayEvent = false;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("This component is just simple utility tool for aligning spawned objects on the ground", UnityEditor.MessageType.Info);

            FGUI_Inspector.LastGameObjectSelected = Get.gameObject;

            DrawDefaultInspector();

            GUILayout.Space(4);
            if (GUILayout.Button("Test Align")) Get.AlignObject();
        }
    }
#endif


    #endregion

}