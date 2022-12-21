using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.FunctionNode;
using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerFunctionNode
    {

        #region Utilities

        public override Color GetNodeColor()
        {
            return NodeColor;
        }

        public override string GetDisplayName(float maxWidth = 120)
        {
            string nme = DisplayName;
            if (string.IsNullOrEmpty(nme)) return name;
            return nme + " (f)";
        }

        #endregion


#if UNITY_EDITOR
        List<SerializedProperty> sp_portsToDisplay = new List<SerializedProperty>();
        GUIContent _measureCont = new GUIContent();
        bool firstDraw = false;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            float height = 70;
            float singleLine = EditorGUIUtility.singleLineHeight;

            if (firstDraw == false)
            {
                RefreshDisplayPortInstances();
                firstDraw = true;
            }

            for (int i = 0; i < sp_portsToDisplay.Count; i++)
            {
                EditorGUILayout.PropertyField(sp_portsToDisplay[i]);
                height += singleLine;
            }

            if (sp_portsToDisplay.Count == 0) height = 54;

            _measureCont.text = DisplayName;
            float measure = EditorStyles.boldLabel.CalcSize(_measureCont).x + 70;

            nodeSize = new Vector2(Mathf.Max(210, measure), height);
        }
#endif

    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PlannerFunctionNode))]
    public class PlannerFunctionNodeEditor : UnityEditor.Editor
    {
        public PlannerFunctionNode Get { get { if (_get == null) _get = (PlannerFunctionNode)target; return _get; } }
        private PlannerFunctionNode _get;

        bool IsPortNode(PGGPlanner_NodeBase node)
        {
            if (node is FN_Input || node is FN_Output || node is FN_Parameter) return true;
            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(4f);

            FGUI_Inspector.FoldHeaderStart(ref portsFoldout, "Ports Order", FGUI_Resources.BGInBoxStyle);

            if (portsFoldout)
            {
                int locI = 0;
                //int realI = 0;
                int firstI = 0;
                int lastI = 0;

                for (int i = 0; i < Get.Procedures.Count; i++)
                {
                    if (Get.Procedures[i] is FN_Input || Get.Procedures[i] is FN_Output || Get.Procedures[i] is FN_Parameter)
                    {
                        if (locI == 0) firstI = i;
                        lastI = i;
                        locI += 1;
                    }
                }

                locI = 0;

                for (int i = 0; i < Get.Procedures.Count; i++)
                {
                    if (Get.Procedures[i] is FN_Input || Get.Procedures[i] is FN_Output || Get.Procedures[i] is FN_Parameter)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(Get.Procedures[i].GetDisplayName(), Get.Procedures[i], typeof(PGGPlanner_NodeBase), true);

                        if (lastI != i)
                        {
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), FGUI_Resources.ButtonStyle, GUILayout.Width(20)))
                            {
                                for (int j = i + 1; j < Get.Procedures.Count; j++)
                                {
                                    var nde = Get.Procedures[j];
                                    if (IsPortNode(nde))
                                    {
                                        Get.Procedures[j] = Get.Procedures[i];
                                        Get.Procedures[i] = nde;
                                        break;
                                    }
                                }
                            }
                        }

                        if (firstI != i)
                        {
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), FGUI_Resources.ButtonStyle, GUILayout.Width(20)))
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    var nde = Get.Procedures[j];
                                    if (IsPortNode(nde))
                                    {
                                        Get.Procedures[j] = Get.Procedures[i];
                                        Get.Procedures[i] = nde;
                                        break;
                                    }
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();

            if (Get.DefaultFunctionsDirectory)
            {
                GUILayout.Space(4);
                if (GUILayout.Button("Move function node to the default directory in project"))
                {
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get), AssetDatabase.GetAssetPath(Get.DefaultFunctionsDirectory) + "/" + Get.name + ".asset");
                    //AssetDatabase.Refresh();
                    //AssetDatabase.SaveAssets();
                    EditorGUIUtility.PingObject(Get);
                }
            }

        }

        bool portsFoldout = false;

    }
#endif



}