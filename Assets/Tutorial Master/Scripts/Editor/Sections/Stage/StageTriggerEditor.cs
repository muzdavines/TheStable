using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StageTriggerEditor : Section
    {
        public StageTriggerEditor(ref TutorialMasterEditor editor)
            : base(
                ref editor,
                "Trigger",
                "Triggers are means of progressing to another part of the Tutorial. E.g. clicking a button.")
        {
        }

        protected override void OnSectionGUI()
        {
            int selectedStageIndex = Mathf.Clamp(
                Editor.EditorData.SelectedStageIndex,
                0,
                Editor.SelectedTutorial.Stages.Count - 1);
            string basePropertyPath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Trigger",
                Editor.EditorData.SelectedTutorialIndex,
                selectedStageIndex);

            StageTrigger stageTrigger = Editor.SelectedStage.Trigger;
            SerializedProperty property = Editor.serializedObject.FindProperty(basePropertyPath + ".Type");

            if (property != null)
            {
                EditorField.Field(property, new GUIContent("Trigger Type"));

                switch (stageTrigger.Type)
                {
                    case TriggerType.KeyPress:

                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".TriggerKey"),
                            new GUIContent("Key to Press"));

                        break;

                    case TriggerType.Input:

                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".TriggerInput"),
                            new GUIContent("Input Name"));
                        break;

                    case TriggerType.UGUIButtonClick:

                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".UIButtonTarget"),
                            new GUIContent("UGUI Element"));

                        break;

                    case TriggerType.UnityEventInvoke:

                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".EventTarget"),
                            new GUIContent("Target"));

                        break;

                    case TriggerType.Timer:

                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".TriggerTimerAmount"),
                            new GUIContent("Timer Amount"));

                        break;
                }

                if (stageTrigger.Type != TriggerType.None)
                {
                    if (stageTrigger.Type != TriggerType.Timer)
                    {
                        EditorField.Field(
                            Editor.serializedObject.FindProperty(basePropertyPath + ".TriggerActivationDelay"),
                            new GUIContent("Activation Delay"));

                        EditorGUILayout.Space();

                        EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnDelayEnd"));
                    }

                    EditorGUILayout.LabelField("Trigger Activation", EditorStyles.boldLabel);

                    EditorGUILayout.Space();

                    EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnTriggerActivate"));
                    EditorField.Field(
                        Editor.serializedObject.FindProperty(basePropertyPath + ".TriggerActivationEvent"),
                        new GUIContent("Additional Action"));
                }
            }
        }
    }
}