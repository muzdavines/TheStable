using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FReflectionProbePreset))]
    public class FReflectionProbePresetEditor : UnityEditor.Editor
    {
        public FReflectionProbePreset Get { get { if (_get == null) _get = (FReflectionProbePreset)target; return _get; } }
        private FReflectionProbePreset _get;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Unity's built in components presets can't be used on build so we must use custom preset class", MessageType.Info);

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/ReflectionProbe/Export Runtime Preset File")]
        private static void ReflectionProbeFit(MenuCommand menuCommand)
        {
            ReflectionProbe targetComponent = (ReflectionProbe)menuCommand.context;

            if (targetComponent)
            {
                FReflectionProbePreset preset = CreateInstance<FReflectionProbePreset>();
                preset.CopySettingsFrom(targetComponent);
                FGenerators.GenerateScriptable(preset, "ReflectionProbe_", "lastProbe");
            }
        }
#endif

    }

}