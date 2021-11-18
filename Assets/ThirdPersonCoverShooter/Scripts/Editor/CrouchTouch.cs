using UnityEditor;

namespace CoverShooter
{
    [CustomEditor(typeof(CrouchTouch))]
    public class CrouchTouchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var touch = target as CrouchTouch;

            var value = (MobileController)EditorGUILayout.ObjectField("Controller", touch.Controller, typeof(MobileController), true, null);

            if (value != touch.Controller)
            {
                Undo.RecordObjects(targets, "Crouch Touch Controller");
                touch.Controller = value;
            }
        }
    }
}
