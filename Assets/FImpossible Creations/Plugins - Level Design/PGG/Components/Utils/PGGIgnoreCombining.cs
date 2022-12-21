using UnityEngine;

namespace FIMSpace.Generating
{
    public class PGGIgnoreCombining : MonoBehaviour
    {
        
    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGGIgnoreCombining))]
    public class PGGIgnoreCombiningEditor : UnityEditor.Editor
    {
        public PGGIgnoreCombining Get { get { if (_get == null) _get = (PGGIgnoreCombining)target; return _get; } }
        private PGGIgnoreCombining _get;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("This component will just prevent attached MESH RENDERER's mesh from being combined", UnityEditor.MessageType.Info);
        }
    }
#endif

}