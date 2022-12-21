#if UNITY_EDITOR

// If UNITY_EDITOR for being able to define drawer in the main assembly,
// if there are just few gui elements to change, it's just more comfortable
// when creating new nodes and managing smaller ones

using UnityEngine;

namespace FIMSpace.Graph
{
    public partial class FGraph_NodeBase_Drawer
    {
        public Rect _E_LatestOutputRect { get; set; }
        public Rect _E_LatestInputRect { get; set; }

        public Rect _E_BodyRect { get; private set; }
        public Rect _E_HeaderRect { get; private set; }


        public virtual Rect GetBaseConnectorInputClickAreaRect()
        {
            Rect inRect = new Rect(_E_LatestInputRect);
            inRect.size *= 2f;
            inRect.position += new Vector2(-_E_LatestInputRect.width + 6, -_E_LatestInputRect.height / 2);
            return inRect;
        }

        /// <summary> Implement for multiple connectors support </summary>
        public virtual Rect GetBaseConnectorInputClickAreaRect(int multipleImplementedID)
        { return GetBaseConnectorInputClickAreaRect(); }

        public virtual Rect GetBaseConnectorOutputClickAreaRect()
        {
            Rect area = new Rect(_E_LatestOutputRect);
            area.size *= 2f;
            area.position += new Vector2(-6, -_E_LatestOutputRect.height / 2);
            return area;
        }

        /// <summary> Implement for multiple connectors support </summary>
        public virtual Rect GetBaseConnectorOutputClickAreaRect(int multipleImplementedID)
        { return GetBaseConnectorOutputClickAreaRect(); }

        //public virtual Rect GetBaseConnectorInputRectC()
        //{
        //    Rect area = new Rect(_E_LatestOutputRect);
        //    area.size *= 2f;
        //    area.position += new Vector2(-2, -_E_LatestOutputRect.height / 2);
        //    return area;
        //}

        protected virtual Vector2 _OutputConnectorOffset => Vector2.zero;
        public virtual Rect GetBaseConnectorOutputRect(Rect referenceRect)
        {
            Rect outputRect = new Rect(NodePosition, new Vector2(21, 21));
            outputRect.position += new Vector2(referenceRect.width - 20, 7) + _OutputConnectorOffset;
            _E_LatestOutputRect = outputRect;
            return outputRect;
        }

        /// <summary> Implement for multiple connectors support </summary>
        public virtual Rect GetBaseConnectorOutputRect(Rect referenceRect, int multipleImplementedID)
        { return GetBaseConnectorOutputRect(referenceRect); }

        public virtual Rect GetFrameBodyHighlightRect(Rect referenceRect)
        {
            Rect highlightRect = new Rect(referenceRect);
            highlightRect.position += new Vector2(-5, -4);
            highlightRect.width += 8;
            highlightRect.height += 8;
            return highlightRect;
        }

        public virtual Rect GetFrameBodyHighlightMultiSelectedRect(Rect referenceRect)
        {
            Rect highlightRect = new Rect(GetFrameBodyHighlightRect(referenceRect));
            highlightRect.position += new Vector2(2, -14);
            highlightRect.size += new Vector2(-4, 14);
            return highlightRect;
        }

        protected virtual Vector2 _InputConnectorOffset => Vector2.zero;
        public virtual Rect GetBaseConnectorInputRect(Rect referenceRect)
        {
            Rect inputRect = new Rect(NodePosition, new Vector2(21, 21));
            inputRect.position += new Vector2(1, 7) + _InputConnectorOffset;
            _E_LatestInputRect = inputRect;
            return inputRect;
        }
        /// <summary> Implement for multiple connectors support </summary>
        public virtual Rect GetBaseConnectorInputRect(Rect referenceRect, int multipleImplementedID)
        { return GetBaseConnectorInputRect(referenceRect); }

        public virtual Rect GetDragRect()
        {
            Rect dragRect = baseGet._E_LatestRect;
            dragRect.height = 70f;
            dragRect.position += Vector2.right * 20;
            dragRect.width -= 40;
            return dragRect;
        }


        public virtual Rect GetGuiBodyRect()
        {
            return new Rect(24, 34, baseGet.NodeSize.x - 50, baseGet.NodeSize.y - 34);
        }


        public virtual Vector2 GetOutputConnectorPinPosition(int multipleConnectorsIDHelper = -1)
        {
            return _E_LatestOutputRect.center;
        }

        public virtual Vector2 GetInputConnectorPinPosition(int multipleConnectorsIDHelper = -1)
        {
            return _E_LatestInputRect.center;
        }


    }

}
#endif
