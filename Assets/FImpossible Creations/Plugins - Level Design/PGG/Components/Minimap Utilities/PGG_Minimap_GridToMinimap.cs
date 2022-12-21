using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.Generating
{

    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Grid to Minimap")]
    [DefaultExecutionOrder(10)]
    public class PGG_Minimap_GridToMinimap : MonoBehaviour
    {
        public PGG_MinimapHandler TargetMinimap;
        public PGGGeneratorRoot GenerateOutOf;

        [Tooltip("Generated texture will be applied to this image as sprite")]
        [HideInInspector] public Image ApplyTo;
        [Tooltip("At start there will be generated this prefab and texture will be applied to it's image component as sprite")]
        [HideInInspector] public GameObject ImagePrefab;

        [Space(3)]
        public EGenerateMode GenerateTextureMode = EGenerateMode.TargetTextureResolution;

        [Tooltip("Maximum number of pixels for generated texture width or height (depending if grid is wider or taller)")]
        [HideInInspector] public int TargetResolution = 128;

        [Tooltip("Border offset will improve look of room when it's rotated in any angle. Value is in world units scale.")]
        [HideInInspector] public float BorderPaddingOffset = 1f;

        [Space(5)]
        [Tooltip("Enable if you need to bake just one Y Level of some grid.")]
        [HideInInspector] public bool RestrictDimensionValue = false;


        PGG_MinimapUtilities.MinimapGeneratingSetup latestGenerate;

        public Vector2 LatestRatioTexToWorld { get { return latestGenerate.LatestRatioTexToWorld; } }
        public Texture2D LatestPixelmap { get { return latestGenerate.LatestPixelmap; } }

        /// <summary> Grid bounds in zero rotation, original position and scaled with it's FieldSetup cell size, so without parent world transform effects! </summary>
        public Bounds LatestBounds { get { return latestGenerate.LatestBounds; } }


        [NonSerialized] public float ScalePaintBounds = 1f;
        public GameObject TextPrefab { get; set; }
        public string TextToSet { get; set; }


        /// <summary> Forcing to paint pixelmap in different color than white </summary>
        public Color? OverridePaintColor = null;

        void Reset()
        {
            GenerateOutOf = GetComponentInChildren<PGGGeneratorRoot>();
        }


        void Start()
        {

            if (TargetMinimap == null) TargetMinimap = PGG_MinimapHandler.Instance;


            if (TargetMinimap)
            {
                RectTransform rect;
                Image img;

                #region Generate target object and get components to display room sprite on

                if (ApplyTo != null)
                {
                    rect = ApplyTo.rectTransform;
                    img = ApplyTo;
                }
                else if (ImagePrefab != null)
                {
                    GameObject uiElement = Instantiate(ImagePrefab);

                    rect = uiElement.GetComponent<RectTransform>();
                    if (rect == null) rect = uiElement.AddComponent<RectTransform>();

                    img = uiElement.GetComponent<Image>();
                    if (img == null) img = uiElement.GetComponentInChildren<Image>();
                }
                else // Generating new game object with image component
                {
                    GameObject uiElement = new GameObject(name + "-Minimap");
                    rect = uiElement.AddComponent<RectTransform>();
                    img = uiElement.AddComponent<Image>();
                }

                #endregion

                GenerateGridTexture();

                Sprite mapSprite = Sprite.Create(LatestPixelmap, new Rect(0f, 0f, LatestPixelmap.width, LatestPixelmap.height), latestGenerate.LatestPivotForUI);
                mapSprite.name = name + "-Sprite";

                img.sprite = mapSprite;

                Vector2 borderPaddingScaleRatio = new Vector2(1f, 1f);
                borderPaddingScaleRatio.x = latestGenerate.LatestBakeBounds.size.x / LatestBounds.size.x;
                borderPaddingScaleRatio.y = SecondaryAxis(latestGenerate.LatestBakeBounds.size) / SecondaryAxis(LatestBounds.size);

                rect.sizeDelta = GetUISize(LatestBounds.size, borderPaddingScaleRatio);
                rect.pivot = latestGenerate.LatestPivotForUI;

                TargetMinimap.PutToBackLayer(rect);
                rect.localScale = Vector3.one;

                TargetMinimap.PrepareRectTransformForMinimap(rect);

                rect.localRotation = GetUIRotation(transform);
                TargetMinimap.SetUIPosition(rect, transform.position);

                gameObject.AddComponent<PGG_Minimap_AutoDestroyWith>().ToDestroyWhenBeingDestroyed = rect.gameObject;

                GenerateTextOn(TextPrefab, latestGenerate.GetWorldBounds.center, TextToSet, TargetMinimap);

            }

            Destroy(this);
        }

        public static void GenerateTextOn(GameObject TextPrefab, Vector3 worldPos, string TextToSet, PGG_MinimapHandler TargetMinimap)
        {
            if (TextPrefab)
            {
                if (TextToSet != "")
                    if (TextPrefab.GetComponent<Text>())
                    {
                        if (TargetMinimap == null) TargetMinimap = PGG_MinimapHandler.Instance;
                        if (TargetMinimap == null) return;

                        GameObject uiElement = Instantiate(TextPrefab);

                        RectTransform rect = uiElement.GetComponent<RectTransform>();
                        if (rect == null) rect = uiElement.AddComponent<RectTransform>();

                        Text txt = uiElement.GetComponent<Text>();
                        if (txt == null) txt = uiElement.GetComponentInChildren<Text>();
                        txt.text = TextToSet;

                        TargetMinimap.PutToDefaultLayer(rect);
                        TargetMinimap.PrepareRectTransformForMinimap(rect);

                        rect.localRotation = /*GetUIRotation(transform) * */TextPrefab.transform.rotation;
                        rect.localScale = TextPrefab.transform.localScale;
                        TargetMinimap.SetUIPosition(rect, worldPos);
                    }
            }
        }


        public void GenerateGridTexture()
        {

            latestGenerate = new PGG_MinimapUtilities.MinimapGeneratingSetup();

            latestGenerate.Prepare(GetPaintingColor(), BorderPaddingOffset, GenerateTextureMode, TargetResolution);
            latestGenerate.ApplyFunctions(SecondaryAxis, SetSecondaryAxis, HeightAxis);
            latestGenerate.ScaleInitialBounds = ScalePaintBounds;

            latestGenerate.GenerateFieldMinimap(GenerateOutOf);


            #region Editor Code
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            #endregion

        }



        #region Helper Methods ready to override to midify axis baking

        protected virtual Color GetPaintingColor()
        {
            if (OverridePaintColor != null) return OverridePaintColor.Value;
            return Color.white;
        }

        protected virtual Vector3 SetSecondaryAxis(Vector3 pos, float value)
        {
            pos.z = value; return pos;
        }

        /// <summary> For top down return Z, for Sidescroller return Y </summary>
        protected virtual float SecondaryAxis(Vector3 pos)
        {
            return pos.z;
        }

        /// <summary> For top down return Y, for Sidescroller return Z </summary>
        protected virtual float HeightAxis(Vector3 pos)
        {
            return pos.y;
        }


        #endregion



        protected virtual Quaternion GetUIRotation(Transform t)
        {
            return Quaternion.Euler(0f, 0f, -t.eulerAngles.y);
        }


        protected Vector2 GetUISize(Vector3 worldSize, Vector2 borderPaddingScaleRatio)
        {
            if (TargetMinimap == null) return Vector2.one;

            worldSize.x *= borderPaddingScaleRatio.x;
            worldSize = SetSecondaryAxis(worldSize, SecondaryAxis(worldSize) * borderPaddingScaleRatio.y);

            float ratio = TargetMinimap.DisplayRatio;
            return new Vector2(worldSize.x * ratio, SecondaryAxis(worldSize) * ratio);
        }



        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor(typeof(PGG_Minimap_GridToMinimap))]
        public class PGG_Minimap_GridToMinimapEditor : UnityEditor.Editor
        {

            public PGG_Minimap_GridToMinimap Get { get { if (_get == null) _get = (PGG_Minimap_GridToMinimap)target; return _get; } }
            private PGG_Minimap_GridToMinimap _get;

            SerializedProperty sp_MaxPixelsResolution;
            SerializedProperty sp_ApplyTo;
            SerializedProperty sp_ImagePrefab;

            void OnEnable()
            {
                sp_ApplyTo = serializedObject.FindProperty("ApplyTo");
                sp_ImagePrefab = serializedObject.FindProperty("ImagePrefab");
                sp_MaxPixelsResolution = serializedObject.FindProperty("TargetResolution");
            }

            public override void OnInspectorGUI()
            {
                UnityEditor.EditorGUILayout.HelpBox("Generating small texture with shape of the grid for use on the minimap", UnityEditor.MessageType.None);

                serializedObject.Update();

                GUILayout.Space(4f);
                DrawPropertiesExcluding(serializedObject, "m_Script");
                if (Get.TargetMinimap == null) UnityEditor.EditorGUILayout.HelpBox("Target minimap is not assigned: Component will try to get lastest initialized minimap and add this object to it.", UnityEditor.MessageType.None);

                if (Get.GenerateTextureMode == EGenerateMode.EachCellIsPixel)
                {
                    EditorGUILayout.HelpBox("'Each Cell Is Pixel' Mode is cheap in RAM Memory terms but it looks bad when rooms are rotated", MessageType.Info);
                }
                else if (Get.GenerateTextureMode == EGenerateMode.TargetTextureResolution)
                {
                    SerializedProperty sp = sp_MaxPixelsResolution.Copy();

                    GUILayout.Space(6);
                    EditorGUILayout.PropertyField(sp); sp.Next(false);

                    EditorGUILayout.PropertyField(sp); sp.Next(false);
                    GUILayout.Space(6);
                }


                if (Get.ApplyTo)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 68;
                    EditorGUILayout.PropertyField(sp_ApplyTo);
                    if (GUILayout.Button("X", GUILayout.Width(22))) Get.ApplyTo = null;
                    EditorGUILayout.EndHorizontal();
                }
                else if (Get.ImagePrefab)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 100;
                    EditorGUILayout.PropertyField(sp_ImagePrefab);
                    if (GUILayout.Button("X", GUILayout.Width(22))) Get.ImagePrefab = null;
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 58;
                    EditorGUILayout.PropertyField(sp_ApplyTo);
                    EditorGUILayout.LabelField(" OR ", GUILayout.Width(30));
                    EditorGUIUtility.labelWidth = 82;
                    EditorGUILayout.PropertyField(sp_ImagePrefab);
                    EditorGUILayout.EndHorizontal();
                }


                serializedObject.ApplyModifiedProperties();

                GUILayout.Space(8f);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Preview Generate Grid Texture"))
                {
                    Get.GenerateGridTexture();
                }

                if (Get.LatestPixelmap != null)
                    if (GUILayout.Button("X", GUILayout.Width(22)))
                        FGenerators.DestroyObject(Get.LatestPixelmap);

                EditorGUILayout.EndHorizontal();

                if (Get.LatestPixelmap != null)
                {
                    float viewWidth = EditorGUIUtility.currentViewWidth;
                    float targetWidth = viewWidth - 30;
                    if (targetWidth <= 0f) return;

                    float ratio = Get.LatestPixelmap.width / viewWidth;

                    if (ratio <= 0f) return;
                    float targetHeight = Get.LatestPixelmap.height / ratio;

                    if (targetHeight > Get.LatestPixelmap.height)
                    {
                        targetHeight = Get.LatestPixelmap.height;
                        if (Get.LatestPixelmap.width < targetWidth) targetWidth = Get.LatestPixelmap.width;
                    }

                    GUILayout.Space(4);
                    EditorGUILayout.ObjectField("Grid Texture:", Get.LatestPixelmap, typeof(Texture), true);
                    GUILayout.Space(-20);
                    EditorGUILayout.LabelField(new GUIContent(Get.LatestPixelmap), GUILayout.Width(targetWidth), GUILayout.Height(targetHeight));
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Width: " + Get.LatestPixelmap.width + " Height: " + Get.LatestPixelmap.height + " Tex-to-world Ratio: " + (Get.LatestRatioTexToWorld), MessageType.None);
                }

            }

        }

#endif

        #endregion


    }

}