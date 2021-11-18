using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(LayerManager))]
    public class LayerManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var manager = (LayerManager)target;
            Undo.RecordObject(manager, "Layers");

            edit("Geometry:", ref manager.Geometry);
            edit("Cover:", ref manager.Cover);
            edit("Scope:", ref manager.Scope);
            edit("Character: ", ref manager.Character);
            edit("Zones: ", ref manager.Zones);

            manager.SetValues();
        }

        private void edit(string name, ref int value)
        {
            EditorGUILayout.LabelField(name);
            EditorGUI.indentLevel++;

            for (int i = 0; i < 32; i++)
            {
                int bit = 1 << i;
                bool on = (value & bit) == bit;

                var layerName = LayerMask.LayerToName(i);

                if (layerName == "")
                {
                    if (i < 8)
                        layerName = "Builtin Layer " + i.ToString();
                    else
                        layerName = "User Layer " + i.ToString();
                }

                on = EditorGUILayout.ToggleLeft(layerName, on);

                if (on)
                    value |= bit;
                else
                    value &= ~bit;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();
        }
    }
}
