using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    /// <summary>
    /// Utility class to be used in Editor Windows for displaying graph with nodes
    /// </summary>
    public abstract partial class FGraphDrawerBase
    {
        public EditorWindow Parent;

        /// <summary> EditorWindow GUI position (rect) </summary>
        public Rect position { get { return Parent.position; } }

        string _SearchableAddNodeId = "-";

        public static GUIStyle label;
        public static GUIStyle boldLabel;
        protected Color graphNodesTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        int afterInitializeFlag = 0;

        public FGraphDrawerBase(EditorWindow parent)
        {
            Parent = parent;
            OnGraphStructureChange();
            _SearchableAddNodeId = "PlanAdd-" + Parent.GetInstanceID();
        }

        public virtual void DrawGraph()
        {
            if (afterInitializeFlag == 2) { refreshRequest = true; RefreshNodes(); }

            // Temporarily Changing default styles colors
            Color preLblCol = EditorStyles.label.normal.textColor;
            Color preBoldLblCol = EditorStyles.boldLabel.normal.textColor;

            if (EditorGUIUtility.isProSkin == false)
            {
                label = EditorStyles.label;
                boldLabel = EditorStyles.boldLabel;
            }

            try
            {
                if (EditorGUIUtility.isProSkin == false)
                {
                    // Applying new temporary color for nodes (in case of light skin)
                    label.normal.textColor = graphNodesTextColor;
                    boldLabel.normal.textColor = graphNodesTextColor;
                }

                PrepareGraphDraw();
                DisplayGraphBody();
                UpdateGraphInput();
                RefreshNodePortsRects();
            }
            catch (System.Exception) { }


            if (EditorGUIUtility.isProSkin == false)
            {
                // Restoring default styles colors
                label.normal.textColor = preLblCol;
                boldLabel.normal.textColor = preBoldLblCol;
            }

            if (afterInitializeFlag < 4) afterInitializeFlag += 1;
        }

        /// <summary>
        /// Must be called with EditorWindow.Update() 
        /// Handling active drawing multiple nodes selection frame
        /// Handling active drawing node connection creation
        /// </summary>
        public virtual void Update()
        {
            if (isConnectingNodes || isSelectingMultiple || wasDragging)
            {
                Parent.Repaint();
                _dtForcingUpdate = true;
                if (wasDragging) ClampGraphPosition();
            }

            if (refreshAfterSelecting > 0)
            {
                Parent.Repaint();
                refreshAfterSelecting -= 1;
            }
        }
    }
}