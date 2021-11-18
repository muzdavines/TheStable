using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    [CustomEditor(typeof(EquipAnimation))]
    public class EquipEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var equip = (EquipAnimation)target;
            Undo.RecordObject(equip, "Min Max Slider");

            EditorGUILayout.LabelField("Grab: ", equip.Grab.ToString());
            EditorGUILayout.LabelField("End: ", equip.End.ToString());
            EditorGUILayout.MinMaxSlider(ref equip.Grab, ref equip.End, 0, 1);
        }
    }
}
