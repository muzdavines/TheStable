using FIMSpace.FEditor;
using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        int wasFirstDraw = -1;
        public Vector2 GraphSize = new Vector2(1280, 1024);

        public Vector2 graphDisplayOffset = Vector2.zero;
        public float graphZoom = 1f;
        public float graphTopMarginOffset = 100;
        public float TopMarginOffset
        {
            get { return graphTopMarginOffset ; }
        }

        protected GUIStyle graphNoOffsetStyle = null;
        protected Rect graphDisplayRect;
        protected Rect graphAreaRect;

        protected Vector2 eventMousePos;
        protected Vector2 eventMousePosOffsetted;
        protected Vector2 inGraphMousePos;

        /// <summary> Bounds are collected every all nodes draw loop </summary>
        public Bounds NodesBounds { get; protected set; }
        Rect upperLimitRect;

        protected virtual Vector2 MinCanvasSize { get { return new Vector2(1536, 768); } }
        protected virtual Vector2 CanvasExpandArea { get { return new Vector2(600, 500); } }

        protected virtual void PrepareGraphDraw()
        {
            wasRemovingNodes = false;

            if (refreshRequest)
            {
                OnGraphStructureChange();
                refreshRequest = false;
            }

            CheckScheduledEvents();

            isAnimating = false;
            displayPlaymodeGraph = false;
            if (Application.isPlaying) PreparePlaymodeNodes();

            CheckForSearchableAction();

            if (graphNoOffsetStyle == null) graphNoOffsetStyle = new GUIStyle();
            RefreshNetTextureReference();

            #region Dynamic Expanse of grid canvas (right and down)

            GraphSize.x = NodesBounds.max.x + CanvasExpandArea.x;
            GraphSize.y = NodesBounds.max.y + CanvasExpandArea.y;
            //
            if (GraphSize.x < MinCanvasSize.x) GraphSize.x = MinCanvasSize.x; // Minimum Width
            if (GraphSize.y < MinCanvasSize.y) GraphSize.y = MinCanvasSize.y; // Minimum Height

            #endregion

            if (graphDisplayOffset.x > GraphSize.x) graphDisplayOffset.x = GraphSize.x / 2f;
            if (graphDisplayOffset.y > GraphSize.y) graphDisplayOffset.y = GraphSize.y / 2f;

            graphAreaRect = new Rect(graphDisplayOffset.x, graphDisplayOffset.y, GraphSize.x, GraphSize.y);

            var content = new GUIContent(" _ ");
            upperLimitRect = GUILayoutUtility.GetRect(content, FGUI_Resources.BGInBoxStyle);
            if (upperLimitRect.position.y > 0f) graphTopMarginOffset = upperLimitRect.position.y;

            if (reloadConnections)
            {
                if (drawingNodes == null || drawingNodes.Count == 0) GetAllNodes();

                FGenerators.CheckForNulls(drawingNodes);

                for (int i = 0; i < drawingNodes.Count; i++)
                    if (drawingNodes[i].IndividualID == -1) drawingNodes[i].GenerateID(drawingNodes);

                for (int i = 0; i < drawingNodes.Count; i++)
                    drawingNodes[i].RefreshConnections(drawingNodes);

                reloadConnections = false;
            }

            graphStartDrawEvent = Event.current;
            if (graphStartDrawEvent != null) graphStartDrawCursorPos = graphStartDrawEvent.mousePosition;
        }

        public Vector4 displayPadding = Vector4.zero;
        public GUIStyle BGVignetteStyle = null;
        private Rect viewRect;
        public float AdjustTopOffset = 0f;
        protected virtual void DisplayGraphBody()
        {
            eventMousePos = Event.current.mousePosition;
            eventMousePosOffsetted = eventMousePos;
            eventMousePosOffsetted.y -= TopMarginOffset;
            inGraphMousePos = eventMousePosOffsetted;
            inGraphMousePos /= graphZoom; // To graph zoom
            inGraphMousePos -= graphDisplayOffset; // To graph offset

            float hh = position.height;
            //if (FixedHeight != null) hh = FixedHeight.Value;

            graphDisplayRect = new Rect(displayPadding.x, (TopMarginOffset) + displayPadding.y, position.width - displayPadding.z, (hh - (TopMarginOffset)) - displayPadding.w);
            viewRect = new Rect(new Vector2(-graphDisplayOffset.x, -graphDisplayOffset.y), (graphDisplayRect.size * 1f) / graphZoom);

            GUI.DrawTexture(graphDisplayRect, FGraphDrawerBase.Tex_BG_L, ScaleMode.StretchToFill);

            // Drawing tiles for background graph grid
            EditorGUIZoomArea.Begin(graphZoom, graphDisplayRect);

            GUI.BeginGroup(graphAreaRect);

            #region Grid Background

            if (Tex_Net != null)
            {
                if (Tex_Net.width < 4)
                {
                    GUI.DrawTexture(new Rect(0, 0, GraphSize.x, GraphSize.y), Tex_Net, ScaleMode.StretchToFill);
                }
                else
                {
                    int xRepetitions = Mathf.CeilToInt(GraphSize.x / Tex_Net.width);
                    int yRepetitions = Mathf.CeilToInt(GraphSize.y / Tex_Net.height);

                    for (int x = 0; x <= xRepetitions; x++)
                        for (int y = 0; y <= yRepetitions; y++)
                            GUI.Box(new Rect(x * (Tex_Net.width - 0), y * Tex_Net.height, Tex_Net.width, Tex_Net.height), Tex_Net, graphNoOffsetStyle);
                }
            }

            if (BGVignetteStyle == null)
            {
                BGVignetteStyle = new GUIStyle();
                BGVignetteStyle.normal.background = FGraphStyles.TEX_Vignette;
            }

            #endregion

            GUI.EndGroup();
            EditorGUIZoomArea.End();

            if (FGraph_NodeBase.RequestsConnectionsRefresh)
            {
                RefreshNodes();
                FGraph_NodeBase.RequestsConnectionsRefresh = false;
            }

            // Vignette
            GUI.color = new Color(1f, 1f, 1f, 0.85f);
            GUI.Box(graphDisplayRect, GUIContent.none, BGVignetteStyle);

            // Expanding Guide
            Rect expGuide = new Rect(graphDisplayRect);
            expGuide.size = new Vector2(16, 16);
            expGuide.position += new Vector2(graphDisplayRect.width - 12, graphDisplayRect.height - 16);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.Label(expGuide, new GUIContent("┘", "Graph is expansing it's size in right-down direction if you place nodes near to bottom or right side of the graph canvas"));

            GUI.color = Color.white;


            // Continue Drawing Graph
            EditorGUIZoomArea.Begin(graphZoom, graphDisplayRect);
            GUI.BeginGroup(graphAreaRect);
            GraphBeginDraw();

            Handles.BeginGUI();
            DrawConnections(); //
            Handles.EndGUI();


            Parent.BeginWindows();
            DrawNodes(); //
            Parent.EndWindows();


            GraphEndDraw();

            GUI.EndGroup();
            EditorGUIZoomArea.End();

            DrawGraphOverlay();

        }


        /// <summary>
        /// Always on top GUI elements
        /// </summary>
        protected virtual void DrawGraphOverlay()
        {
            Rect headerRect = new Rect(0f, TopMarginOffset, graphDisplayRect.width, 24);

            string info = "  Cursor Pos: " + eventMousePosOffsetted + "   In Graph: " + inGraphMousePos;
            GUI.Box(headerRect, new GUIContent(info), FGUI_Resources.BGInBoxStyle);
        }


        /// <summary> Nodes which will be displayed by graph drawer </summary>
        protected List<FGraph_NodeBase> nodesToDraw = new List<FGraph_NodeBase>();
        protected ScriptableObject nodesFrom = null;
        protected List<FGraph_NodeBase> GetAllNodes()
        {
            var setup = displayedGraphSetup;
            bool reset = false;

            if (nodesToDraw.Count != PresetNodesCount)
                reset = true;
            else
            if (nodesFrom == null)
                reset = true;

            if (reset)
            {
                nodesToDraw.Clear();
                FillListWithNodesToDraw(nodesToDraw);

                for (int i = 0; i < nodesToDraw.Count; i++)
                {
                    if (nodesToDraw[i] == null) continue;
                    nodesToDraw[i].Drawer(this).DeSelect();
                }

                RefreshNodes();
            }

            if (nodesToDraw.Count == 0) return null;

            nodesFrom = setup;

            return nodesToDraw;
        }


        public void ForceGraphRepaint()
        {
            repaintRequest = true;
        }

    }
}