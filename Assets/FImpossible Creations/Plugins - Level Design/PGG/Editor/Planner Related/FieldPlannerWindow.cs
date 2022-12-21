using FIMSpace.FEditor;
using FIMSpace.Graph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


namespace FIMSpace.Generating.Planning
{
    public class FieldPlannerWindow : EditorWindow
    {
        public static FieldPlannerWindow Get;
        Vector2 mainScroll = Vector2.zero;
        bool flex = false;
        public static FieldPlanner LatestFieldPlanner;
        public Texture2D VignPlanner;

        //[MenuItem("Window/FImpossible Creations/Level Design/Field Planner Window (Beta)", false, 51)]
        static void Init()
        {
            FieldPlannerWindow window = (FieldPlannerWindow)GetWindow(typeof(FieldPlannerWindow));
            window.titleContent = new GUIContent("Field Planner", Resources.Load<Texture>("SPR_PGG"));
            window.Show();
            window.minSize = new Vector2(240, 160);
            Get = window;
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenBuildPlannerScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as FieldPlanner != null)
            {
                Init();
                LatestFieldPlanner = obj as FieldPlanner;
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            Get = this;
        }

        #region Simple Utilities

        public static void SelectFieldPlanner(FieldPlanner mod, bool show = true)
        {
            FieldPlannerWindow window = (FieldPlannerWindow)GetWindow(typeof(FieldPlannerWindow));
            Get = window;
            LatestFieldPlanner = mod;
            Get.prem = null;

            if (show)
            {
                window = (FieldPlannerWindow)GetWindow(typeof(FieldPlannerWindow));
                window.Show();
            }
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenFieldScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as FieldPlanner != null)
            {
                Init();
                LatestFieldPlanner = (FieldPlanner)obj;
                return true;
            }

            return false;
        }

        #endregion

        FieldPlanner prem = null;
        public static bool forceChanged = false;


        private void OnGUI()
        {
            PGGUtils.SetDarkerBacgroundOnLightSkin();
            bool changed = false;
            //EditorGUIUtility.labelWidth = 340;
            //flex = EditorGUILayout.Toggle("Toggle this if there is too many vertical elements to view", flex);
            //EditorGUIUtility.labelWidth = 0;

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            GUILayout.Space(5);

            //latestMod = (FieldPlanner)EditorGUILayout.ObjectField("Edited Planner", latestMod, typeof(FieldPlanner), false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Field Planner:", EditorStyles.boldLabel, GUILayout.Width(82));
            EditorGUILayout.HelpBox("Generator and placer of the Grid Area (Cells) to run Field Setup on", MessageType.None);
            //EditorGUILayout.LabelField("Generator and placer of the Grid Area / Cells to run Field Setup on");

            //FieldPlannerEditor.DrawFieldPlannerSelector(LatestFieldPlanner);

            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.HelpBox("  Field Planner: Generator and placer of grid to run Field Setup on", MessageType.None);

            if (LatestFieldPlanner == null)
                if (Selection.activeObject is FieldPlanner)
                {
                    LatestFieldPlanner = (FieldPlanner)Selection.activeObject;
                    mainScroll = Vector2.zero;
                }

            if (LatestFieldPlanner == null)
            {
                EditorGUILayout.HelpBox("Select some  'Field Planner'  through  'Build Planner Window'  to edit it here", MessageType.Info);
                GUILayout.Space(5);

                if (BuildPlannerWindow.Get == null)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("Open new Build Planner Window"))
                    {
                        BuildPlannerWindow.Init();
                    }
                }

                //flex = EditorGUILayout.Toggle(flex);
                mainScroll = Vector2.zero;

                EditorGUILayout.EndScrollView();

            }
            else
            {
                if (prem != LatestFieldPlanner)
                {
                    //FieldPlannerEditor.RefreshSpawnersList(latestMod);
                    mainScroll = Vector2.zero;
                }

                SerializedObject so = new SerializedObject(LatestFieldPlanner);
                //FieldPlannerEditor.DrawHeaderGUI(so, latestMod);

                bool pre = EditorGUIUtility.wideMode;
                bool preh = EditorGUIUtility.hierarchyMode;
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.hierarchyMode = true;

                if (flex)
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle, GUILayout.Height(2200));
                else
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle, GUILayout.ExpandHeight(true), GUILayout.MinHeight(position.height * 0.8f));

                if (drawGraph)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                }

                changed = FieldPlannerEditor.DrawGUI(LatestFieldPlanner, so, this, position);

                if (!flex) GUILayout.FlexibleSpace();

                if (!drawGraph) GUILayout.Space(5);

                EditorGUIUtility.hierarchyMode = preh;
                EditorGUIUtility.wideMode = pre;

                if (!drawGraph)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                }

            }

            prem = LatestFieldPlanner;

            if (forceChanged)
            {
                changed = true;
                forceChanged = false;
            }

            if (changed) BuildPlannerWindow.ForceUpdateView();

            PGGUtils.EndVerticalIfLightSkin();
        }

        public static void RefreshGraphView()
        {
            if (Get != null)
                if (Get.BuildPlannerGraphDraw != null)
                {
                    Get.BuildPlannerGraphDraw.ForceGraphRepaint();
                }
        }


        public Texture2D Tex_Net;
        public Texture2D Tex_Net2;

        private SerializedObject so_currentSetup = null;
        private PlannerGraphDrawer BuildPlannerGraphDraw = null;
        internal bool drawGraph = true;

        //public static bool AutoRefreshInitialShapePreview = true;

        internal PlannerGraphDrawer DrawGraph(bool endVerts, SerializedObject so, FieldPlanner.EViewGraph graphView)
        {
            if (so != null) so_currentSetup = so;

            if (LatestFieldPlanner != null)
            {
                if (so_currentSetup == null) so_currentSetup = new SerializedObject(LatestFieldPlanner);

                if (BuildPlannerGraphDraw == null || BuildPlannerGraphDraw.currentSetup.ScrObj != LatestFieldPlanner)
                {
                    BuildPlannerGraphDraw = new PlannerGraphDrawer(this, LatestFieldPlanner);
                }
            }

            if (BuildPlannerGraphDraw != null)
            {

                BuildPlannerGraphDraw.DrawedInsideInspector = true;
                BuildPlannerGraphDraw.displayPadding = new Vector4(5, 0, 12, 8);
                if (VignPlanner != null) BuildPlannerGraphDraw.AltVignette = VignPlanner;

                BuildPlannerGraphDraw.Tex_Net = Tex_Net;
                if (graphView == FieldPlanner.EViewGraph.PostProcedures_Cells)
                { BuildPlannerGraphDraw.Tex_Net = FieldPlannerWindow.Get.Tex_Net2; }


                BuildPlannerGraphDraw.Parent = this;
                BuildPlannerGraphDraw.DrawGraph();

                if (so_currentSetup != null)
                    if (BuildPlannerGraphDraw.AsksForSerializedPropertyApply)
                    {
                        so_currentSetup.ApplyModifiedProperties();
                        so_currentSetup.Update();
                        BuildPlannerGraphDraw.AsksForSerializedPropertyApply = false;
                    }
            }

            return BuildPlannerGraphDraw;
        }

        private void Update()
        {
            if (FGenerators.RefIsNull(BuildPlannerGraphDraw)) return;
            BuildPlannerGraphDraw.Update();

            if (BuildPlannerGraphDraw.CheckDisplayRepaintRequest(PlannerGraphWindow._RefreshDrawFlag))
                Repaint();
        }

    }
}
