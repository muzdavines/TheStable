
namespace FIMSpace.Graph
{
    public static class NodeExtensionMethods
    {
        public static FGraph_NodeBase_Drawer Drawer(this FGraph_NodeBase node, FGraphDrawerBase graph)
        {
            if (node == null)
            {
                UnityEngine.Debug.Log("No Node Drawer, you probably need to implement one!");
                return null;
            }

            if (node._editorDrawer == null)
            {
                node._editorDrawer = graph.GetNodeDrawer(node);
            }

            return node._editorDrawer as FGraph_NodeBase_Drawer;
        }
    }
}