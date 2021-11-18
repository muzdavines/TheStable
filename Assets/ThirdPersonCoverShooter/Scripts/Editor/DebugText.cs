using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    public static class DebugText
    {
        public static void Draw(string text, Vector3 position, float offsetx = 0, float offsety = 0)
        {
            Draw(text, position, offsetx, offsety, Color.white);
        }

        public static void Draw(string text, Vector3 position, float offsetx, float offsety, Color color)
        {
            Handles.BeginGUI();

            var borderCheck = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(position);
            if (borderCheck.y < 0 || borderCheck.y > Screen.height || borderCheck.x < 0 || borderCheck.x > Screen.width || borderCheck.z < 0)
            {
                Handles.EndGUI();
                return;
            }

            var previous = GUI.color;

            GUI.color = Color.black;
            Handles.Label(transform(position, offsetx + 1, offsety + 16 - 1), text);

            GUI.color = color;
            Handles.Label(transform(position, offsetx, offsety + 16), text);

            GUI.color = previous;

            Handles.EndGUI();
        }

        private static Vector3 transform(Vector3 position, float x, float y)
        {
            return transform(position, new Vector3(x, y));
        }

        private static Vector3 transform(Vector3 position, Vector3 translation)
        {
            var camera = UnityEditor.SceneView.currentDrawingSceneView.camera;

            if (camera)
                return camera.ScreenToWorldPoint(camera.WorldToScreenPoint(position) + translation);
            else
                return position;
        }
    }
}
