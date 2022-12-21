using FIMSpace.FEditor;
using FIMSpace.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(SR_ModGraph))]
    public class SR_ModGraphEditor : UnityEditor.Editor
    {
        public SR_ModGraph Get { get { if (_get == null) _get = (SR_ModGraph)target; return _get; } }
        private SR_ModGraph _get;



        #region Open window on double-click on File

        [OnOpenAssetAttribute(1)]
        public static bool OpenModGraphScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as SR_ModGraph != null)
            {
                SR_ModGraph get = obj as SR_ModGraph;
                ModGraphWindow.Init(get);

                return true;
            }

            return false;
        }
        #endregion



        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("Field Modificator Node Graph File", UnityEditor.MessageType.Info);

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);


            if (GUILayout.Button(new GUIContent("  Graph In Separated Window", PGGUtils.TEX_ModGraphIcon), GUILayout.Height(26)))
            {
                ModGraphWindow.Init(Get);
            }

            GUILayout.Space(5);

            if (Get.ExternalModGraph)
            {
                if (GUILayout.Button(new GUIContent("  Open External Graph", PGGUtils.TEX_ModGraphIcon), GUILayout.Height(26)))
                {
                    ModGraphWindow.Init(Get.ExternalModGraph);
                }
            }

            GUILayout.Space(5);

            if (AssetDatabase.IsSubAsset(Get) == false)
                if (GUILayout.Button(new GUIContent("Switch Internal Nodes Visiblity", FGUI_Resources.Tex_AB), GUILayout.Height(22)))
                {
                    var nodes = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Get));

                    if (nodes.Length > 0)
                    {
                        var baseFlag = nodes[0].hideFlags;
                        if (baseFlag == HideFlags.HideInHierarchy) baseFlag = HideFlags.None; else baseFlag = HideFlags.HideInHierarchy;

                        for (int n = 0; n < nodes.Length; n++)
                        {
                            nodes[n].hideFlags = baseFlag;
                        }

                        AssetDatabase.SaveAssets();
                    }
                }

        }
    }
}