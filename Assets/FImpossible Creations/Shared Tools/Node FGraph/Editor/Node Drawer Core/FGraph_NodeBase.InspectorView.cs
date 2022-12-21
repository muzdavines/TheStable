using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FGraph_NodeBase), true)]
    public class FGraph_NodeBaseEditor : UnityEditor.Editor
    {
        public FGraph_NodeBase Get { get { if (_get == null) _get = (FGraph_NodeBase)target; return _get; } }
        private FGraph_NodeBase _get;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(4f);
            Get.Editor_OnAdditionalInspectorGUI();
        }
    }
}