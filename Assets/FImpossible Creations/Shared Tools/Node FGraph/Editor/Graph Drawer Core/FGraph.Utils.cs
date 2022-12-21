using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        /// <summary> If any containable is present in the graph </summary>
        bool _containablesPresent = false;
        public virtual void RefreshNodes()
        {
            if (drawingNodes == null) return;

            _containablesPresent = false;

            // Check nulls
            for (int i = drawingNodes.Count - 1; i >= 0; i--)
            {
                if ((drawingNodes[i] is null)) drawingNodes.RemoveAt(i);
            }
            
            // Generating IDs if not assigned and checking for null references
            for (int i = 0; i < drawingNodes.Count; i++)
            {
                if (drawingNodes[i].IndividualID == -1) drawingNodes[i].GenerateID(drawingNodes);
                drawingNodes[i].RefreshConnections(drawingNodes);
                drawingNodes[i].CheckForNulls();

                if (drawingNodes[i].IsContainable)
                {
                    _containablesPresent = true;
                    var node = drawingNodes[i];
                    drawingNodes.RemoveAt(i);
                    drawingNodes.Insert(0, node);
                }
            }
        }


        public FGraph_NodeBase FindNodeOfType(System.Type type)
        {
            var nodes = GetAllNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                if (nodes[i].GetType() == type) return nodes[i];
            }

            return null;
        }


        public virtual void ResetGraphPosition()
        {
            graphZoom = 1f;
            graphDisplayOffset.x = -GraphSize.x / 2f + position.width / 2f;
            graphDisplayOffset.y = -GraphSize.y / 2f + position.height / 2f;

            GetAllNodes();

            if (drawingNodes != null)
                if (drawingNodes.Count > 0)
                {
                    Bounds b = new Bounds(drawingNodes[0].NodePosition + drawingNodes[0].NodeSize / 2, drawingNodes[0].NodeSize);
                    for (int i = 1; i < drawingNodes.Count; i++)
                        b.Encapsulate(new Bounds(drawingNodes[i].NodePosition + drawingNodes[i].NodeSize / 2f, drawingNodes[i].NodeSize));

                    graphDisplayOffset = -b.center + new Vector3(position.width / 2f, position.height / 2f);
                }

            graphDisplayOffset.y -= TopMarginOffset * 0.5f;

            ClampGraphPosition();
        }

        protected virtual void OnAddNode(FGraph_NodeBase node)
        {
            node.OnCreated();
            OnGraphStructureChange();
            node.RefreshNodeParams();
        }

        public virtual void SetDirty()
        {
            if (CanDrawDebugPreset) if (DebugDrawPreset != null) EditorUtility.SetDirty(DebugDrawPreset);
            if (ProjectFilePreset != null) EditorUtility.SetDirty(ProjectFilePreset);
        }


        protected virtual void OnAddConnection(FGraph_TriggerNodeConnection conn)
        {
            OnGraphStructureChange();
        }

        protected virtual void OnRemoveConnection(FGraph_TriggerNodeConnection conn)
        {
            OnGraphStructureChange();
        }

        public virtual void ClampGraphPosition()
        {
            Vector2 clampAreaSize = graphAreaRect.size;

            if (graphAreaRect.size == Vector2.zero)
            {
                clampAreaSize = new Vector2(1280, 1024);
            }

            if (graphDisplayOffset.x > 0) graphDisplayOffset.x = 0f;
            if (graphDisplayOffset.y > 0) graphDisplayOffset.y = 0f;

            if (graphDisplayOffset.x - position.width / graphZoom < -clampAreaSize.x) { graphDisplayOffset.x = Mathf.Min(-GraphSize.x + position.width / graphZoom, 0f); }
            if (graphDisplayOffset.y - (position.height - TopMarginOffset) / graphZoom < -clampAreaSize.y) { graphDisplayOffset.y = Mathf.Min(-GraphSize.y + (position.height - TopMarginOffset) / graphZoom, 0f); }
        }



        /// <summary>
        /// To be executed on the beginning of the next gui redraw
        /// </summary>
        public static void ScheduleEditorEvent(System.Action ac)
        {
            EditorEvents.Add(ac);
        }

        static List<System.Action> EditorEvents = new List<System.Action>();
        public static void UseEditorEvents()
        {
            if (EditorEvents.Count == 0) return;

            for (int i = 0; i < EditorEvents.Count; i++)
            {
                if (EditorEvents[i] != null) EditorEvents[i].Invoke();
            }

            EditorEvents.Clear();
        }

        protected static void CheckScheduledEvents()
        {
            if (Event.current != null) if (Event.current.type == EventType.Layout) UseEditorEvents();
        }
    }
}