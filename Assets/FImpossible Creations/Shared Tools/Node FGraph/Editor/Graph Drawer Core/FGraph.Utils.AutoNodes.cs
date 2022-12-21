using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {

        /// <summary> Used for auto-nodes to choose create menu. Can be overrided for custom path. </summary>
        public virtual string GetNodesNamespace { get { return GetBaseNodeType.Namespace; } }
        /// <summary> Base type of the nodes used in this graph. It's namespace will be used for auto-node menu </summary>
        public abstract Type GetBaseNodeType { get; }

       protected bool nodesListRequiresRefresh { get { return types == null; } }

        protected List<Type> types = null;
        public List<Type> GetAndSetupNodeTypesByNamespace(string rootPathNamespace = "YourNamespace.Graph.Nodes", string altNamespace = "")
        {
            if (string.IsNullOrEmpty(altNamespace)) { if (nodesListRequiresRefresh) types = GetNodeTypesByNamespace(rootPathNamespace, altNamespace); }
            else return GetNodeTypesByNamespace(rootPathNamespace, altNamespace);
            return types;
        }

        /// <summary>
        /// GetAndSetupNodeTypesByNamespace for some optimization
        /// </summary>
        public List<Type> GetNodeTypesByNamespace(string rootPathNamespace = "YourNamespace.Graph.Nodes", string altNamespace = "")
        {
            List<Type> types = new List<Type>();

            string nmspc = GetNodesNamespace;
            if (!string.IsNullOrEmpty(altNamespace)) nmspc = altNamespace;
            foreach (Type t in GetDerivedTypes(typeof(FGraph_NodeBase)))
            {
                if (t == typeof(FGraph_NodeBase)) continue; // Ignore base class
                if (t.Namespace.StartsWith(nmspc) == false) continue; // Ignore other namespace nodes

                string path = GetPathNameForGenericMenu(t, rootPathNamespace);
                if (string.IsNullOrEmpty(path)) continue;
                types.Add(t);
            }

            return types;
        }

        public virtual void FillGenericMenuWithAutomaticDetectedNodesByNamespace(GenericMenu menu)
        {
            var nodes = GetNodesByNamespace();

            for (int i = 0; i < nodes.Count; i++)
            {
                var nNode = nodes[i].node;
                menu.AddItem(new GUIContent(nodes[i].name), false, () =>
                {
                    AddNewNodeToPreset(nNode);
                });
            }
        }

        private List<NodeRef> _assemblyNodes = new List<NodeRef>();

        /// <param name="baseNamespace"> shorter path, categories root </param>
        public List<NodeRef> GatherNodesByNamespace(string baseNamespace = "", string nodeNamespace = "")
        {
            List<NodeRef> _assemblyNodes = new List<NodeRef>();
            List<Type> types = GetAndSetupNodeTypesByNamespace(baseNamespace, nodeNamespace);

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type == typeof(FGraph_NodeBase)) continue;

                string path = GetPathNameForGenericMenu(type, baseNamespace);
                if (string.IsNullOrEmpty(path)) continue;

                string name = path;
                FGraph_NodeBase node = ScriptableObject.CreateInstance(type) as FGraph_NodeBase;
                
                if (string.IsNullOrEmpty(node.EditorCustomMenuPath()) == false)
                {
                    name = node.EditorCustomMenuPath() + "/" + node.GetDisplayName();
                }
                else
                {
                    if (node != null) name = path.Replace(node.GetType().Name, "") + node.GetDisplayName();
                    name = System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
                }

                _assemblyNodes.Add(new NodeRef() { name = name, node = node });
            }

            return _assemblyNodes;
        }

        public virtual List<NodeRef> GetNodesByNamespace()
        {
            for (int a = 0; a < _assemblyNodes.Count; a++) { if (_assemblyNodes[a].node == null) { _assemblyNodes.Clear(); break; } }
            if (_assemblyNodes.Count > 0) return _assemblyNodes;

            _assemblyNodes = GatherNodesByNamespace(GetNodesNamespace);
            return _assemblyNodes;
        }


        #region Reflection helper methods

        public struct NodeRef
        {
            public string name;
            public FGraph_NodeBase node;
        }

        public static List<Type> GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try { types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray()); }
                catch (ReflectionTypeLoadException) { }
            }

            return types;
        }

        public virtual string GetPathNameForGenericMenu(Type type, string initialNamespace)
        {
            string name = type.ToString();
            name = name.Replace(initialNamespace + ".", "");
            return name.Replace('.', '/');
        }

        #endregion


    }
}