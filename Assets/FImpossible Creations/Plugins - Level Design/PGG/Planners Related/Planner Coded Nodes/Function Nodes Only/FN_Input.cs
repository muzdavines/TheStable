using FIMSpace.Graph;
using UnityEngine;
using System;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.FunctionNode
{

    public class FN_Input : PlannerRuleBase
    {
        [HideInInspector] public string InputName = "Input";
        public override string GetDisplayName(float maxWidth = 120) { return InputName; }
        public override string GetNodeTooltipDescription { get { return "Defining input port for other nodes which will use this function node.\nCan be ordered through inspector window if you select this function node file"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Externals; } }
        public override EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.JustFunctions; } }

        public override Vector2 NodeSize { get { return new Vector2(Mathf.Max(160, InputName.Length * 12), 84); } }
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
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGCellPort CellInput;


        public NodePortBase GetFunctionOutputPort()
        {
            switch (InputType)
            {
                case EType.Int: return IntInput;
                case EType.Bool: return BoolInput;
                case EType.Number: return FloatInput;
                case EType.Vector3: return Vector3Input;
                case EType.String: return StringInput;
                case EType.Cell: return CellInput;
            }

            return null;
        }


        public void SetValueOf(NodePortBase p)
        {
            if (p == null) { /*UnityEngine.Debug.Log("Null port value!");*/ return; }
            object o = p.GetPortValueSafe;

            switch (InputType)
            {
                case EType.Int:
                    if (o != null)
                        if (o is int || o is float || o is double || o is Single) IntInput.Value = Mathf.RoundToInt(Convert.ToSingle(o));
                    break;

                case EType.Bool:
                    if (o != null)
                        if (o is bool) BoolInput.Value = (bool)o;
                    break;

                case EType.Number:

                    if (o != null)
                    {
                        if (o is float)
                            FloatInput.Value = (float)(o);
                        else
                            FloatInput.Value = Convert.ToSingle(o);
                    }

                    break;
                case EType.Vector3:
                    if (o != null)
                        Vector3Input.Value = (Vector3)o;
                    break;

                case EType.String:
                    if (o != null)
                        if (o is string) StringInput.StringVal = (string)o;
                    break;

                case EType.Cell:
                    if (p is PGGCellPort) CellInput.ProvideFullCellData(p as PGGCellPort);
                    break;
            }
        }


#if UNITY_EDITOR

        public override bool Editor_PreBody()
        {
            Rect r = new Rect(NodeSize.x - 37, 18, 14, 14);
            if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Rename), EditorStyles.label))
            {
                string filename = EditorUtility.SaveFilePanelInProject("Type new name (no file will be created)", InputName, "", "Type new display name for the input (no file will be created)");
                if (!string.IsNullOrEmpty(filename)) InputName = System.IO.Path.GetFileNameWithoutExtension(filename);
            }

            Color preC = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            r.size = new Vector2(12, 12);
            r.position = new Vector2(24, r.position.y + 1);
            //if (_port == null) _port = Resources.Load<Texture2D>("ESPR_InputConnected");
            if (_port2 == null) _port2 = Resources.Load<Texture2D>("ESPR_Input.fw");
            GUI.DrawTexture(r, _port2);
            //GUI.DrawTexture(r, _port);
            GUI.color = preC;

            return false;
        }

        //Texture2D _port = null;
        Texture2D _port2 = null;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            //UnityEditor.EditorGUILayout.BeginVertical();
            InputType = (EType)UnityEditor.EditorGUILayout.EnumPopup(InputType, GUILayout.Width(NodeSize.x - 80));

            GUILayout.Space(-20);
            NodePortBase port = null;

            IntInput.AllowDragWire = false;
            BoolInput.AllowDragWire = false;
            FloatInput.AllowDragWire = false;
            Vector3Input.AllowDragWire = false;
            StringInput.AllowDragWire = false;
            CellInput.AllowDragWire = false;

            switch (InputType)
            {
                case EType.Int: port = IntInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("IntInput")); break;
                case EType.Bool: port = BoolInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("BoolInput")); break;
                case EType.Number: port = FloatInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("FloatInput")); break;
                case EType.Vector3: port = Vector3Input; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("Vector3Input")); break;
                case EType.String: port = StringInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("StringInput")); break;
                case EType.Cell: port = CellInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("CellInput")); break;
            }

            if (port != null) port.AllowDragWire = true;

        }

#endif

#if UNITY_EDITOR

        SerializedProperty sp_InputName = null;
        public override void Editor_OnAdditionalInspectorGUI()
        {
            if (sp_InputName == null) sp_InputName = baseSerializedObject.FindProperty("InputName");
            EditorGUILayout.PropertyField(sp_InputName);
            GUILayout.Space(4);

            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);

            switch (InputType)
            {
                case EType.Int: GUILayout.Label("Port Value: " + IntInput.Value); break;
                case EType.Bool: GUILayout.Label("Port Value: " + BoolInput.Value); break;
                case EType.Number: GUILayout.Label("Port Value: " + FloatInput.Value); break;
                case EType.Vector3: GUILayout.Label("Port Value: " + Vector3Input.Value); break;
                case EType.String: GUILayout.Label("Port Value: " + StringInput.StringVal); break;
                case EType.Cell:
                    GUILayout.Label("Port Value: " + CellInput.Cell);
                    if (CellInput.Cell != null) GUILayout.Label("Cell Pos: " + CellInput.Cell.Pos);
                    if (CellInput.Checker != null)
                    {
                        GUILayout.Label("Cell World Pos: " + CellInput.Checker.GetWorldPos(CellInput.Cell));

                        if ( CellInput.GetInputCheckerValue != null)
                        GUILayout.Label("Parent Field Cells Count: " + CellInput.GetInputCheckerValue.ChildPositionsCount);
                    }

                    break;
            }

        }
#endif

    }
}