using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PipeGenerator))]
    public class TestPipeEditor : UnityEditor.Editor
    {
        public PipeGenerator Get { get { if (_get == null) _get = (PipeGenerator)target; return _get; } }
        private PipeGenerator _get;

        string[] exclude = new string[] { "m_Script", "componentPreset" };

        public override void OnInspectorGUI()
        {
            Color bg = GUI.backgroundColor;

            if (Get._EditorCategory == PipeGenerator.EEditorState.Setup) EditorGUILayout.HelpBox("Future versions of PipeGenerator will bring support for multi-way pipes generation", MessageType.None);

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (Get._EditorCategory == PipeGenerator.EEditorState.Setup) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent(" Setup", FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Height(26))) { Get._EditorCategory = PipeGenerator.EEditorState.Setup; }
            GUI.backgroundColor = bg;

            if (Get._EditorCategory == PipeGenerator.EEditorState.Adjust) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent(" Generating", FGUI_Resources.Tex_Sliders), FGUI_Resources.ButtonStyle, GUILayout.Height(26))) { Get._EditorCategory = PipeGenerator.EEditorState.Adjust; }
            GUI.backgroundColor = bg;

            if (Get._EditorCategory == PipeGenerator.EEditorState.Extra) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent(" Extra", FGUI_Resources.Tex_Tweaks), FGUI_Resources.ButtonStyle, GUILayout.Height(26))) { Get._EditorCategory = PipeGenerator.EEditorState.Extra; }
            GUI.backgroundColor = bg;

            if (Get._EditorCategory == PipeGenerator.EEditorState.All) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent( FGUI_Resources.Tex_Default), FGUI_Resources.ButtonStyle, GUILayout.Height(26))) { Get._EditorCategory = PipeGenerator.EEditorState.All; }
            GUI.backgroundColor = bg;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxLightStyle);
            //if (GUILayout.Button(" Pipe Assets Preparation ", FGUI_Resources.HeaderStyle)) Get._EditorDrawPreparation = !Get._EditorDrawPreparation;
            //EditorGUILayout.EndHorizontal();

            if (Get._EditorCategory == PipeGenerator.EEditorState.Setup)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectPreset"), new GUIContent("Preset (Optional)"));
                if (GUILayout.Button("Create New", GUILayout.Width(94))) { Get._EditorSetPreset((PipePreset)FGenerators.GenerateScriptable(CreateInstance<PipePreset>(), "PP_Pipe-")); }
                if (GUILayout.Button("Export", GUILayout.Width(94))) 
                {
                    PipePreset exp = CreateInstance<PipePreset>();
                    exp.Data = Get.PresetData.Copy();
                    Get._EditorSetPreset((PipePreset)FGenerators.GenerateScriptable(exp, "PP_Pipe-")); 
                }

                EditorGUILayout.EndHorizontal();

                SerializedProperty sp_pData;
                SerializedObject so_pData = null;

                if (Get._EditorGetProjectPreset() == null)
                {
                    sp_pData = serializedObject.FindProperty("componentPreset");
                }
                else
                {
                    so_pData = new SerializedObject(Get._EditorGetProjectPreset());
                    sp_pData = so_pData.FindProperty("Data");
                }

                EditorGUI.indentLevel++;
                Object toDirty; if (so_pData == null) toDirty = Get; else toDirty = Get._EditorGetProjectPreset();
                PipePreset.DrawSettings(Get.PresetData, sp_pData, so_pData, toDirty);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }


            if (Get._EditorCategory == PipeGenerator.EEditorState.Adjust)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                GUILayout.Space(3f);

                SerializedProperty sp = serializedObject.FindProperty("GenerateOnStart");
                EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);

                EditorGUILayout.EndVertical();
            }


            if (Get._EditorCategory == PipeGenerator.EEditorState.Extra)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                GUILayout.Space(3f);

                SerializedProperty sp = serializedObject.FindProperty("HoldMask");
                EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);
                sp.NextVisible(false); EditorGUILayout.PropertyField(sp, true);

                EditorGUILayout.EndVertical();
            }


            if (Get._EditorCategory == PipeGenerator.EEditorState.All)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                GUILayout.Space(3f);

                DrawPropertiesExcluding(serializedObject, exclude);

                EditorGUILayout.EndVertical();
            }


            GUILayout.Space(3f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview"))
            {
                Get.PreviewGenerate();
            }

            if (GUILayout.Button("Generate")) { Get.GenerateObjects(); }
            if (Get.AreGeneratedObjects) if (GUILayout.Button("Remove Generated")) { Get.ClearGenerated(); }
            EditorGUILayout.EndHorizontal();
            Get._EditorConstantPreview = EditorGUILayout.Toggle("Constant preview", Get._EditorConstantPreview);

            serializedObject.ApplyModifiedProperties();

            if ( EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }


        private void OnSceneGUI()
        {
            Color preH = Handles.color;
            Handles.matrix = Get.transform.localToWorldMatrix;

            if (Get._EditorCategory != PipeGenerator.EEditorState.Setup) // Drawing generation preview
            {

            }
            else // Pipe segments setup
            {
                if (Get.PresetData._editorSelected > Get.PresetData.Segments.Count - 1) Get.PresetData._editorSelected = -1;

                if (Get.PresetData._editorSelected != -1)
                {
                    PipeSegmentSetup s;
                    if (Get.PresetData._editorSelected >= 0) s = Get.PresetData.Segments[Get.PresetData._editorSelected];
                    else s = Get.PresetData._editorSelected == -2 ? Get.PresetData.OptionalUnended : Get.PresetData.OptionalEndCap;

                    if (s != null)
                    {
                        EditorGUI.BeginChangeCheck();

                        for (int i = 0; i < s.JoinPoints.Length; i++)
                        {
                            var j = s.JoinPoints[i];

                            if (j.outAxis.sqrMagnitude != 0f)
                            {
                                //if (j.forward.sqrMagnitude != 0f && j.up.sqrMagnitude != 0f)
                                Quaternion r = Quaternion.LookRotation(j.outAxis, s.ModelUpAxis);
                                j.origin = FEditor_TransformHandles.PositionHandle(j.origin, r, .35f, true, false);
                                //j.origin = FEditor_TransformHandles.PositionHandle(j.origin, Quaternion.LookRotation(j.forward, j.up), .25f, false, false);
                                FGUI_Handles.DrawArrow(j.origin, r, s.ReferenceScale * 0.15f, 0f);

                                Vector3 cross = Vector3.Cross(j.outAxis, s.ModelUpAxis).normalized;

                                Handles.color = new Color(1f, 1f, 1f, 0.5f);
                                Handles.Label(j.origin + cross * s.ReferenceScale * 0.05f, new GUIContent("J[" + i.ToString() + "]"));
                                Handles.DrawDottedLine(j.origin + cross * 0.25f, j.origin - cross * 0.25f, 1f);
                                Handles.DrawDottedLine(j.origin + s.ModelUpAxis * 0.25f, j.origin - s.ModelUpAxis * 0.25f, 1f);
                                Handles.color = preH;
                            }
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "PipeGen Joints");
                        }
                    }
                }
            }

            Handles.matrix = Matrix4x4.identity;
            Handles.color = preH;

        }
    }

    static class PipeMenuItems
    {
        [MenuItem("GameObject/Generators/Pipe Generator", false, 1001)]
        static void CreatePipeeneratorObject()
        {
            GameObject go = new GameObject();
            go.name = "Pipe Generator";
            go.AddComponent<PipeGenerator>();

            PositionNewObject(go);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        static void PositionNewObject(GameObject go)
        {
            var sceneCamera = SceneView.lastActiveSceneView.camera;

            if (sceneCamera != null)
                go.transform.position = sceneCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            else
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        }
    }
}
