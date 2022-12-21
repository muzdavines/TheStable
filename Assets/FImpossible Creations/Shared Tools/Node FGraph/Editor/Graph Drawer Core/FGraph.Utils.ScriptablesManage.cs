using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {

        public void AddNodeToFile(FGraph_NodeBase node, ScriptableObject container)
        {
            if (container == null) return;
            if (node == null) return;

            node.hideFlags = HideFlags.HideInHierarchy;
            node.name = node.GetDisplayName();
            FGenerators.AddScriptableTo(node, container, false, false);
            EditorUtility.SetDirty(container);

        }


        public void ClearUnusedNodesFromFile(ScriptableObject container)
        {

            if (AssetDatabase.Contains(container) == false) return;

            string path = AssetDatabase.GetAssetPath(container);
            if (string.IsNullOrEmpty(path)) return;

            List<UnityEngine.Object> toRemove = new List<UnityEngine.Object>();

            for (int i = 0; i < nodesToDraw.Count; i++)
            {
                FGraph_NodeBase nodeBase = nodesToDraw[i];
                if (nodeBase == null) continue;
                if (nodeBase.GetType().IsSubclassOf(GetBaseNodeType) == false) continue;
                toRemove.Add(nodeBase);
            }

            if (toRemove.Count == 0) return;

            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            for (int i = 0; i < assets.Length; i++)
            {
                if (toRemove.Contains(assets[i]))
                {
                    GameObject.DestroyImmediate(assets[i], true);
                }
            }

            EditorUtility.SetDirty(container);

        }

        public static void RemoveNodeFromFile(FGraph_NodeBase node, ScriptableObject container)
        {

            if (AssetDatabase.Contains(container) == false) return;

            string path = AssetDatabase.GetAssetPath(container);
            if (string.IsNullOrEmpty(path)) return;

            GameObject.DestroyImmediate(node, true);

            EditorUtility.SetDirty(container);

        }

    }
}