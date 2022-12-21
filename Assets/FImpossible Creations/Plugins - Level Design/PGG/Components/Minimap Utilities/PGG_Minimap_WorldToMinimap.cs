#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.Generating
{
    [DefaultExecutionOrder(10)]
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG World Object to Minimap")]
    public class PGG_Minimap_WorldToMinimap : MonoBehaviour
    {
        public PGG_MinimapHandler TargetMinimap;

        public bool StaticObject = true;
        public bool RotateOnMinimap = false;

        [Tooltip("Change if you want to add rotation angle offset to the minimap sprite if it's rotated in wrong direction")]
        public float AngleOffset = 0f;

        [Range(0f, 1f)]
        [Tooltip("If zooming in-out should effect this model display on it.\n1 is default and model will be scalled accordingly with the map.\n0 is no scale effect on this object display on minimap.")]
        public float ScaleRatioOnMinimap = 1f;

        [Tooltip("Apply minimap position controller to instantiated prefab - or if it's scene object - applying minimap position controller to the scene object with rect transform.")]
        public GameObject UIPrefab;
        public Sprite SpriteForGeneratedUI = null;
        public Color ColorizeGeneratedUI = Color.white;
        public Vector3 WorldSize = Vector3.zero;
        public EMinimapLayer TargetLayer = EMinimapLayer.Front;

        void Start()
        {
            if (TargetMinimap == null) TargetMinimap = PGG_MinimapHandler.Instance;

            if (TargetMinimap)
            {
                Bounds targetSize = new Bounds(Vector3.zero, WorldSize);

                localSet = false;
                if (WorldSize == Vector3.zero) targetSize = AutoGetLocalBounds(); else localSet = true;

                targetSize.size = Vector3.Scale(targetSize.size, transform.lossyScale);

                GameObject uiElement;
                RectTransform rect;

                if (UIPrefab == null)
                {
                    uiElement = new GameObject(name + "-Minimap");
                    rect = uiElement.AddComponent<RectTransform>();
                    Image img = uiElement.AddComponent<Image>();
                    img.sprite = SpriteForGeneratedUI;
                    img.color = ColorizeGeneratedUI;
                    rect.sizeDelta = GetUISize(targetSize.size);
                    rect.localScale = Vector3.one;
                }
                else
                {
                    if (UIPrefab.scene == transform.gameObject.scene)
                    {
                        uiElement = UIPrefab;
                        rect = uiElement.GetComponent<RectTransform>();
                    }
                    else
                    {
                        uiElement = Instantiate(UIPrefab);
                        rect = uiElement.GetComponent<RectTransform>();
                        if (rect == null) rect = uiElement.AddComponent<RectTransform>();
                    }
                }

                bool buildPlannerSprite = false;
                if (GetComponent<BuildPlannerReference>())
                {
                    Image img = uiElement.GetComponent<Image>();
                    if (img) img.color = ColorizeGeneratedUI;
                    rect.sizeDelta = GetUISize(targetSize.size);
                    rect.localScale = Vector3.one;
                    buildPlannerSprite = true;
                }
                else
                {
                    if (localSet)
                    {
                        if (TargetMinimap.GetUIPosition(new Vector3(0f, 1f, 0f)).y != 0f)
                        { } // Just for now - check if it's 2D
                        else
                            rect.rotation = TargetMinimap.GetUIRotation(transform);
                    }
                }

                TargetMinimap.PutToLayer(rect, TargetLayer);
                TargetMinimap.PrepareRectTransformForMinimap(rect);
                TargetMinimap.SetUIPosition(rect, transform.TransformPoint(targetSize.center));

                if (!StaticObject)
                {
                    PGG_UI_MinimapActiveElement activElem = uiElement.AddComponent<PGG_UI_MinimapActiveElement>();
                    activElem.Minimap = TargetMinimap;
                    activElem.WorldObjectToFollow = transform;
                    activElem.Rotate = RotateOnMinimap;
                    activElem.ScaleRatio = ScaleRatioOnMinimap;
                    activElem.AngleOffset = AngleOffset;
                }
                else
                {
                    if (!buildPlannerSprite)
                        rect.localRotation = TargetMinimap.GetUIRotation(transform.eulerAngles.y);
                }

                gameObject.AddComponent<PGG_Minimap_AutoDestroyWith>().ToDestroyWhenBeingDestroyed = rect.gameObject;
            }

            Destroy(this);
        }

        /// <summary> When setting bounds using manual values local to world </summary>
        bool localSet = false;

        public Bounds AutoGetLocalBounds()
        {
            localSet = false;

            Vector3 cent = Vector3.zero;
            Vector3 size = WorldSize;

            BuildPlannerReference bpRef = GetComponent<BuildPlannerReference>();

            if (bpRef)
            {
                Bounds b = bpRef.WorldSpaceBounds;
                b.center = transform.InverseTransformPoint(b.center);
                return b;
            }

            MeshFilter filt = GetComponent<MeshFilter>();
            if (!filt) filt = GetComponentInChildren<MeshFilter>();
            if (filt) if (filt.sharedMesh)
                {
                    return filt.sharedMesh.bounds;
                }

            localSet = true;

            //SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();
            //if (!rend) rend = GetComponentInChildren<SkinnedMeshRenderer>();
            //if (rend) { size = rend.bounds.size; cent = rend.bounds.center; }
            //else localSet = true;

            return new Bounds(cent, size);
        }

        Vector3 DivV3(Vector3 a, Vector3 by)
        {
            if (by.x == 0f || by.y == 0f || by.z == 0f) return a;
            return new Vector3(a.x / by.x, a.y / by.y, a.z / by.z);
        }

        protected virtual Vector2 GetUISize(Vector3 worldSize)
        {
            if (TargetMinimap == null) return Vector2.one;
            float ratio = TargetMinimap.DisplayRatio;
            return new Vector2(worldSize.x * ratio, worldSize.z * ratio);
        }

#if UNITY_EDITOR

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.7f);
            Gizmos.matrix = transform.localToWorldMatrix;

            Bounds tSize = new Bounds(Vector3.zero, WorldSize);
            if (WorldSize == Vector3.zero) tSize = AutoGetLocalBounds(); else localSet = true;

            Vector3 cent = tSize.center;
            Vector3 size = tSize.size; // DivV3(tSize.size, transform.lossyScale)

            if (localSet)
            {
                cent = Vector3.zero;
                //size = DivV3(WorldSize, transform.lossyScale);
            }

            Gizmos.DrawWireCube(cent, size);

            Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.1f);
            Gizmos.DrawCube(cent, size);

            Gizmos.matrix = Matrix4x4.identity;
        }

        public bool _Editor_ShowTopInfo = true;

#endif

    }


    #region Editor Class

#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGG_Minimap_WorldToMinimap))]
    public class PGG_Minimap_WorldToMinimapEditor : UnityEditor.Editor
    {
        public PGG_Minimap_WorldToMinimap Get { get { if (_get == null) _get = (PGG_Minimap_WorldToMinimap)target; return _get; } }
        private PGG_Minimap_WorldToMinimap _get;

        SerializedProperty sp_TargetMinimap;
        SerializedProperty sp_UIPrefab;
        SerializedProperty sp_ColorizeUI;
        SerializedProperty sp_WorldSize;
        SerializedProperty sp_Static;
        SerializedProperty sp_RotateOnMinimap;
        SerializedProperty sp_ScaleRatioOnMinimap;
        SerializedProperty sp_SpriteForGeneratedUI;
        SerializedProperty sp_TargetLayer;
        SerializedProperty sp_SpriteAngleOffset;

        void OnEnable()
        {
            sp_TargetMinimap = serializedObject.FindProperty("TargetMinimap");
            sp_UIPrefab = serializedObject.FindProperty("UIPrefab");
            sp_ColorizeUI = serializedObject.FindProperty("ColorizeGeneratedUI");
            sp_WorldSize = serializedObject.FindProperty("WorldSize");
            sp_Static = serializedObject.FindProperty("StaticObject");
            sp_RotateOnMinimap = serializedObject.FindProperty("RotateOnMinimap");
            sp_ScaleRatioOnMinimap = serializedObject.FindProperty("ScaleRatioOnMinimap");
            sp_SpriteForGeneratedUI = serializedObject.FindProperty("SpriteForGeneratedUI");
            sp_TargetLayer = serializedObject.FindProperty("TargetLayer");
            sp_SpriteAngleOffset = serializedObject.FindProperty("AngleOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Get._Editor_ShowTopInfo)
            {
                UnityEditor.EditorGUILayout.HelpBox("Transporting this world object to target minimap on start", UnityEditor.MessageType.Info);
                Rect infoRect = GUILayoutUtility.GetLastRect();
                if (GUI.Button(infoRect, "", EditorStyles.label)) Get._Editor_ShowTopInfo = false;
            }

            EditorGUILayout.PropertyField(sp_TargetMinimap);
            if (Get.TargetMinimap == null) UnityEditor.EditorGUILayout.HelpBox("Target minimap is not assigned: Component will try to get lastest initialized minimap and add this object to it.", UnityEditor.MessageType.None);
            EditorGUILayout.PropertyField(sp_TargetLayer);

            GUILayout.Space(4f);

            EditorGUILayout.PropertyField(sp_UIPrefab);

            if (Get.UIPrefab == null)
            {
                UnityEditor.EditorGUILayout.HelpBox("UI Prefab is empty: Component will generate simple UI rectangle object instead.", UnityEditor.MessageType.None);
                EditorGUILayout.PropertyField(sp_SpriteForGeneratedUI);
                EditorGUILayout.PropertyField(sp_ColorizeUI);

                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(sp_WorldSize);

                if (Get.WorldSize == Vector3.zero)
                {
                    UnityEditor.EditorGUILayout.HelpBox("World Size is Zero: Trying to determinate it using render component: " + Get.AutoGetLocalBounds().size + "\nTake look on scene gizmos for a box as a reference", UnityEditor.MessageType.None);
                }
            }


            GUILayout.Space(4f);
            EditorGUILayout.PropertyField(sp_Static);
            if (Get.StaticObject == false)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_RotateOnMinimap);
                EditorGUIUtility.labelWidth = 80;
                if (sp_RotateOnMinimap.boolValue) EditorGUILayout.PropertyField(sp_SpriteAngleOffset);
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.PropertyField(sp_ScaleRatioOnMinimap);
            }

            GUILayout.Space(7f);
            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion

}