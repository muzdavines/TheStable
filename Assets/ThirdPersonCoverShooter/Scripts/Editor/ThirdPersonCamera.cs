using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(ThirdPersonCamera))]
    [CanEditMultipleObjects]
    public class ThirdPersonCameraEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
        public static void DrawDebugThirdPersonCamera(Transform transform, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
                return;

            var camera = transform.GetComponent<ThirdPersonCamera>();
            if (camera == null) return;

            var position = transform.position;
            var offset = 8;

            DebugText.Draw(camera.State, position, 0, offset);
        }
    }
}
