using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        protected abstract int PresetNodesCount { get; }
        /// <summary> Get all nodes for drawing - useful if you keep nodes in multiple different lists, then you can generate this list </summary>
        protected abstract void FillListWithNodesToDraw(List<FGraph_NodeBase> willBeDrawed);


        /// <summary> Can't be null! Currently selected graph setup file </summary>
        public abstract ScriptableObject ProjectFilePreset { get; }
        /// <summary> Return null if you don't want to display different graph in playmode at all </summary>
        public virtual ScriptableObject DebugDrawPreset { get { return null; } }
        /// <summary> Compute it with: if want to draw instantiation of selected project preset return true, else return false </summary>
        public virtual bool CanDrawDebugPreset { get { return false; } }


        /// <summary>
        /// After override you must add "YourNodesList.Add(node);" before "base.AddNode(node);"
        /// </summary>
        public virtual void AddNewNodeToPreset(FGraph_NodeBase node, bool setPositionOnCursor = true)
        {
            if (setPositionOnCursor) node.NodePosition = GetNodeCreatePos();
            // YourNodesList.Add(node);
            OnAddNode(node);
            AddNodeToFile(node, ProjectFilePreset);
        }

        /// <summary>
        /// In addition you must add line 
        /// "YourNodeList.Remove(node as YourNodeBaseClass);" 
        /// before base.RemoveNode(node);
        /// </summary>
        protected virtual void RemoveNode(FGraph_NodeBase node)
        {
            //node.ClearPorts();
            OnGraphStructureChange();
            node.OnRemoving();

            RemoveNodeFromFile(node, ProjectFilePreset);
            SetDirty();

            wasRemovingNodes = true;
        }



        protected List<FGraph_NodeBase> requireNodeSerializeUpdateNodes = new List<FGraph_NodeBase>();
        protected virtual void DrawNode(FGraph_NodeBase node)
        {
            try
            {

                #region Draw GUI Area for node

                FGraph_NodeBase_Drawer dr = null;

                if (node.isCulled == false)
                {
                    dr = node.Drawer(this);

                    #region Check input forwarding

                    if (Event.current != null)
                        if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp)
                            if (isMouseCursorInGraph == false)
                            {
                                if (IsCursorInAdditionalActionArea())
                                {
                                    cursorOutOfGraphEventForward = new Event(Event.current);
                                    Event.current = null;
                                }

                                return;
                            }

                    #endregion

                    if (node.Ports_CheckIfConnectionsChanged()) refreshRequest = true;

                    UnityEditor.EditorGUI.BeginChangeCheck();

                    node.Editor_PreBody();

                    GUILayout.BeginArea(dr.GetGuiBodyRect());

                    node.IsDrawingGUIInNodeMode = true;
                    GUILayout.Space(1);

                    node.Editor_OnNodeBodyGUI(displayedGraphSetup);
                    node.IsDrawingGUIInNodeMode = false;


                    #region Resizable Handle Draw and Rect

                    if (node.IsResizable)
                    {
                        Color preC = GUI.color;
                        Rect r = new Rect();
                        r.size = new Vector2(20, 20);
                        r.position = GetScallableHanglePositionOffset(node);
                        GUI.color = new Color(1f, 1f, 1f, 0.375f);
                        GUI.Label(r, "┘");
                        GUI.color = preC;
                    }

                    #endregion


                    if (UnityEditor.EditorGUI.EndChangeCheck())
                    {
                        node._E_SetDirty();

                        if (node.IsFoldableFix)
                        {
                            node.baseSerializedObject.ApplyModifiedProperties();
                            requireNodeSerializeUpdateNodes.Add(node);
                        }
                    }

                    GUILayout.EndArea();


                    dr = node.Drawer(this);

                    #endregion

                    HandleInputsForNode(node);

                    node._EditorWasDrawn = true;
                }

                #region Construct Draw Bounds

                if (!(node is null))
                {
                    Bounds b = NodesBounds;
                    b.Encapsulate(new Bounds(node.NodePosition + node.NodeSize / 2, node.NodeSize));
                    NodesBounds = b;

                    if (dr != null) if (dr._Editor_RequestPostRefresh) { repaintRequest = true; dr._Editor_RequestPostRefresh = false; }
                    if (selectedNode == node) node.Drawer(this).NodeSelected_Graph = true;
                }

                #endregion

                // Check for changes
                if (node._editorForceChanged)
                {
                    node._editorForceChanged = false;
                }
            }
            catch (System.Exception exc)
            {
                if (!_Editor_IsExceptionIgnored(exc))
                {
                    //UnityEngine.Debug.Log("ign");
                    //UnityEngine.Debug.Log("id " + exc.HResult);
                    throw;
                }
                else
                {
                    return;
                }
            }

        }

        protected Vector2 GetScallableHanglePositionOffset(FGraph_NodeBase node)
        {
            return new Vector2(node.NodeSize.x - 61, node.NodeSize.y - 72);
        }

        public static bool _Editor_IsExceptionIgnored(System.Exception exc)
        {
            if (exc == null) return true;

            if (exc.HResult == -2147024809 || exc.HResult == -2146233088) return true;

            return false;
        }

        /// <summary>
        /// Get Drawer for node graph with custom styles.
        /// </summary>
        public virtual FGraph_NodeBase_Drawer GetNodeDrawer(FGraph_NodeBase node)
        {
            return new FGraph_NodeBase_Drawer(node);
        }

    }
}