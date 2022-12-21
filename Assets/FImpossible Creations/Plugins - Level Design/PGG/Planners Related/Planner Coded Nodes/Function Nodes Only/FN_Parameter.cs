using FIMSpace.Graph;
using UnityEngine;
using System;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.FunctionNode
{

    public class FN_Parameter : PlannerRuleBase
    {
        [HideInInspector] public string ParameterName = "Parameter";
        public override string GetDisplayName(float maxWidth = 120) { return ParameterName; }
        public override string GetNodeTooltipDescription { get { return "Defining parameter field for other nodes which will use this function node.\nCan be ordered through inspector window if you select this function node file"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Externals; } }
        public override EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.JustFunctions; } }

        public override Vector2 NodeSize { get { return new Vector2(Mathf.Max(160, ParameterName.Length * 12), 84); } }
        public override Color GetNodeColor() { return new Color(.4f, .4f, .4f, .95f); }
        //public override Color _E_GetColor() { return new Color(.8f, .55f, .3f, .95f); }

        public override bool DrawInspector { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public EType InputType = EType.Number;

        [HideInInspector] [Port(EPortPinType.Output, true)] public IntPort IntInput;
        [HideInInspector] [Port(EPortPinType.Output, true)] public BoolPort BoolInput;
        [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort FloatInput;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Vector3Input;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGStringPort StringInput;


        public void SetValue(object o)
        {
            if (o == null) { /*UnityEngine.Debug.Log("Null port value!");*/ return; }

            switch (InputType)
            {
                case EType.Int:
                    if (o is int || o is float || o is double || o is Single) IntInput.Value = Mathf.RoundToInt(Convert.ToSingle(o));
                    break;
                case EType.Bool:
                    if (o is bool) BoolInput.Value = (bool)o;
                    break;
                case EType.Number:

                    if (o is float)
                        FloatInput.Value = (float)(o);
                    else
                        FloatInput.Value = Convert.ToSingle(o);

                    break;
                case EType.Vector3:
                    Vector3Input.Value = (Vector3)o;
                    break;

                case EType.String:
                    StringInput.StringVal = (string)o;
                    break;
            }
        }

#if UNITY_EDITOR

        public override void OnGUIModify()
        {

        }

        public override bool Editor_PreBody()
        {
            Rect r = new Rect(NodeSize.x - 37, 18, 14, 14);
            if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Rename), EditorStyles.label))
            {
                string filename = EditorUtility.SaveFilePanelInProject("Type new name (no file will be created)", ParameterName, "", "Type new display name for the input (no file will be created)");
                if (!string.IsNullOrEmpty(filename)) ParameterName = System.IO.Path.GetFileNameWithoutExtension(filename);
            }

            return false;
        }

        SerializedProperty sp_ParameterName = null;
        public override void Editor_OnAdditionalInspectorGUI()
        {
            if (sp_ParameterName == null) sp_ParameterName = baseSerializedObject.FindProperty("ParameterName");
            EditorGUILayout.PropertyField(sp_ParameterName);
            GUILayout.Space(4);

            //UnityEditor.EditorGUILayout.BeginVertical();
            InputType = (EType)UnityEditor.EditorGUILayout.EnumPopup(InputType, GUILayout.Width(NodeSize.x - 80));
            if (InputType == EType.Cell) InputType = EType.Int;

            GUILayout.Space(-20);
            NodePortBase port = null;

            IntInput.AllowDragWire = false;
            BoolInput.AllowDragWire = false;
            FloatInput.AllowDragWire = false;
            Vector3Input.AllowDragWire = false;
            StringInput.AllowDragWire = false;

            switch (InputType)
            {
                case EType.Int: port = IntInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("IntInput")); break;
                case EType.Bool: port = BoolInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("BoolInput")); break;
                case EType.Number: port = FloatInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("FloatInput")); break;
                case EType.Vector3: port = Vector3Input; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("Vector3Input")); break;
                case EType.String: port = StringInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("StringInput")); break;
            }

            if (port != null) port.AllowDragWire = true;

            //port._EditorCustomOffset = new Vector2(0, -20);
            //if ( port != null)
            //{
            //    port._E_LatestPortRect.position -= new Vector2(0, 22);
            //    port._E_LatestCorrectPortRect.position -= new Vector2(0, 22);
            //}
            //UnityEditor.EditorGUILayout.EndVertical();
        }

#endif

    }
}