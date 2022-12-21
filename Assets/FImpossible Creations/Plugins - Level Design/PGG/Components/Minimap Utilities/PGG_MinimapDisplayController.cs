#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    [DefaultExecutionOrder(100)]
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Minimap Display Controller")]
    public class PGG_MinimapDisplayController : MonoBehaviour
    {
        public PGG_MinimapHandler Handler;

        [Tooltip("Required to define center of view when following target object")]
        public RectTransform MapMaskedView;

        [Space(6)]
        public Transform ToFollow;

        [Tooltip("Offset following position in world units")]
        public Vector3 FollowOffset = Vector3.zero;

        [Tooltip("World Space Units to clamp-limit minimap movement to.\nWhen this value is greater than zero, you will see gizmos box on the scene in scale of provided world bounds below.")]
        public float ClampBorder = 0f;
        public Bounds BoundsForClamp;

        [Space(6)]
        [Range(0f, 1f)]
        public float FollowSpeed = 1f;
        [Space(1)]
        [Range(0f, 1f)]
        public float ZoomAnimateSpeed = 1f;
        [Space(1)]
        [Range(0f, 1f)]
        public float RotateAnimateSpeed = 1f;

        [Space(6)]
        [Tooltip("Apply debug input: Mouse scroll to controll zoom and 'Q' and 'E' keys for map rotation")]
        public bool TestInputForDebugging = true;

        /// <summary> You can change this variable for custom zoom clamp ranges </summary>
        [System.NonSerialized] public Vector2 ClampZoom = new Vector2(0.25f, 2f);

        Vector2 _sd_Follow = Vector2.zero;
        public Vector2 _Follow { get; private set; }

        float _sd_Zoom = 0f;
        float _TargetZoom = 1f;
        float _sd_Rotate = 0f;
        float _TargetRotation = 0f;
        float _CurrentZoom = 1f;
        float _CurrentRotation = 0f;


        float GetSmoothDampDuration(float speed)
        {
            return Mathf.InverseLerp(1f, 0.001f, speed);
        }


        void LateUpdate()
        {

            // Apply Zoom and rotation

            #region Support animating zoom / rotation smoothly

            if (ZoomAnimateSpeed > 0f)
            {
                if (ZoomAnimateSpeed < 1f)
                    _CurrentZoom = Mathf.SmoothDamp(_CurrentZoom, _TargetZoom, ref _sd_Zoom, GetSmoothDampDuration(ZoomAnimateSpeed) * 0.5f, float.MaxValue, Time.deltaTime);
                else
                    _CurrentZoom = _TargetZoom;
            }

            if (RotateAnimateSpeed > 0f)
            {
                if (RotateAnimateSpeed < 1f)
                    _CurrentRotation = Mathf.SmoothDampAngle(_CurrentRotation, _TargetRotation, ref _sd_Rotate, GetSmoothDampDuration(RotateAnimateSpeed) * 0.3f, float.MaxValue, Time.deltaTime);
                else
                    _CurrentRotation = _TargetRotation;
            }

            #endregion


            Handler.DisplayRect.localScale = new Vector3(_CurrentZoom, _CurrentZoom, 1f);
            Handler.DisplayRect.localRotation = Quaternion.Euler(0f, 0f, _CurrentRotation);


            if (FollowSpeed > 0f)
                if (ToFollow != null)
                {
                    // Prepare world position to follow
                    Vector3 followWorldPosition = ToFollow.position;

                    if (ClampBorder > 0f) followWorldPosition = Handler.ClampFollowWorldPosition(BoundsForClamp, followWorldPosition, new Vector2(ClampBorder, ClampBorder), new Vector2(ClampBorder, ClampBorder));
                    followWorldPosition += FollowOffset;


                    // Transform world position to UI display
                    //float displayRatio = GetTextureToDisplayRatio();
                    Vector2 mapSpacePos = Handler.GetUIPosition(followWorldPosition);

                    if (FollowSpeed >= 1f) _Follow = mapSpacePos;
                    else _Follow = Vector2.SmoothDamp(_Follow, mapSpacePos, ref _sd_Follow, GetSmoothDampDuration(FollowSpeed), float.MaxValue, Time.deltaTime);


                    // Apply zoom / rotation if using
                    Vector2 finalFollowPos = _Follow;
                    finalFollowPos *= _CurrentZoom;
                    if (_CurrentRotation != 0f) finalFollowPos = Quaternion.Euler(0f, 0f, _CurrentRotation) * finalFollowPos;


                    // Calculate and apply final UI position
                    Vector2 viewCenterOffset = new Vector2();
                    viewCenterOffset.x = (MapMaskedView.rect.width * 0.5f);
                    viewCenterOffset.y = (MapMaskedView.rect.height * -0.5f);

                    //viewCenterOffset = Handler.GetUIPosition(viewCenterOffset);
                    Vector2 targetMapPos = -finalFollowPos + viewCenterOffset;
                    Handler.DisplayRect.anchoredPosition = targetMapPos;
                }



            if (TestInputForDebugging)
            {
                if (ZoomAnimateSpeed > 0f)
                {
                    if (Input.mouseScrollDelta.y > 0.1f) ZoomInBy(0.1f);
                    else if (Input.mouseScrollDelta.y < -0.1f) ZoomInBy(-0.1f);
                }

                if (RotateAnimateSpeed > 0f)
                {
                    if (Input.GetKey(KeyCode.Q)) { RotateBy(-150f * Time.deltaTime); }
                    if (Input.GetKey(KeyCode.E)) { RotateBy(150f * Time.deltaTime); }
                }
            }


        }


        public void ApplyZoom(float targetZoom = 1f)
        {
            _TargetZoom = Mathf.Clamp(targetZoom, ClampZoom.x, ClampZoom.y);
        }


        public void ZoomInBy(float zoomBy = 0.1f)
        {
            ApplyZoom(_TargetZoom + zoomBy * _TargetZoom);
        }


        public void RotateBy(float rotate)
        {
            ApplyMapRotate(_TargetRotation + rotate);
        }


        public void ApplyMapRotate(float targetRotation = 0f)
        {
            _TargetRotation = targetRotation;
        }


        #region Editor Code

#if UNITY_EDITOR

        void OnDrawGizmosSelected()
        {
            if (ClampBorder <= 0f) return;

            Vector3 MapWorldCenter = BoundsForClamp.center;
            Vector3 MapWorldSize = BoundsForClamp.size;

            Gizmos.color = new Color(0.25f, 1f, 0.25f, 0.25f);
            Gizmos.DrawWireSphere(MapWorldCenter, 0.1f);
            Gizmos.DrawRay(MapWorldCenter - Vector3.right * 3f, Vector3.right * 6f);
            Gizmos.DrawRay(MapWorldCenter - Vector3.forward * 3f, Vector3.forward * 6f);
            Gizmos.DrawRay(MapWorldCenter - Vector3.up * 3f, Vector3.up * 6f);

            Gizmos.DrawWireCube(MapWorldCenter, MapWorldSize);
            Gizmos.color = new Color(0.25f, 1f, 0.25f, 0.05f);
            Gizmos.DrawCube(MapWorldCenter, MapWorldSize);

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.3f);

            float tY = MapWorldCenter.y + 1f;

            Vector3 c1 = BoundsForClamp.max; c1.y = tY;
            Vector3 c2 = BoundsForClamp.max; c2.y = tY;

            Vector3 rf = -new Vector3(ClampBorder, 0f, ClampBorder );
            Vector3 rb = -new Vector3(ClampBorder, 0f, -ClampBorder );
            Vector3 lf = -new Vector3(-ClampBorder, 0f, ClampBorder );
            Vector3 lb = -new Vector3(-ClampBorder, 0f, -ClampBorder );

            c2.x = BoundsForClamp.min.x;
            Gizmos.DrawLine( c1 + rf ,  c2 + lf);

            c1 = BoundsForClamp.min; c1.y = tY;
            Gizmos.DrawLine( c1 + lb,  c2 + lf);

            c2 = BoundsForClamp.max; c2.y = tY;
            c2.z = BoundsForClamp.min.z;
            Gizmos.DrawLine( c1 + lb,  c2 + rb);
            
            c1 = BoundsForClamp.max; c1.y = tY;
            Gizmos.DrawLine( c1 + rf,  c2 + rb);

        }

#endif

        #endregion


    }


    #region Editor Class

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(PGG_MinimapDisplayController))]
    public class PGG_PixelMapDisplayHelperEditor : UnityEditor.Editor
    {
        public PGG_MinimapDisplayController Get { get { if (_get == null) _get = (PGG_MinimapDisplayController)target; return _get; } }
        private PGG_MinimapDisplayController _get;

        readonly string[] ignore = new string[] { "m_Script" };
        readonly string[] ignoreClamp = new string[] { "m_Script", "BoundsForClamp" };

        SerializedProperty sp_Handler;

        void OnEnable()
        {
            sp_Handler = serializedObject.FindProperty("Handler");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Get.Handler == null)
            {
                EditorGUILayout.PropertyField(sp_Handler);

                GUILayout.Space(4);
                UnityEditor.EditorGUILayout.HelpBox("Minimap Handler is Required!", UnityEditor.MessageType.Info);

                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawPropertiesExcluding(serializedObject, Get.ClampBorder > 0f ? ignore : ignoreClamp);
            serializedObject.ApplyModifiedProperties();

            if (Get.ClampBorder < 0f) Get.ClampBorder = 0f;
            GUILayout.Space(4f);
            UnityEditor.EditorGUILayout.HelpBox("Handling minimap following target, minimap zoom and minimap rotation", UnityEditor.MessageType.Info);
        }
    }

#endif

    #endregion


}