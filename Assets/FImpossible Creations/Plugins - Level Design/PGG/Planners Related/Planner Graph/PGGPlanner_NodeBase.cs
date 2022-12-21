using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planner.Nodes
{
    public abstract class PGGPlanner_NodeBase : FGraph_NodeBase
    {
        public static bool AutoSnap = true;

        [HideInInspector]
        public bool Enabled = true;


        /// <summary> For nodes dropdown view </summary>
        public virtual string CustomPath { get { return ""; } }

#if UNITY_EDITOR
        public override string EditorCustomMenuPath() {  return CustomPath; }
#endif

        /// <summary> For nodes dropdown view, by default is Uncategorized </summary>
        public virtual EPlannerNodeType NodeType { get { return EPlannerNodeType.Uncategorized; } }
        /// <summary> For nodes dropdown view </summary>
        public virtual EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.All; } }
        public virtual bool DrawInspector { get { return false; } }

        public enum EPlannerNodeType { Uncategorized, Externals, Math, ReadData, WholeFieldPlacement, CellsManipulation,
            Logic, Debug,
            Cosmetics, ModGraphNode
        }
        public enum EPlannerNodeVisibility { All, JustPlanner, JustFunctions, Hidden }

        public override Color GetNodeColor()
        {
            return new Color(0.55f, 0.55f, 0.55f, 0.85f);
        }

        public override void OnEndDrag()
        {
            base.OnEndDrag();

            if (OutputConnections.Count > 0)
            {
                var other = OutputConnections[0].GetOther(this);
                Rect myBounds = new Rect(NodePosition, NodeSize * 0.75f);
                Rect oBounds = new Rect(other.NodePosition, other.NodeSize * 0.75f);

                if ( myBounds.Overlaps(oBounds))
                {
                    AlignViewedNodeWith(other, true);
                    return;
                }
            }

            if (InputConnections.Count > 0)
            {
                var other = InputConnections[0].GetOther(this);
                Rect myBounds = new Rect(NodePosition, NodeSize * 0.7f);
                Rect oBounds = new Rect(other.NodePosition, other.NodeSize * 0.7f);

                if (myBounds.Overlaps(oBounds))
                {
                    AlignViewedNodeWith(other, false);
                    base.OnEndDrag();
                }
            }

        }

        public override FGraph_TriggerNodeConnection CreateConnectionWith(FGraph_NodeBase otherNode, bool connectingFromOut, int fromAltID = -1, int toAltID = -1)
        {
            var c = base.CreateConnectionWith(otherNode, connectingFromOut, fromAltID, toAltID);
            //if (AutoSnap) AlignViewedNodeWith(otherNode, connectingFromOut);
            return c;
        }

        /// <param name="belowOrAbove"> True is above false is below </param>
        public void AlignViewedNodeWith(FGraph_NodeBase other, bool belowOrAbove = true)
        {
            if ( belowOrAbove)
            {
                NodePosition = other.NodePosition;
                float xDiff = NodeSize.x / 2f - other.NodeSize.x / 2f;
                NodePosition.x -= xDiff;
                NodePosition.y = other.NodePosition.y - NodeSize.y + 22;
                base.OnEndDrag();
            }
            else
            {
                NodePosition = other.NodePosition;
                float xDiff = NodeSize.x / 2f - other.NodeSize.x / 2f;
                NodePosition.x -= xDiff;
                NodePosition.y = other.NodePosition.y + other.NodeSize.y - 22;
            }
        }


    }
}