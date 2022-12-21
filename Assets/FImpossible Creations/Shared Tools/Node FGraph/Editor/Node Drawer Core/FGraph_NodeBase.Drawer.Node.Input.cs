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

        public virtual bool InteractionAreaContainsCursor(Vector2 inGraphMousePos)
        {
            if (RightConnectorContainsCursor(inGraphMousePos)) return true;
            if (OutputPortContainsCursor(inGraphMousePos) > -1) return true;
            if (LeftConnectorContainsCursor(inGraphMousePos)) return true;
            if (InputPortContainsCursor(inGraphMousePos) > -1) return true;
            return false;
        }

        public virtual int InputPortContainsCursor(Vector2 inGraphMousePos)
        {
            var ports = baseGet.GetInputPorts();
            if (ports != null)
            {
                for (int i = 0; i < ports.Count; i++)
                {
                    if (ports[i].PortClickAreaRect.Contains(inGraphMousePos)) return i;
                }
            }

            return -1;
        }

        public virtual int OutputPortContainsCursor(Vector2 inGraphMousePos)
        {
            var ports = baseGet.GetOutputPorts();
            if (ports != null)
            {
                for (int i = 0; i < ports.Count; i++)
                {
                    if (ports[i].PortClickAreaRect.Contains(inGraphMousePos)) return i;
                }
            }

            return -1;
        }

        public bool LeftConnectorContainsCursor(Vector2 inGraphMousePos)
        {
            Rect connRect = GetBaseConnectorInputRect(baseGet._E_LatestRect);
            //connRect.size *= new Vector2(2f, 3f);
            //connRect.position -= new Vector2(connRect.size.x / 1.8f, connRect.size.y / 2f);

            connRect.size += new Vector2(20, 10);
            connRect.position -= new Vector2(12, 5);

            return connRect.Contains(inGraphMousePos);
        }

        public bool RightConnectorContainsCursor(Vector2 inGraphMousePos)
        {
            Rect connRect = GetBaseConnectorOutputRect(baseGet._E_LatestRect);


            connRect.size += new Vector2(22, 10);
            connRect.position -= new Vector2(12, 5);

            //connRect.size *= new Vector2(2f, 3f);
            //connRect.position += new Vector2(connRect.size.x / 1.8f, -connRect.size.y / 2f);
            return connRect.Contains(inGraphMousePos);
        }

    }

}
#endif
