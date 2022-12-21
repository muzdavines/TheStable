using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using UnityEditor.Callbacks;
using FIMSpace.Generating.Checker;

namespace FIMSpace.Generating
{
    public partial class ShapeDesignerWindow : EditorWindow
    {
        public static ShapeDesignerWindow Get;

        GenerationShape tempPreset;
        GenerationShape projectPreset;
        GenerationShape selectedPreset;

        SerializedObject so_preset;
        Vector2 mainScroll = Vector2.zero;
        bool repaint = true;
        public int seed = 0;
        public float PreviewSize = 1f;
        bool drawGenPreviewSetts = true;
        CheckerField checker;


        #region Opening window

        [MenuItem("Window/FImpossible Creations/Level Design/Legacy/Shape Designer (Will be removed in next versions)", false, 171)]
        static void Init()
        {
            ShapeDesignerWindow window = (ShapeDesignerWindow)GetWindow(typeof(ShapeDesignerWindow));
            window.titleContent = new GUIContent("Generation Shape", Resources.Load<Texture>("SPR_GenShape"));
            window.Show();
            if (window.tempPreset == null) window.tempPreset = CreateInstance<GenerationShape>();
            Get = window;
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenBuildPlanScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as GenerationShape != null)
            {
                Init();
                Get.projectPreset = obj as GenerationShape;
                return true;
            }

            return false;
        }

        #endregion


        void OnGUI()
        {
            if (Get == null) Init();

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            EditorGUILayout.HelpBox("NOT SUPPORTED BY 'Building Plan Generator' component! But you can use FacilityGenerator instead", MessageType.None);
            EditorGUILayout.HelpBox("In future versions there will be much more options for shape generating", MessageType.None);

            #region Preparations and headers

            Color bgC = GUI.backgroundColor;


            Get = this;
            if (projectPreset == null) selectedPreset = tempPreset;
            else selectedPreset = projectPreset;
            if (selectedPreset != null) so_preset = new SerializedObject(selectedPreset);


            GUILayout.Space(6);
            EditorGUILayout.LabelField("Controll shape of target generated areas", FGUI_Resources.HeaderStyle);
            GUILayout.Space(2);


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref drawGenPreviewSetts, new GUIContent(" Preview Settings"), FGUI_Resources.FoldStyle, null);


            #region Center camera buttons

            // Center Button
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                if (view.camera != null)
                {
                    float referenceScale = 8f;
                    if (projectPreset != null) referenceScale = PreviewSize * 3f;


                    if (Vector3.Distance(view.camera.transform.position, new Vector3(0, referenceScale, -referenceScale)) > referenceScale)
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent(" Center", EditorGUIUtility.IconContent("Camera Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Height(19)))
                        {
                            FieldDesignWindow.FrameCenter(referenceScale);
                        }
                    }

                    float angleDiff = Quaternion.Angle(view.camera.transform.rotation, Quaternion.identity);

                    if (angleDiff > 125)
                    {
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("RotateTool").image), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            FieldDesignWindow.FrameCenter(referenceScale, true);
                        }
                    }
                }

            #endregion


            EditorGUILayout.EndHorizontal();


            EditorGUI.BeginChangeCheck();

            if (drawGenPreviewSetts)
            {
                GUILayout.Space(3);

                PreviewSize = EditorGUILayout.FloatField("Preview Scale", PreviewSize);

                GUILayout.Space(5);
            }

            if (EditorGUI.EndChangeCheck()) repaint = true;
            EditorGUILayout.EndVertical();


            #endregion


            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            projectPreset = (GenerationShape)EditorGUILayout.ObjectField("Preset:", projectPreset, typeof(GenerationShape), false);

            if (projectPreset == null)
            {
                if (GUILayout.Button("Export Current", GUILayout.Width(94))) projectPreset = (GenerationShape)FGenerators.GenerateScriptable(Instantiate(selectedPreset), "Shape_");
            }
            else
                if (GUILayout.Button("Export Copy", GUILayout.Width(94))) projectPreset = (GenerationShape)FGenerators.GenerateScriptable(Instantiate(selectedPreset), projectPreset.name);

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            seed = EditorGUILayout.IntField("Seed: ", seed);
            if (seed == 0) EditorGUILayout.LabelField("(Random)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            EditorGUI.BeginChangeCheck();

            #region Drawing preset inspector

            GUILayout.Space(9);

            so_preset.Update();
            SerializedProperty sp_setup = so_preset.FindProperty("Setup");

            if (sp_setup != null)
            {
                #region Drawing Inspector GUI for Generation Mode

                var s = selectedPreset.Setup;
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                SerializedProperty sp_i = sp_setup.Copy();
                sp_i.Next(true);
                EditorGUILayout.PropertyField(sp_i);

                if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.StaticSizeRectangle)
                {
                    SerializedProperty sp = sp_setup.FindPropertyRelative("RectSetup");
                    if (s.RectSetup.Width.IsZero) { s.RectSetup.Width = new MinMax(5, 5); s.RectSetup.Height = new MinMax(3, 3); s.RectSetup.StartPos = new Vector2Int(0, 0); }

                    selectedPreset.Setup.RectSetup.Width.Min = EditorGUILayout.IntField("Width:", selectedPreset.Setup.RectSetup.Width.Min);
                    if (selectedPreset.Setup.RectSetup.Width.Min < 1) selectedPreset.Setup.RectSetup.Width.Min = 1;
                    selectedPreset.Setup.RectSetup.Height.Min = EditorGUILayout.IntField("Height:", selectedPreset.Setup.RectSetup.Height.Min);
                    if (selectedPreset.Setup.RectSetup.Height.Min < 1) selectedPreset.Setup.RectSetup.Height.Min = 1;
                }
                if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.RandomRectangle)
                {
                    if (s.RectSetup.Width.IsZero) { s.RectSetup.Width = new MinMax(4, 5); s.RectSetup.Height = new MinMax(2, 3); s.RectSetup.StartPos = new Vector2Int(0, 0); }

                    SerializedProperty sp = sp_setup.FindPropertyRelative("RectSetup");
                    sp.Next(true);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                }
                #region Backup
                //else if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.MultipleRectangles)
                //{
                //    for (int i = 0; i < s.Rects.Count; i++)
                //    {
                //        var r = s.Rects[i];
                //        if (r.Width.IsZero)
                //        {
                //            r.Width = new MinMax(4, 5);
                //            r.Height = new MinMax(2, 3);
                //            r.StartPos = new Vector2Int(0, 0);
                //            s.Rects[i] = r;
                //        }
                //    }

                //    SerializedProperty sp = sp_setup.FindPropertyRelative("Rects");
                //    EditorGUILayout.PropertyField(sp);
                //}
                #endregion
                else if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.RandomTunnels)
                {
                    SerializedProperty sp = sp_setup.FindPropertyRelative("BranchLength");
                    EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                }

                EditorGUILayout.EndVertical();

                #endregion
            }
            else
            {
                EditorGUILayout.HelpBox("No Preset!", MessageType.Warning);
            }

            GUILayout.Space(9);

            #endregion

            if (EditorGUI.EndChangeCheck()) repaint = true;

            if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.ManualRectangles)
            {
                DrawManualRectanglesGUI();

                if ( selectedPreset.Setup.CellSets.Count > 1)
                {
                    EditorGUILayout.HelpBox("Generators will randomly choose one of prepared shapes with random seed", MessageType.None);
                }
            }

            GUILayout.Space(3);

            if (GUILayout.Button("Generate Preview"))
            {
                repaint = true;
            }

            GUILayout.Space(3);

            EditorGUILayout.EndScrollView();


            #region Ending, applying and restoring

            if (so_preset != null) so_preset.ApplyModifiedProperties();

            if (FGenerators.CheckIfIsNull(checker)) repaint = true;

            if (selectedPreset != null)
                if (selectedPreset.Setup != null)
                    if (repaint)
                    {
                        if (selectedPreset.Setup.GenerationMode == GenerationShape.EGenerationMode.ManualRectangles)
                        {
                            if (selectedPreset.Setup != null)
                            {
                                if (selectedManualShape >= selectedPreset.Setup.CellSets.Count) selectedManualShape = 0;
                                checker = selectedPreset.Setup.CellSets[selectedManualShape].GetChecker();
                            }
                        }
                        else
                            checker = selectedPreset.GetChecker(null);
                        checker.UseBounds = false;
                        SceneView.RepaintAll();
                        repaint = false;
                    }


            #endregion

        }




        #region Base preparation for Scene GUI Draw

        public Matrix4x4 gridMatrix = Matrix4x4.identity;
        void OnSceneGUI(SceneView sceneView)
        {
            //if (SceneView.currentDrawingSceneView == null) return;
            //if (SceneView.currentDrawingSceneView.camera == null) return;

            //Handles.SetCamera(SceneView.currentDrawingSceneView.camera);
            //Handles.BeginGUI();
            //DrawGuidesGUI();
            //DrawShape();
            //Handles.EndGUI();

            //Handles.BeginGUI();
            //Handles.SetCamera(SceneView.currentDrawingSceneView.camera);
            //Handles.matrix = gridMatrix;
            //if (grid != null) DrawHandles(SceneView.currentDrawingSceneView.camera);
            //Handles.matrix = Matrix4x4.identity;
            //Handles.EndGUI();
        }


        void OnFocus()
        {
            //#if UNITY_2019_1_OR_NEWER
            //            SceneView.duringSceneGui -= this.OnSceneGUI;
            //            SceneView.duringSceneGui += this.OnSceneGUI;
            //#else
            //            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            //            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            //#endif

            FGeneratorsGizmosDrawer.AddEvent(OnDrawGizmos);

        }


        void OnDestroy()
        {
            //#if UNITY_2019_1_OR_NEWER
            //            SceneView.duringSceneGui -= this.OnSceneGUI;
            //#else
            //            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            //#endif

            if (FGeneratorsGizmosDrawer.Instance)
            {
                FGenerators.DestroyObject(FGeneratorsGizmosDrawer.Instance.gameObject);
            }
        }

        #endregion


        void OnDrawGizmos()
        {
            DrawGuidesGUI();
            DrawShape();
        }


        void DrawGuidesGUI()
        {
            Color prehC = Handles.color;

            Vector3 cellSize = Vector3.one * PreviewSize;
            int pSz = Mathf.RoundToInt(PreviewSize);

            // Draw grid helper axis
            Vector3Int min = new Vector3Int(-pSz, 0, 0);
            if (FGenerators.CheckIfExist_NOTNULL(checker))
            {
                min = new Vector3Int(checker.GetBoundsMin().x * pSz, 0, checker.GetBoundsMin().y * pSz);
            }

            Handles.color = new Color(0f, 0f, 1f, 0.5f);
            Vector3 leftSide = new Vector3((min.x - 0.5f) * (cellSize.x), 0, (min.z + 0.25f) * (cellSize.z));
            leftSide += cellSize.x * Vector3.left * 0.3f;

            Handles.Label(leftSide + cellSize.x * Vector3.left * 0.3f + Vector3.back * cellSize.x * 0.2f, new GUIContent("Forward"), EditorStyles.centeredGreyMiniLabel);
            FEditor.FGUI_Handles.DrawArrow(leftSide, Quaternion.identity, cellSize.x * 0.5f);

            Handles.color = new Color(1f, .4f, .4f, 0.5f);
            Vector3 rightSide = new Vector3((min.x + 0.25f) * (cellSize.x), 0, (min.z - 0.5f) * (cellSize.z));
            rightSide += cellSize.z * Vector3.back * 0.3f;
            Handles.Label(rightSide + cellSize.z * Vector3.back * 0.3f, new GUIContent("Right"), EditorStyles.centeredGreyMiniLabel);
            FEditor.FGUI_Handles.DrawArrow(rightSide, Quaternion.Euler(0, 90, 0), cellSize.x * 0.5f);

            Handles.color = prehC;
        }


        void DrawShape()
        {
            if (FGenerators.CheckIfIsNull(checker)) return;
            Color preC = Gizmos.color;
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            checker.DrawGizmos(PreviewSize * 1f, false, 0.9f);
            Gizmos.color = preC;
        }

    }
}