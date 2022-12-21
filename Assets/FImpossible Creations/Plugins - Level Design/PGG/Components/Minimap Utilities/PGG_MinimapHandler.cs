using System;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Minimap Handler")]
    public class PGG_MinimapHandler : MonoBehaviour
    {

        /// <summary> Latest initialized minimap instance </summary>
        public static PGG_MinimapHandler Instance { get; protected set; }


        public RectTransform DisplayRect;

        [Tooltip("How many times displayed content is smaller than the world scale.\n\nIf you want to zoom in-out view dynamically, add 'Minimap Display Controller' component to this object.")]
        public float InitialZoomOut = 20f;
        public Vector3 InitialWorldCenterPosition = Vector3.zero;

        /// <summary> Calculated multiplication on start using 'BaseZoomOut' </summary>
        public float DisplayRatio { get; private set; }


        [HideInInspector]
        public Sprite DefaultMaskSprite;

        protected RectTransform container_BGLayer;
        protected RectTransform container_MiddleLayer;


        void Reset()
        {
            DisplayRect = transform as RectTransform;
        }


        void Awake()
        {
            Instance = this;

            DisplayRatio = 100f / InitialZoomOut;

            if (DisplayRect == null) DisplayRect = transform as RectTransform;
            if (InitialWorldCenterPosition != Vector3.zero) DisplayRect.anchoredPosition = DisplayRect.anchoredPosition + GetUIPosition(InitialWorldCenterPosition);

            #region Prepare Minimap-Layer Containers

            // Generate background layer for minimap display
            container_BGLayer = new GameObject("BG Layer").AddComponent<RectTransform>();
            ResetContainer(container_BGLayer);

            container_MiddleLayer = new GameObject("Mid Layer").AddComponent<RectTransform>();
            ResetContainer(container_MiddleLayer);

            container_MiddleLayer.SetAsFirstSibling();
            container_BGLayer.SetAsFirstSibling();

            #endregion
        }



        /// <summary> Apply UI parameters to handle minimap positioning correctly </summary>
        public void PrepareRectTransformForMinimap(RectTransform element)
        {
            element.anchorMin = DisplayRect.anchorMin;
            element.anchorMax = DisplayRect.anchorMax;
        }

        public void PrepareRectTransformForMinimap(RectTransform element, Vector3 setWorldPosition)
        {
            PrepareRectTransformForMinimap(element);
            SetUIPosition(element, setWorldPosition);
        }

        /// <summary>
        /// Clamping world position for follow to provided units offsets
        /// </summary>
        /// <param name="worldBounds"> World Space bounds of the volume to clamp position with </param>
        /// <param name="followPosition"> Object World Position </param>
        /// <param name="firstAxis"> SET BOTH X and Y positive values. Left / Right border padding values. </param>
        /// <param name="secondaryAxis"> SET BOTH X and Y positive values. Down / Upper border padding values. </param>
        /// <returns></returns>
        public virtual Vector3 ClampFollowWorldPosition(Bounds worldBounds, Vector3 followPosition, Vector2 firstAxis, Vector2 secondaryAxis)
        {
            Vector2 xMargins = new Vector2();
            xMargins.x = worldBounds.center.x - worldBounds.extents.x; xMargins.x += firstAxis.x;
            xMargins.y = worldBounds.center.x + worldBounds.extents.x; xMargins.y -= firstAxis.y;

            Vector2 yMargins = new Vector2();
            yMargins.x = worldBounds.center.z - worldBounds.extents.z; yMargins.x += secondaryAxis.x;
            yMargins.y = worldBounds.center.z + worldBounds.extents.z; yMargins.y -= secondaryAxis.y;

            followPosition.x = Mathf.Clamp(followPosition.x, xMargins.x, xMargins.y);
            followPosition.z = Mathf.Clamp(followPosition.z, yMargins.x, yMargins.y);

            return followPosition;
        }


        public void SetUIPosition(RectTransform r, Vector3 worldPosition)
        {
            r.anchoredPosition = GetUIPosition(worldPosition);
            r.localPosition = new Vector3(r.localPosition.x, r.localPosition.y, 0f);
        }

        public virtual Vector2 GetUIPosition(Vector3 worldPosition)
        {
            return new Vector2(worldPosition.x * DisplayRatio, worldPosition.z * DisplayRatio);
        }

        public Quaternion GetUIRotation(Transform worldTransform)
        {
            return GetUIRotation(worldTransform.eulerAngles.y);
        }

        public Quaternion GetUIRotation(float yAngle)
        {
            return Quaternion.Euler(0f, 0f, -yAngle);
        }


        #region Layer Containers


        /// <summary> Putting UI object in minimap target container </summary>
        public void PutToLayer(RectTransform ui, EMinimapLayer layer)
        {
            switch (layer)
            {
                case EMinimapLayer.Background: PutToBackLayer(ui); return;
                case EMinimapLayer.Middle: PutToMidLayer(ui); return;
                case EMinimapLayer.Front: PutToDefaultLayer(ui); return;
            }
        }


        /// <summary> Putting UI object in minimap background container </summary>
        public void PutToBackLayer(RectTransform ui)
        {
            ui.transform.SetParent(container_BGLayer, false);
        }


        /// <summary> Putting UI object in minimap background container </summary>
        public void PutToMidLayer(RectTransform ui)
        {
            ui.transform.SetParent(container_MiddleLayer, false);
        }


        /// <summary> Putting UI object in minimap front view container </summary>
        public void PutToDefaultLayer(RectTransform ui)
        {
            ui.transform.SetParent(DisplayRect, false);
        }


        protected virtual void ResetContainer(RectTransform container)
        {
            container.SetParent(DisplayRect, true);
            container.pivot = DisplayRect.anchorMin;
            PrepareRectTransformForMinimap(container);
            container.anchoredPosition = Vector2.zero;
            container.localScale = Vector3.one;
            container.localPosition = new Vector3(container.localPosition.x, container.localPosition.y, 0f);
            container.localRotation = Quaternion.identity;
        }


        #endregion


    }



    #region Editor Class

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PGG_MinimapHandler))]
    public class PGG_PixelMapHandlerEditor : UnityEditor.Editor
    {
        public PGG_MinimapHandler Get { get { if (_get == null) _get = (PGG_MinimapHandler)target; return _get; } }
        private PGG_MinimapHandler _get;

        readonly string[] ignore = new string[] { "m_Script" };
        readonly string[] ignorePlaymode = new string[] { "m_Script", "InitialZoomOut", "InitialWorldCenterPosition" };

        UnityEditor.SerializedProperty sp_DisplayRect;
        private void OnEnable()
        {
            sp_DisplayRect = serializedObject.FindProperty("DisplayRect");
        }

        public override void OnInspectorGUI()
        {
            //if (Get.DisplayRect == null)
            //{
            //    serializedObject.Update();
            //    UnityEditor.EditorGUILayout.PropertyField(sp_DisplayRect);
            //    UnityEditor.EditorGUILayout.HelpBox("This component requires Unity UI RectTransform for DisplayRect!", UnityEditor.MessageType.Error);
            //    serializedObject.ApplyModifiedProperties();
            //    return;
            //}

            GUILayout.Space(4);
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, Application.isPlaying ? ignorePlaymode : ignore);
            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(4f);

            UnityEditor.EditorGUILayout.HelpBox("Handling world-to-minimap scale display, draw order and draw position correctness.", UnityEditor.MessageType.None);


            if (Application.isPlaying == false)
            {
                UnityEditor.EditorGUILayout.HelpBox("Inside Display Rect Position (0,0) is reflection of world position (0,0,0)", UnityEditor.MessageType.None);
                if (Get.GetComponent<PGG_MinimapDisplayController>() == false)
                {
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    UnityEditor.EditorGUILayout.HelpBox("Add 'Minimap Display Controller' component for Minimap Follow Target and Minimap Zoom features.", UnityEditor.MessageType.None);

                    GUI.backgroundColor = new Color(0.85f, 0.85f, 1f, 1f);
                    if (GUILayout.Button("Add 'Minimap Display Controller'", UnityEditor.EditorStyles.miniButton, GUILayout.Height(15), GUILayout.MaxWidth(220)))
                    {
                        PGG_MinimapDisplayController dcontr = Get.gameObject.AddComponent<PGG_MinimapDisplayController>();
                        dcontr.Handler = Get;
                    }
                    GUI.backgroundColor = Color.white;

                    UnityEditor.EditorGUILayout.EndVertical();
                }

                bool askForMask = false;
                if (Get.transform.childCount == 0) askForMask = true;
                else if (Get.transform.GetChild(0).childCount == 0) askForMask = true;
                if (Get.transform.GetComponentInChildren<Mask>()) askForMask = false;
                if (Get.transform.GetComponentInChildren<RectTransform>() == null) askForMask = false;

                if (askForMask)
                {
                    UnityEditor.EditorGUILayout.HelpBox("Not detected mask display component in the hierarchy.", UnityEditor.MessageType.Info);
                    if (GUILayout.Button("Add Image mask and Display Rect"))
                    {
                        RectTransform getRect = Get.transform as RectTransform;

                        GameObject mask = new GameObject("Minimap-Mask");
                        RectTransform r = mask.AddComponent<RectTransform>();
                        r.SetParent(Get.transform, true);
                        r.pivot = new Vector2(0, 1);
                        r.anchoredPosition = Vector2.zero;
                        r.localScale = Vector3.one;
                        r.localRotation = Quaternion.identity;
                        r.anchorMin = new Vector2(0, 0);
                        r.anchorMax = new Vector2(1, 1);
                        r.offsetMin = Vector2.zero;
                        r.offsetMax = Vector2.zero;

                        Image maskImg = mask.AddComponent<Image>();
                        maskImg.sprite = Get.DefaultMaskSprite;
                        mask.AddComponent<Mask>().showMaskGraphic = false;

                        GameObject display = new GameObject("Minimap-Display");
                        Get.DisplayRect = display.AddComponent<RectTransform>();
                        display.transform.SetParent(r, true);
                        Get.DisplayRect.sizeDelta = new Vector2(100, 100);
                        Get.DisplayRect.anchoredPosition = Vector2.zero;
                        Get.DisplayRect.localScale = Vector3.one;
                        Get.DisplayRect.localRotation = Quaternion.identity;
                        Get.DisplayRect.pivot = new Vector2(0, 1);
                        Get.DisplayRect.anchorMin = new Vector2(0, 1);
                        Get.DisplayRect.anchorMax = new Vector2(0, 1);

                        UnityEditor.EditorUtility.SetDirty(Get);
                    }
                }

            }

        }
    }
#endif
    #endregion



}