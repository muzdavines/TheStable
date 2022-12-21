#if UNITY_EDITOR

// If UNITY_EDITOR for being able to define drawer in the main assembly,
// if there are just few gui elements to change, it's just more comfortable
// when creating new nodes and managing smaller ones

using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FIMSpace.Graph
{
    public partial class FGraph_NodeBase_Drawer
    {
        public enum EConnectorsWireMode { Left_Right, Up_Down }


        public virtual void ClampNodeFrameRectInWindow(ref Rect r, Rect graphAreaRect)
        {
            if (r.min.x < 0f) r.position = new Vector2(0f, r.position.y);
            //if (r.max.x > graphAreaRect.width) r.position = new Vector2(graphAreaRect.width - r.width, r.position.y);
            if (r.min.y < 0f) r.position = new Vector2(r.position.x, 0f);
            //if (r.max.y > graphAreaRect.height) r.position = new Vector2(r.position.x, graphAreaRect.height - r.height);
        }


        public virtual void DrawDebugProgress( string text = "")
        {
            Rect dbg = new Rect(baseGet._E_LatestRect);

            dbg.size = new Vector2(dbg.size.x * 0.6f, 6);
            dbg.position += new Vector2((baseGet._E_LatestRect.size.x - dbg.size.x) * 0.5f, baseGet._E_LatestRect.height - 17);
            //dbg.position += new Vector2(_E_LatestRect.size.x * 0.185f, _E_LatestRect.height-17);

            EditorGUI.ProgressBar(dbg, baseGet.DebuggingProgress, text);
        }


    }

}
#endif
