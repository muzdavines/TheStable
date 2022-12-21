using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("")]
    public class PGG_UI_MinimapActiveElement : MonoBehaviour
    {
        public PGG_MinimapHandler Minimap;

        [Space(5)] public Transform WorldObjectToFollow;
        public bool Rotate = true;
        public float AngleOffset = 0f;
        [Range(0f, 1f)]
        public float ScaleRatio = 1f;

        protected RectTransform rect;


        protected virtual void Start()
        {
            rect = transform as RectTransform;
            Minimap.PrepareRectTransformForMinimap(rect);
        }


        protected virtual void Update()
        {
            if (Minimap == null) return;
            if (WorldObjectToFollow == null) return;

            Minimap.SetUIPosition(rect, WorldObjectToFollow.position);
            
            if (Rotate) 
            { transform.localRotation = Minimap.GetUIRotation(WorldObjectToFollow.eulerAngles.y + AngleOffset); }

            if (ScaleRatio != 1f)
            {
                float scale = Mathf.LerpUnclamped(1f / Minimap.DisplayRect.localScale.x, 1f, ScaleRatio);
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

    }


    #region Editor Class
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGG_UI_MinimapActiveElement))]
    public class PGG_UI_MinimapActiveElementEditor : UnityEditor.Editor
    {
        public PGG_UI_MinimapActiveElement Get { get { if (_get == null) _get = (PGG_UI_MinimapActiveElement)target; return _get; } }
        private PGG_UI_MinimapActiveElement _get;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(4f);
            UnityEditor.EditorGUILayout.HelpBox("This component is decitated for UI! It's updating minimap UI position for target world object.\n\nIn most cases it's added by other components so you shouldn't add it on your own if you don't know what it is!", UnityEditor.MessageType.Info);
        }
    }
#endif
    #endregion

}