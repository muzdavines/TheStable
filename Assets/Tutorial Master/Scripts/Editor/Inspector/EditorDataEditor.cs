using UnityEditor;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <inheritdoc />
    /// <summary>
    /// Renders the Editor for EditorData ScriptableObject
    /// </summary>
    /// <seealso cref="T:UnityEditor.Editor" />
    [CustomEditor(typeof(EditorData))]
    public sealed class EditorDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Tutorial Master Editor Data", EditorStyles.largeLabel);
        }
    }
}