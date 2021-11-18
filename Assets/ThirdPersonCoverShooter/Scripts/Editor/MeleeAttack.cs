using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    [CustomEditor(typeof(MeleeAnimation))]
    public class MeleeAttackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var attack = (MeleeAnimation)target;
            Undo.RecordObject(attack, "Min Max Slider");

            attack.Limb = (Limb)EditorGUILayout.EnumPopup("Limb: ", attack.Limb);

            attack.Moment = EditorGUILayout.Slider("Moment: ", attack.Moment, 0, 1);

            if (attack.Moment > attack.End)
                attack.Moment = attack.End;

            EditorGUILayout.LabelField("Scan Start: ", attack.ScanStart.ToString());
            EditorGUILayout.LabelField("Scan End: ", attack.ScanEnd.ToString());
            EditorGUILayout.MinMaxSlider(ref attack.ScanStart, ref attack.ScanEnd, 0, 1);
            attack.nextComboMove = EditorGUILayout.TextField("Next Combo Move Name:", attack.nextComboMove);
            //attack.thisMove = EditorGUILayout.TextField("This Move Name:", attack.thisMove);
            if (attack.EnableCombo)
            {
                EditorGUILayout.LabelField("Combo Check: ", attack.ComboCheck.ToString());
                EditorGUILayout.LabelField("End: ", attack.End.ToString());
                EditorGUILayout.MinMaxSlider(ref attack.ComboCheck, ref attack.End, 0, 1);
            }
            else
                attack.End = EditorGUILayout.Slider("End: ", attack.End, 0, 1);

            attack.EnableCombo = EditorGUILayout.Toggle("Enable Combo: ", attack.EnableCombo);

            if (attack.End < attack.ScanEnd)
                attack.End = attack.ScanEnd;
        }
    }
}
