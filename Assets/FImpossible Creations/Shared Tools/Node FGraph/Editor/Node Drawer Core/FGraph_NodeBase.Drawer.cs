#if UNITY_EDITOR

// If UNITY_EDITOR for being able to define drawer in the main assembly,
// if there are just few gui elements to change, it's just more comfortable
// when creating new nodes and managing smaller ones

namespace FIMSpace.Graph
{
    public partial class FGraph_NodeBase_Drawer
    {
        public FGraph_NodeBase baseGet { get { return _bsget; } }
        private FGraph_NodeBase _bsget;

        public FGraph_NodeBase_Drawer(FGraph_NodeBase owner)
        {
            _bsget = owner;
            owner.OnCreatedDrawer();
        }
    }
}
#endif
