using FIMSpace.Generating;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Rules.QuickSolutions;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public class ModGraphWindow : EditorWindow
    {

        public static ModGraphWindow Get;
        public Texture2D VignPlanner;

        #region Initialize and show window

        public static void Init(SR_ModGraph planner)
        {
            ModGraphWindow alreadyWindow = (ModGraphWindow)GetWindow(typeof(ModGraphWindow));
            ModGraphWindow window = alreadyWindow;

#if UNITY_2019_4_OR_NEWER
            if (alreadyWindow)
                window = alreadyWindow;
            else
                window = CreateWindow<ModGraphWindow>();

            if (window.currentFunction != null)
            {
                window = CreateWindow<ModGraphWindow>();
            }
#endif

            window.titleContent = new GUIContent("Mod Graph", PGGUtils.TEX_ModGraphIcon, "Field Modificator Graph Window");
            window.Show();

            Rect p = window.position;
            if (p.size.x < 400) { p.size += new Vector2(140, 0); }
            if (p.size.y < 300) p.size += new Vector2(0, 100);
            window.position = p;
            window.currentContainer = null;
            window.currentFunction = null;
            window.currentSetup = planner;

            Get = window;

            SR_ModGraph.Graph_ModGraph = planner;
        }

        public static void Init(PlannerFunctionNode functionNode)
        {
            ModGraphWindow alreadyWindow = (ModGraphWindow)GetWindow(typeof(ModGraphWindow));
            ModGraphWindow window = alreadyWindow;

#if UNITY_2019_4_OR_NEWER
            if (alreadyWindow)
                window = alreadyWindow;
            else
                window = CreateWindow<ModGraphWindow>();
#endif

            window.titleContent = new GUIContent("Mod Func", Resources.Load<Texture2D>("SPR_PlanSmall"), "Build Planner Function - Graph Window");
            window.Show();

            Rect p = window.position;
            if (p.size.x < 300) { p.size += new Vector2(100, 0); }
            if (p.size.y < 200) p.size += new Vector2(0, 100);
            window.position = p;

            if (functionNode.Tex_Net != null) window.Tex_Net = functionNode.Tex_Net;

            window.currentContainer = null;
            window.currentSetup = null;
            window.currentFunction = functionNode;
        }

#endregion


#region Open File with double click - open graph window


#endregion


        public Texture2D Tex_Net;
        public SR_ModGraph currentSetup;
        public PlannerFunctionNode currentFunction;
        public IPlanNodesContainer currentContainer;
        private SerializedObject so_currentSetup = null;
        private ModGraphDrawer BuildPlannerGraphDraw;


        private void OnGUI()
        {
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));

            if (currentContainer == null)
            {
                if (currentFunction != null) currentContainer = currentFunction;
                else currentContainer = currentSetup;
            }

            if (currentContainer != null)
            {
                if (so_currentSetup == null) so_currentSetup = new SerializedObject(currentContainer.ScrObj);

                if (BuildPlannerGraphDraw == null || BuildPlannerGraphDraw.currentSetup != currentContainer)
                {
                    BuildPlannerGraphDraw = new ModGraphDrawer(this, currentContainer);
                }
            }

            if (BuildPlannerGraphDraw != null)
            {

                    BuildPlannerGraphDraw.Tex_Net = Tex_Net;
             
                if (currentSetup != null) if (VignPlanner != null) BuildPlannerGraphDraw.AltVignette = VignPlanner;


                BuildPlannerGraphDraw.Parent = this;

                if (currentSetup)
                {
                    BuildPlannerGraphDraw.RefreshTitle();
                }
                else if (currentFunction)
                {
                    if (BuildPlannerGraphDraw.TopTitle.Length != currentFunction.GetDisplayName().Length)
                    {
                        BuildPlannerGraphDraw.TopTitle = currentFunction.GetDisplayName();
                    }
                }

                BuildPlannerGraphDraw.DrawGraph();

                if (so_currentSetup != null)
                    if (BuildPlannerGraphDraw.AsksForSerializedPropertyApply)
                    {
                        so_currentSetup.ApplyModifiedProperties();
                        so_currentSetup.Update();

                        BuildPlannerGraphDraw.AsksForSerializedPropertyApply = false;
                    }
            }
        }

        private void Update()
        {
            if (BuildPlannerGraphDraw != null)
            {
                BuildPlannerGraphDraw.Update();

                if (BuildPlannerGraphDraw.CheckDisplayRepaintRequest(_RefreshDrawFlag))
                    Repaint();
            }
        }


        // Styles --------------------------


#region Graph Helper Graphics

        public static PlannerGraphStyles Styles { get { return _styles != null ? _styles : _styles = new PlannerGraphStyles(); } }
        private static PlannerGraphStyles _styles = null;
        public static double _RefreshDrawFlag = -1;

        public class PlannerGraphStyles
        {
            public GUIStyle nodeHeaderText, nodeHeader, nodeBody, nodeExBody, nodeExBodyHighlight, bodyHighlight;

#region Textures References

            public static Texture2D TEX_Gradient1 { get { return _tex_grad1 != null ? _tex_grad1 : _tex_grad1 = Resources.Load<Texture2D>("SPR_PlannerGradient.fw"); } }
            private static Texture2D _tex_grad1;

            public static Texture2D TEX_nodeBody { get { return _tex_nodeBody != null ? _tex_nodeBody : _tex_nodeBody = Resources.Load<Texture2D>("SPR_PlannerNodeBody.fw"); } }
            private static Texture2D _tex_nodeBody;
            public static Texture2D TEX_nodeExBody { get { return _tex_nodeExBody != null ? _tex_nodeExBody : _tex_nodeExBody = Resources.Load<Texture2D>("SPR_PlannerNodeExec.fw"); } }
            private static Texture2D _tex_nodeExBody;
            public static Texture2D TEX_nodeExHighlight { get { return _tex_nodeExHghl != null ? _tex_nodeExHghl : _tex_nodeExHghl = Resources.Load<Texture2D>("SPR_PlannerNodeExecHighlight.fw"); } }
            private static Texture2D _tex_nodeExHghl;
            public static Texture2D TEX_nodeHeader { get { return _tex_nodeHeader != null ? _tex_nodeHeader : _tex_nodeHeader = Resources.Load<Texture2D>("SPR_PlannerNodeHeader.fw"); } }
            private static Texture2D _tex_nodeHeader;
            public static Texture2D TEX_nodeHighlight { get { return _tex_nodeHighlight != null ? _tex_nodeHighlight : _tex_nodeHighlight = Resources.Load<Texture2D>("SPR_PlannerNodeHighlight.fw"); } }
            private static Texture2D _tex_nodeHighlight;

            public static Texture2D TEX_nodeInput { get { return _tex_nodeInput != null ? _tex_nodeInput : _tex_nodeInput = Resources.Load<Texture2D>("SPR_PlannerNodeInput.fw"); } }
            private static Texture2D _tex_nodeInput;
            public static Texture2D TEX_nodeOutput { get { return _tex_nodeOutput != null ? _tex_nodeOutput : _tex_nodeOutput = Resources.Load<Texture2D>("SPR_PlannerNodeOutput.fw"); } }
            private static Texture2D _tex_nodeOutput;

#endregion

            public PlannerGraphStyles()
            {
                nodeHeaderText = new GUIStyle(EditorStyles.boldLabel);
                nodeHeaderText.fontSize = 12;
                nodeHeaderText.alignment = TextAnchor.UpperCenter;
                nodeHeaderText.padding = new RectOffset(8, 8, 10, 0);


                nodeHeader = new GUIStyle(nodeHeaderText);
                nodeHeader.normal.background = TEX_nodeHeader;
                nodeHeader.border = new RectOffset(16, 16, 16, 16);


                nodeBody = new GUIStyle();
                nodeBody.normal.background = TEX_nodeBody;
                nodeBody.border = new RectOffset(32, 32, 20, 20);
                nodeBody.padding = new RectOffset(20, 20, 10, 10);

                nodeExBody = new GUIStyle(nodeHeader);
                nodeExBody.normal.background = TEX_nodeExBody;
                nodeExBody.alignment = TextAnchor.MiddleCenter;
                nodeExBody.border = new RectOffset(20, 20, 20, 20);
                nodeExBody.padding = nodeBody.padding;

                nodeExBodyHighlight = new GUIStyle(nodeExBody);
                nodeExBodyHighlight.normal.background = TEX_nodeExHighlight;

                bodyHighlight = new GUIStyle(nodeBody);
                bodyHighlight.normal.background = TEX_nodeHighlight;
                bodyHighlight.border = new RectOffset(24, 24, 26, 26);
            }
        }

#endregion


    }
}