using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    public class SpawnPointEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void Gizmo(SpawnPoint point, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.8f : 0.4f;

            Gizmos.color = new Color(1, 0.6f, 0, alpha);
            Gizmos.DrawCube(point.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
