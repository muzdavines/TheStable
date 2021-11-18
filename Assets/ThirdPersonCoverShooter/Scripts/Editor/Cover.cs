using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    [CustomEditor(typeof(Cover))]
    [CanEditMultipleObjects]
    public class CoverEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void Gizmo(Cover cover, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.9f : 0.5f;

            var collider = cover.GetComponent<BoxCollider>();
            if (collider != null)
            {
                Gizmos.color = new Color(204 / 255f, 147 / 255f, 89f / 255f, alpha);
                Gizmos.matrix = cover.transform.localToWorldMatrix;
                Gizmos.DrawCube(collider.center, collider.size);

                const float depth = 0.2f;
                const float height = 0.1f;

                var coverDepth = Vector3.Distance(cover.transform.TransformPoint(0, 0, 0), cover.transform.TransformPoint(0, 0, collider.size.z));
                var coverHeight = Vector3.Distance(cover.transform.TransformPoint(0, 0, 0), cover.transform.TransformPoint(0, collider.size.y, 0));

                Gizmos.matrix = Matrix4x4.Translate(new Vector3(0, -coverHeight * 0.5f + height * 0.5f, 0) - cover.Forward * (coverDepth * 0.5f + depth * 0.5f)) * 
                                cover.transform.localToWorldMatrix * 
                                Matrix4x4.Scale(new Vector3(1, height / coverHeight, depth / coverDepth));
                Gizmos.DrawCube(collider.center, collider.size);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Auto set openings"))
            {
                Undo.RecordObjects(targets, "Auto set openings");

                for (int i = 0; i < targets.Length; i++)
                {
                    var cover = (Cover)targets[i];
                    var height = cover.IsTall ? 1.2f : 0.8f;

                    cover.OpenLeft = !checkLine(cover.LeftCorner(height, 0), cover.LeftCorner(height, 0.5f)) && !checkRay(cover.LeftCorner(height, 0.5f), cover.Forward, 2);
                    cover.OpenRight = !checkLine(cover.RightCorner(height, 0), cover.RightCorner(height, 0.5f)) && !checkRay(cover.RightCorner(height, 0.5f), cover.Forward, 2);
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Select all covers"))
            {
                List<GameObject> list = new List<GameObject>();

                foreach (var cover in GameObject.FindObjectsOfType<Cover>())
                    list.Add(cover.gameObject);

                Selection.objects = list.ToArray();
            }
        }

        private bool checkRay(Vector3 position, Vector3 direction, float distance)
        {
            return checkLine(position, position + direction * distance);
        }

        private bool checkLine(Vector3 position, Vector3 end)
        {
            if (Physics.Raycast(position, (end - position).normalized, Vector3.Distance(end, position)))
            {
                Debug.DrawLine(position, end, Color.red, 3);
                return true;
            }

            Debug.DrawLine(position, end, Color.green, 3);
            return false;
        }
    }
}
