using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(FighterBrain))]
    [CanEditMultipleObjects]
    public class FighterBrainEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
        public static void DrawDebugAINotSelected(Transform transform, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
                return;

            var motor = transform.GetComponent<CharacterMotor>();
            if (motor == null || !motor.IsAlive) return;

            var brain = transform.GetComponent<FighterBrain>();
            if (brain == null) return;

            var position = transform.position + Vector3.up * 2;
            var offset = -8;

            DebugText.Draw(brain.State.ToString(), position, 0, offset);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Add default components"))
                for (int i = 0; i < targets.Length; i++)
                {
                    var obj = ((FighterBrain)targets[i]).gameObject;
                    Undo.RecordObject(obj, "Add default components");

                    tryAdding<AIAim>(obj);
                    tryAdding<AIListener>(obj);
                    tryAdding<AICommunication>(obj);
                    tryAdding<AICover>(obj);
                    tryAdding<AIFire>(obj);
                    tryAdding<AIInvestigation>(obj);
                    tryAdding<AIMovement>(obj);
                    tryAdding<AISearch>(obj);
                    tryAdding<AISight>(obj);
                }
        }

        private void tryAdding<T>(GameObject obj) where T : MonoBehaviour
        {
            if (obj.GetComponent<T>() == null)
                obj.AddComponent<T>();
        }
    }
}
