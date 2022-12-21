using FIMSpace.FEditor;
using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        /// <summary>
        /// No need to override if you don't need fully custom node add menu
        /// By default it's using namespace of default node type to generate menus
        /// </summary>
        protected virtual void NodeAddMenu(Event e)
        {

#if UNITY_2019_4_OR_NEWER
            ShowNodeAddSearchableMenu();
#else
            ShowNodeAddGenericMenu();
#endif
            //menu.AddItem(new GUIContent("Add Some Node"), false, () =>
            //{
            //    // Your code
            //});

            //menu.AddItem(GUIContent.none, false, () => { });

            //menu.AddItem(new GUIContent("Add Other Node"), false, () =>
            //{
            //    // Your code
            //});

        }

#if UNITY_2019_4_OR_NEWER
        List<FGraph_NodeBase> searchable_nodes = new List<FGraph_NodeBase>();
        List<string> searchable_names = new List<string>();

        protected virtual void ShowNodeAddSearchableMenu(string title = "Add New Node")
        {
            searchable_nodes.Clear();
            searchable_names.Clear();

            var nodes = GetNodesByNamespace();

            for (int i = 0; i < nodes.Count; i++)
            {
                searchable_nodes.Add(nodes[i].node);
                searchable_names.Add(nodes[i].name);
            }

            SearchableDropdown<FGraph_NodeBase> nodeMenu = new SearchableDropdown<FGraph_NodeBase>(searchable_nodes, searchable_names, title);
            SetDropID(_SearchableAddNodeId);
            DisplayMenuUnscaled(nodeMenu);
        }


#endif


#if UNITY_2019_4_OR_NEWER
        public static string SetDropID(string id) { return SearchableDropdown<FGraph_NodeBase>.ChoosingHelperID = id; }
        public static string GetDropID() { return SearchableDropdown<FGraph_NodeBase>.ChoosingHelperID; }
#else
        public static string SetDropID(string id) { return Searchable.ChoosingHelperID = id; }
        public static string GetDropID() { return Searchable.ChoosingHelperID; }
#endif

        protected virtual void CheckForSearchableAction()
        {
            string dropID = GetDropID();
            if (string.IsNullOrEmpty(dropID)) return;

            if (Searchable.IsSetted)
                if (dropID == _SearchableAddNodeId)
                {
                    FGraph_NodeBase nNode = Searchable.Get<FGraph_NodeBase>(true);

                    if (nNode != null)
                    {
                        AddNewNodeToPreset( ScriptableObject.Instantiate(nNode));
                        SetDropID("");
                    }
                }
        }

        private void ShowNodeAddGenericMenu()
        {
            GenericMenu menu = new GenericMenu();
            FillGenericMenuWithAutomaticDetectedNodesByNamespace(menu);
            DisplayMenuUnscaled(menu);
        }

        protected virtual void NodeModifyMenu(Event e, FGraph_NodeBase node)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("REMOVE Node"), false, () =>
            {
                // Remove code
                RemoveNode(node);
            });

            menu.AddItem(new GUIContent("Rename Node"), false, () =>
            {
                string newName = FGenerators.RenamePopup(null, node.NameID);
                if (string.IsNullOrEmpty(newName) == false) { node.NameID = newName; /*SetDirty*/ }
            });


            //menu.AddItem(new GUIContent("[Debugging] Switch Debug Variable"), node._EditorDebugMode, () =>
            //{
            //    node._EditorDebugMode = !node._EditorDebugMode;
            //});

            DisplayMenuUnscaled(menu);
        }


        protected virtual void ConnectionModifyMenu(Event e, FGraph_TriggerNodeConnection node)
        {
            if (node is null) return;
            if (node.From is null) return;
            if (node.To is null) return;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("REMOVE Connection"), false, () =>
            {
                // Remove code
                node.From.RemoveConnectionWith(node.To);
            });

            DisplayMenuUnscaled(menu);
        }

        public void DisplayMenuUnscaled(GenericMenu menu)
        {
            Matrix4x4 preM = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;
            menu.ShowAsContext();
            GUI.matrix = preM;
        }

#if UNITY_2019_4_OR_NEWER

        public void DisplayMenuUnscaled<T>(SearchableDropdown<T> drop) where T : class
        {
            Matrix4x4 preM = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;
            drop.Show(new Rect(Event.current.mousePosition + new Vector2(5, -50), new Vector2(260, 1))); // size y = 1 solves issue with yPositioning
            GUI.matrix = preM;
        }
#endif

        public static Rect GetCursorPosRect(Vector2 offset, Vector2 size)
        {
            return new Rect(Event.current.mousePosition + offset, size);
        }
    }
}