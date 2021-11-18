using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(AIRegrouperButton))]
    [CanEditMultipleObjects]
    public class AIRegrouperButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Regroup"))
                foreach (var t in targets)
                    ((AIRegrouperButton)t).Regroup();
        }
    }
}
