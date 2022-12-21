using UnityEngine;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning.GeneratingLogics;
using System;
using System.Collections.Generic;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning
{
    public partial class FieldPlanner
    {
        [HideInInspector] public bool _EditorDrawShape = true;
        [HideInInspector] public bool _EditorDrawParameters = false;
        [HideInInspector] public bool _EditorDrawLogics = true;
        [HideInInspector] public bool _EditorDrawPostLogics = false;
        [HideInInspector] public int _EditorSelectedShape = 0;
        [HideInInspector] public int _EditorSelectedVar;
        [HideInInspector]/*[NonSerialized]*/ public bool _EditorDisplayGizmosOnPlan = true;

        public List<FieldPlanner> PlannersInBuild { get { return ParentBuildPlanner.BasePlanners; } }

        /// <summary>
        /// Triggering initial checker to prepare, basing on current generating seed
        /// </summary>
        public void PrepareInitialChecker()
        {
            if (_tempOverrideShape != null)
            {
                previewChecker = _tempOverrideShape.GetChecker(this);
            }
            else
            {
                if (ShapeGenerator == null) previewChecker = new CheckerField3D();
                else previewChecker = ShapeGenerator.GetChecker(this);
            }

            //if (ParentBuildPlanner.UseBoundsWorkflow)
            //{
            //    previewChecker.RecalculateMultiBounds();
            //}

            //previewChecker.UseBounds = ParentBuildPlanner.UseBoundsWorkflow;
        }

        public CheckerField3D GetInitialChecker()
        {
            if (previewChecker == null || previewChecker.ChildPositionsCount == 0) PrepareInitialChecker();

            previewChecker.RootScale = GetScale;

            return previewChecker;
        }



#if UNITY_EDITOR

        static Color _mildDisableCol = new Color(1f, 1f, .55f, 1f);
        public void _EditorGUI_DrawVisibilitySwitchButton()
        {
            Color preBg = GUI.backgroundColor;

            if (_EditorDisplayGizmosOnPlan == false) GUI.backgroundColor = _mildDisableCol;

            if (GUILayout.Button(EditorGUIUtility.IconContent(_EditorDisplayGizmosOnPlan ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff"), FEditor.FGUI_Resources.ButtonStyle, GUILayout.Height(22), GUILayout.Width(26)))
            {
                _EditorDisplayGizmosOnPlan = !_EditorDisplayGizmosOnPlan;
                SceneView.RepaintAll();
            }

            if (GUI.backgroundColor == _mildDisableCol) GUI.backgroundColor = preBg;
        }

        public void SetShapeGenerator(ShapeGeneratorBase rule)
        {
            if (ShapeGenerator != null)
            {
                if (rule != ShapeGenerator)
                {
                    DestroyImmediate(ShapeGenerator, true);
                    ShapeGenerator = null;
                }
            }

            rule.hideFlags = HideFlags.HideInHierarchy;
            ShapeGenerator = rule;
            FGenerators.AddScriptableTo(rule, ParentBuildPlanner, false, false);
            EditorUtility.SetDirty(rule);
            EditorUtility.SetDirty(ParentBuildPlanner);
            //AssetDatabase.SaveAssets();
        }

        public void AddRuleToPlanner(PlannerRuleBase rule, bool postProcedure = false)
        {
            rule.ParentPlanner = this;
            rule.hideFlags = HideFlags.HideInHierarchy;

            if (postProcedure) FPostProcedures.Add(rule);
            else FProcedures.Add(rule);

            FGenerators.AddScriptableTo(rule, ParentBuildPlanner, false, false);
            EditorUtility.SetDirty(rule);
            EditorUtility.SetDirty(this);
            //AssetDatabase.SaveAssets();
        }

        public void AddPostProcedureToPlanner(PlannerRuleBase rule)
        {
            rule.ParentPlanner = this;
            rule.hideFlags = HideFlags.HideInHierarchy;
            FPostProcedures.Add(rule);
            FGenerators.AddScriptableTo(rule, ParentBuildPlanner, false, true);
            EditorUtility.SetDirty(rule);
            AssetDatabase.SaveAssets();
        }

        internal void RemoveRuleFromPlanner(PlannerRuleBase plannerRuleBase)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.Log("[PGG] Not allowed removing rules in playmode");
                return;
            }

            FProcedures.Remove(plannerRuleBase);
            DestroyImmediate(plannerRuleBase, true);
            EditorUtility.SetDirty(ParentBuildPlanner);
            AssetDatabase.SaveAssets();
        }

        //public void DrawLogicsGUI(SerializedObject so, bool postProced, string property)
        //{
        //    GUILayout.Space(5);

        //    //if ( postProced)
        //    //{
        //    //    EditorGUILayout.HelpBox("Post Procedures are executed after all Planners and Layers Procedures. It can be used to create relations between fields to for example prevent creating windows in counter fields / fill empty spaces.", MessageType.None);
        //    //}

        //    if (FProcedures.Count == 0)
        //    {
        //        GUILayout.Space(7);
        //        EditorGUILayout.LabelField("No Procedures Yet", EditorStyles.centeredGreyMiniLabel);
        //        GUILayout.Space(9);
        //    }
        //    else
        //    {
        //        SerializedProperty sorls = so.FindProperty(property);

        //        List<PlannerRuleBase> rules = FProcedures;
        //        if (postProced) rules = FPostProcedures;

        //        PGGUtils.CheckForNulls(rules);
        //        for (int i = 0; i < rules.Count; i++)
        //        {
        //            rules[i].DrawGUIStack(i);
        //            GUILayout.Space(8);
        //        }
        //    }

        //    DrawLogicAddButtons(postProced);

        //}

        public void AddContextMenuItems(GenericMenu menu, bool postProced, FieldPlanner planner)
        {
            List<Type> types = FieldModification.GetDerivedTypes(typeof(PlannerRuleBase));
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                if (type == typeof(PlannerRuleBase)) continue;
                string path = GetMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                string name = path;
                PlannerRuleBase rule = CreateInstance(type) as PlannerRuleBase;
                if (rule != null) name = path.Replace(rule.GetType().Name, "") + rule.GetDisplayName();

                if (!postProced)
                {
                    menu.AddItem(new GUIContent(name), false, () =>
                    {
                        planner.AddRuleToPlanner(rule);
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent(name), false, () =>
                    {
                        planner.AddPostProcedureToPlanner(rule);
                    });
                }
            }
        }

        private static string GetMenuName(Type type)
        {
            string name = type.ToString();
            name = name.Replace("FIMSpace.Generating.Planning.ShapeLogics.", "");
            return name.Replace('.', '/');
        }

        public void DrawLogicAddButtons(bool postProced)
        {

            //#if UNITY_2019_4_OR_NEWER
            EditorGUILayout.BeginHorizontal();

            var content = new GUIContent(" + Add Procedure + ");
            var rect = GUILayoutUtility.GetRect(content, EditorStyles.miniButton);

            if (GUI.Button(rect, content))
            //if (GUILayout.Button("+ Add Rule +"))
            {
                GenericMenu menu = new GenericMenu();
                AddContextMenuItems(menu, postProced, this);

                menu.AddItem(new GUIContent("+ Create Custom Procedure Script"), false, () =>
                {

                });

                menu.DropDown(rect);
                //menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
            }
            GUILayout.Space(10);


            EditorGUILayout.EndHorizontal();
            //#else
            //            if (GUILayout.Button("+ Add Rule +"))
            //            {
            //                GenericMenu menu = new GenericMenu();
            //                Parent.AddContextMenuItems(menu, this);
            //                menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
            //            }
            //#endif
        }

        public void OnNodeRemove(FGraph_NodeBase node)
        {
            DestroyImmediate(node, true);
            EditorUtility.SetDirty(this);
            if (ParentBuildPlanner) EditorUtility.SetDirty(ParentBuildPlanner);
        }
#endif
    }

}