using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace CoverShooter
{
    [CustomEditor(typeof(AIActions))]
    public class AIActionsEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
        public static void DrawDebugAINotSelected(Transform transform, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
                return;

            var motor = transform.GetComponent<CharacterMotor>();
            if (motor == null || !motor.IsAlive) return;

            var actions = transform.GetComponent<AIActions>();
            if (actions == null || actions.Active == null) return;

            var position = transform.position + Vector3.up * 2;
            var offset = 8;

            DebugText.Draw(actions.Active.GetType().Name, position, 0, offset);
        }

        private void onActionSelected(object value)
        {
            var actions = (AIActions)target;

            Undo.RecordObject(actions, "Edit actions");

            if (actions.Actions == null)
                actions.Actions = new AIAction[1];
            else
            {
                var new_ = new AIAction[actions.Actions.Length + 1];
                for (int i = 0; i < new_.Length - 1; i++)
                    new_[i] = actions.Actions[i];
                actions.Actions = new_;
            }

            actions.Actions[actions.Actions.Length - 1] = (AIAction)CreateInstance(((Type)value).FullName);

            EditorUtility.SetDirty(actions);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public override void OnInspectorGUI()
        {
            var actions = (AIActions)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Add"))
            {
                var menu = new GenericMenu();

                foreach (Type type in Assembly.GetAssembly(typeof(AIAction)).GetTypes())
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(AIAction)))
                        menu.AddItem(new GUIContent(type.Name), false, onActionSelected, type);

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Default"))
            {
                Undo.RecordObject(actions, "Edit actions");

                actions.Actions = new AIAction[]
                {
                    CreateInstance<AttackAction>(),
                    CreateInstance<HoldPositionAction>()
                };
            }
        }
    }
}
