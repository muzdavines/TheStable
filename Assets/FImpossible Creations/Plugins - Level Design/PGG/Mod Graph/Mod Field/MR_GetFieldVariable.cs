using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetFieldVariable : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Field Variable" : "Get Field Variable"; }
        public override string GetNodeTooltipDescription { get { return "Get Field Setup variable value or get Mod Pack variable value."; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 240 : 200, _EditorFoldout ? 121 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [HideInInspector] public int VariableIdx = 0;

        [Port(EPortPinType.Output)] public PGGUniversalPort Out;

        public enum EVariablesSource { ParentFieldSetup, ParentModPack }
        [HideInInspector] public EVariablesSource VariablesSource = EVariablesSource.ParentFieldSetup;

        public override void OnStartReadingNode()
        {
            var getVar = MGGetVariable(GetTarget(false), VariableIdx);
            if (getVar == null) return;
            Out.Variable.SetValue(getVar);
        }

        public UnityEngine.Object GetTarget(bool editor = true)
        {
            if (VariablesSource == EVariablesSource.ParentFieldSetup)
            {
                if (!editor)
                {
                    return MG_Preset;
                }

                return MGGetFieldSetup();
            }
            else
            if (VariablesSource == EVariablesSource.ParentModPack)
                return MGGetParentPack();

            return null;
        }


        #region Compact Editor node display on graph code

#if UNITY_EDITOR

        SerializedProperty sp = null;

        string GetVarName(int index)
        {
            var v = MGGetVariable(GetTarget(), index);
            if (v == null) return "Not Selected";
            return v.Name;
        }


        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if (GUILayout.Button(GetVarName(VariableIdx), EditorStyles.popup))
            {

                var vars = MGGetVariables(GetTarget());
                if (vars != null)
                {
                    GenericMenu menu = new GenericMenu();

                    for (int v = 0; v < vars.Count; v++)
                    {
                        int newIndex = v;
                        menu.AddItem(new GUIContent(vars[v].Name), VariableIdx == v, () => { VariableIdx = newIndex; });
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    UnityEngine.Debug.Log("[PGG Mod Graph] Can't find variables list!");
                }

            }

            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("VariablesSource");

                SerializedProperty spc = sp.Copy();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(spc, GUIContent.none, GUILayout.MinWidth(124)); spc.Next(false);
                GUI.enabled = false;
                EditorGUILayout.ObjectField(GetTarget(), typeof(UnityEngine.Object), true);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

        #endregion

    }
}