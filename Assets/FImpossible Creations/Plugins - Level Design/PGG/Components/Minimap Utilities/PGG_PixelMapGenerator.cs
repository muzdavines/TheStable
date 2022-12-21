using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Pixel Minimap Generator")]
    public class PGG_PixelMapGenerator : MonoBehaviour
    {
        public List<PGGGeneratorRoot> GenerateOutOf;
        public PGG_MinimapHandler AddToMinimap;


        [Space(5)]
        public int MaxPixelsResolution = 512;
        public float BorderUnitsOffset = 0f;

        public bool ForceAspectRatio = false;
        public Vector2 AspectRatio = new Vector2(1f, 1f);

        [Space(5)]
        [Tooltip("Apply generated pixel map to UI image")]
        public Image ApplyToImage = null;

        [Space(5)]
        [Tooltip("Enable if you need to bake just one Y Level of some grid.")]
        public bool RestrictDimensionValue = false;


        PGG_MinimapUtilities.MinimapGeneratingSetup latestGenerate;
        public Vector3 LatestRatioTexToWorld { get { return latestGenerate.LatestRatioTexToWorld; } }
        public Texture2D LatestPixelmap { get { return latestGenerate.LatestPixelmap; } }


        void Start()
        {
            if (AddToMinimap == null) return;

            if (LatestPixelmap == null) GeneratePixelMap();
            else if (ApplyToImage) return; // Already generated and applied

            RectTransform rect;
            Image img;

            #region Generate target object and get components to display map sprite on

            if (ApplyToImage != null)
            {
                rect = ApplyToImage.rectTransform;
                img = ApplyToImage;
            }
            else // Generating new game object with image component
            {
                GameObject uiElement = new GameObject(name + "-Minimap");
                rect = uiElement.AddComponent<RectTransform>();
                img = uiElement.AddComponent<Image>();
            }

            #endregion

            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite mapSprite = Sprite.Create(LatestPixelmap, new Rect(0f, 0f, LatestPixelmap.width, LatestPixelmap.height), pivot);
            mapSprite.name = name + "-Sprite";
            rect.pivot = pivot;

            img.sprite = mapSprite;

            float textureToWorldRatio = LatestPixelmap.width / latestGenerate.LatestBounds.size.x;
            rect.sizeDelta = new Vector2(LatestPixelmap.width / textureToWorldRatio, LatestPixelmap.height / textureToWorldRatio) * AddToMinimap.DisplayRatio;

            AddToMinimap.PutToDefaultLayer(rect); // Put in main hierarchy
            rect.SetAsFirstSibling(); // And move to be under every element
            rect.localScale = Vector3.one;

            AddToMinimap.PrepareRectTransformForMinimap(rect, latestGenerate.LatestBounds.center);
        }


        public void GeneratePixelMap()
        {

            latestGenerate = new PGG_MinimapUtilities.MinimapGeneratingSetup();

            latestGenerate.Prepare(GetPaintingColor(), BorderUnitsOffset, EGenerateMode.TargetTextureResolution, MaxPixelsResolution);
            latestGenerate.ApplyFunctions(SecAxis, SetSec, HeightAxis);

            Vector2? aspectRatio = null;
            if (ForceAspectRatio) aspectRatio = AspectRatio;
            latestGenerate.GenerateFieldsPixelmap(GenerateOutOf, aspectRatio);


#if UNITY_EDITOR

            if (!Application.isPlaying)
                if (ApplyToImage != null)
                {
                    Sprite mapSprite = Sprite.Create(LatestPixelmap, new Rect(0.0f, 0.0f, LatestPixelmap.width, LatestPixelmap.height), new Vector2(0.5f, 0.5f), 100.0f);
                    mapSprite.name = name + "-Pixelmap";
                    ApplyToImage.sprite = mapSprite;
                    EditorUtility.SetDirty(ApplyToImage);
                }

#endif

        }



        #region Helper Methods ready to override to midify axis baking

        protected virtual Color GetPaintingColor()
        {
            return Color.white;
        }


        /// <summary> Return secondary axis value (For top down it's Z, Sidescroller it's Y) </summary>
        protected virtual float SecAxis(Vector3 v)
        {
            return v.z;
        }

        /// <summary> Set secondary axis value (For top down it's Z, Sidescroller it's Y) </summary>
        protected virtual Vector3 SetSec(Vector3 v, float val)
        {
            v.z = val;
            return v;
        }

        protected virtual float HeightAxis(Vector3 v)
        {
            return v.y;
        }


        #endregion



        #region Editor Code

#if UNITY_EDITOR

        void OnDrawGizmosSelected()
        {
            if (LatestPixelmap == null) return;
            Gizmos.DrawWireCube(latestGenerate.LatestBounds.center, latestGenerate.LatestBounds.size);
        }

#endif

        #endregion


    }



    #region Editor Class

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PGG_PixelMapGenerator))]
    public class PGG_PixelMapGeneratorEditor : UnityEditor.Editor
    {
        public PGG_PixelMapGenerator Get { get { if (_get == null) _get = (PGG_PixelMapGenerator)target; return _get; } }
        private PGG_PixelMapGenerator _get;

        bool unfoldPreview = false;
        readonly string[] _ignore1 = new string[] { "m_Script" };
        readonly string[] _ignore2 = new string[] { "m_Script", "AspectRatio" };

        SerializedProperty sp_BakeOnly;

        void OnEnable()
        {
            sp_BakeOnly = serializedObject.FindProperty("BakeOnly");
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Use PIXEL MAP GENERATOR for simple minimaps.\nIt is not supporting generators with rotation different than 0  90  180  and  270  angles!\nIt generates one big image in a run.", MessageType.Info);
            GUILayout.Space(4f);

            serializedObject.Update();
            if (Get.ForceAspectRatio)
            {
                DrawPropertiesExcluding(serializedObject, _ignore1);
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, _ignore2);
            }

            if (Get.RestrictDimensionValue)
            {
                string axis = Info_RestrictAxisLetter();
                float axisValue = Info_RestrictAxisValue();
                EditorGUILayout.HelpBox("Enabled painting just cells in this object " + axis + " position, so only cells in range of " + axis + " = " + axisValue + " world position.", MessageType.None);
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(4f);
            EditorGUILayout.HelpBox("You can call 'GeneratePixelMap()' in PGG Generator - AfterGenerating Event!", MessageType.None);

            GUILayout.Space(8f);

            if (GUILayout.Button("Generate"))
            {
                Get.GeneratePixelMap();
            }

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


                if (unfoldPreview == false)
                {
                    if (GUILayout.Button(" Click to Preview Generated Pixelmap ", EditorStyles.helpBox)) unfoldPreview = !unfoldPreview;
                }
                else
                {
                    if (GUILayout.Button(" Preview Generated Pixelmap ", EditorStyles.helpBox)) unfoldPreview = !unfoldPreview;
                    GUILayout.Space(4);
                    EditorGUILayout.ObjectField("Pixel Map:", Get.LatestPixelmap, typeof(Texture), true);
                    GUILayout.Space(4);
                    EditorGUILayout.LabelField(new GUIContent(Get.LatestPixelmap), GUILayout.Width(targetWidth), GUILayout.Height(targetHeight));
                    GUILayout.Space(4);
                    EditorGUILayout.HelpBox("Width: " + Get.LatestPixelmap.width + " Height: " + Get.LatestPixelmap.height + " Tex-to-world Ratio: " + (Get.LatestRatioTexToWorld), MessageType.None);
                }

            }
        }

        protected virtual string Info_RestrictAxisLetter()
        {
            return "Y";
        }

        protected virtual float Info_RestrictAxisValue()
        {
            return Get.transform.position.y;
        }

    }

#endif

    #endregion


}
