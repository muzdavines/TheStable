#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using System.Collections.Generic;
using UnityEngine;


namespace FIMSpace.Generating
{
    public class PipePreset : ScriptableObject
    {
        public PipePresetData Data;

        [System.Serializable]
        public class PipePresetData
        {
            public List<PipeSegmentSetup> Segments = new List<PipeSegmentSetup>();

            public bool AllowUseUnendedOnStartAlign = false;
            public PipeSegmentSetup OptionalUnended;

            public bool AlignOnHitNormal = false;
            public PipeSegmentSetup OptionalEndCap;

            [HideInInspector] public int _editorSelected = 0;

            public PipePresetData Copy()
            {
                PipePresetData data = (PipePresetData)MemberwiseClone();
                data.AllowUseUnendedOnStartAlign = AllowUseUnendedOnStartAlign;
                data.OptionalUnended = OptionalUnended.Copy();

                data.AlignOnHitNormal = AlignOnHitNormal;
                data.OptionalEndCap = OptionalEndCap.Copy();

                data.Segments = new List<PipeSegmentSetup>();

                for (int i = 0; i < Segments.Count; i++)
                {
                    data.Segments.Add(Segments[i].Copy());
                }

                return data;
            }
        }

#if UNITY_EDITOR
        public static void DrawSettings(PipePresetData presetData, SerializedProperty sp, SerializedObject so, Object toDirty)
        {
            //EditorGUILayout.PropertyField(sp, new GUIContent("Pipe Segments Preset"), true );
            //sp.Next(false); EditorGUILayout.PropertyField(sp); sp.Next(false);

            sp.Next(true);
            Color bg = GUI.backgroundColor;
            GUILayout.Space(5);

            if (sp.arraySize == 0)
            {
                if (GUILayout.Button("+ Add new pipe segment +", FGUI_Resources.ButtonStyle)) { presetData.Segments.Add(new PipeSegmentSetup()); EditorUtility.SetDirty(toDirty); }
            }
            else
            {
                GUILayoutOption[] g = new GUILayoutOption[] { GUILayout.Width(32), GUILayout.Height(24) };
                GUILayoutOption[] g2 = new GUILayoutOption[] { GUILayout.Height(24) };
                GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel); // titleStyle.alignment = TextAnchor.MiddleLeft;

                for (int i = 0; i < sp.arraySize; i++)
                {
                    var seg = presetData.Segments[i];

                    EditorGUILayout.BeginVertical(/*FGUI_Resources.BGInBoxStyle*/);

                    GUI.backgroundColor = presetData._editorSelected == i ? Color.green : bg;

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("[" + i + "]", FGUI_Resources.ButtonStyle, g)) { if (presetData._editorSelected == i) presetData._editorSelected = -1; else presetData._editorSelected = i; }

                    string title = "Setup Pipe Segment";

                    if (presetData.Segments.Count == sp.arraySize)
                        if (presetData.Segments[i].Prefab != null)
                            title += " (" + presetData.Segments[i].Prefab.name + ")";

                    if (seg.Enabled == false) title += " (DISABLED)";
                    title += "  | Cost=" + seg.UseCost;
                    title += "  | Joins=" + seg.JoinPoints.Length;

                    GUI.backgroundColor = bg;
                    EditorGUILayout.LabelField(title, titleStyle, g2);
                    if (GUILayout.Button("X", FGUI_Resources.ButtonStyle, GUILayout.Width(32))) { presetData.Segments.RemoveAt(i); EditorUtility.SetDirty(toDirty); break; }
                    if (i == 0) if (GUILayout.Button("Add New Segment", FGUI_Resources.ButtonStyle, GUILayout.Width(122))) { presetData.Segments.Add(new PipeSegmentSetup()); EditorUtility.SetDirty(toDirty); }
                    EditorGUILayout.EndHorizontal();


                    GUILayout.Space(3);
                    if (presetData._editorSelected == i) // Draw selected pipe segment setup
                    {
                        SerializedProperty spi = sp.GetArrayElementAtIndex(i);

                        EditorGUILayout.PropertyField(spi, true);

                        if (seg.PreviewMesh == null)
                        {
                            if (seg.Prefab) seg.Refresh();
                        }

                        //GUILayout.Space(3);
                        //EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space(30);
                        //if (GUILayout.Button("Refresh Auto Params", FGUI_Resources.ButtonStyle)) { seg.Refresh(); }
                        //EditorGUILayout.EndHorizontal();
                        GUILayout.Space(3);
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            GUILayout.Space(4);

            GUI.backgroundColor = presetData._editorSelected == -2 ? Color.green : bg;
            if (GUILayout.Button("[Optional Unended]", FGUI_Resources.ButtonStyle)) { if (presetData._editorSelected == -2) presetData._editorSelected = -1; else presetData._editorSelected = -2; }
            GUI.backgroundColor = bg;

            sp.Next(false);

            if (presetData._editorSelected == -2)
            {
                EditorGUILayout.PropertyField(sp);
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);
                if (GUILayout.Button("Refresh Auto Params", FGUI_Resources.ButtonStyle)) { presetData.OptionalUnended.Refresh(); }
            }
            else
            {
                sp.Next(false);
            }

            GUILayout.Space(3);

            GUI.backgroundColor = presetData._editorSelected == -3 ? Color.green : bg;
            if (GUILayout.Button("[Optional End Cap]", FGUI_Resources.ButtonStyle)) { if (presetData._editorSelected == -3) presetData._editorSelected = -1; else presetData._editorSelected = -3; }
            GUI.backgroundColor = bg;

            sp.Next(false);

            if (presetData._editorSelected == -3)
            {
                EditorGUILayout.PropertyField(sp);
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);
                if (GUILayout.Button("Refresh Auto Params", FGUI_Resources.ButtonStyle)) { presetData.OptionalEndCap.Refresh(); }
            }
            else
            {
                sp.Next(false);
            }

            GUILayout.Space(4);

            if (so != null)
            {
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

#endif
    }
}