#if UNITY_EDITOR

// If UNITY_EDITOR for being able to define drawer in the main assembly,
// if there are just few gui elements to change, it's just more comfortable
// when creating new nodes and managing smaller ones

using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FIMSpace.Graph
{
    public partial class FGraph_NodeBase_Drawer
    {

        public bool NodeSelected { get; protected set; } = false;
        public bool NodeSelected_Graph { get; set; } = false;
        public virtual bool SelectNodeInspector { get;  } = true;

        public static FGraph_NodeBase_Drawer _E_SelectedNode = null;

        public static void SelectNodeSignal(FGraph_NodeBase node)
        {
            if (!(_E_SelectedNode is null) && _E_SelectedNode.baseGet != node) _E_SelectedNode.DeSelect();

            _E_SelectedNode = null;


            if ((node is null) == false)
            {
                FGraph_NodeBase_Drawer drawer = node._editorDrawer as FGraph_NodeBase_Drawer;

                if (drawer == null)
                {
                    UnityEngine.Debug.Log("Null Drawer!");
                    return;
                }

                _E_SelectedNode = drawer;
                _E_SelectedNode.Select();
                node.CheckForNulls();

                if (drawer.SelectNodeInspector) Selection.activeObject = node;
            }
        }


        public Vector2 NodePosition { get { return baseGet.NodePosition; } }
        public Vector2 NodeSize { get { return baseGet.NodeSize; } }
        public Vector2 NodeDrawOffset { get { return baseGet.NodeDrawOffset; } }

        [NonSerialized] public bool _Editor_RequestPostRefresh = false;

        public virtual void Select()
        {
            NodeSelected = true;
        }
        public virtual void DeSelect()
        {
            NodeSelected = false;
            NodeSelected_Graph = false;
        }



    }

}
#endif
