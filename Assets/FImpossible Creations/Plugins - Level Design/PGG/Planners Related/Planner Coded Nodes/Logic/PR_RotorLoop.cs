using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops
{

    public class PR_RotorLoop : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Iterate Rotations" : "Iterate Rotations"; }
        public override string GetNodeTooltipDescription { get { return "Running loop iteration for Y axis rotation check."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 220 : 188, _EditorFoldout ? 161 : 131); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override int AllowedOutputConnectionIndex { get { return 0; } }

        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Finish";
            return "Iteration";
        }

        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port IterationRotation;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Stop (Break)")] public BoolPort Stop;

        [HideInInspector] public List<Vector3> Rotations = new List<Vector3>();

        public override void OnCreated()
        {
            base.OnCreated();
            Rotations = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, 270, 0) };
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            for (int r = 0; r < Rotations.Count; r++)
            {
                Stop.TriggerReadPort(true);
                if (Stop.GetInputValue == true) break;

                IterationRotation.Value = Rotations[r];
                CallOtherExecutionWithConnector(1, print);
            }
        }

#if UNITY_EDITOR

        int selected = 0;
        //SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();

                if (Rotations.Count > 0)
                {
                    GUILayout.Space(4);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("[" + selected + "] Angles", GUILayout.Width(64));
                    if (GUILayout.Button("<", GUILayout.Width(20))) selected -= 1;
                    if (GUILayout.Button(">", GUILayout.Width(20))) selected += 1;
                    if (GUILayout.Button("+", GUILayout.Width(20))) { Rotations.Add(new Vector3()); }
                    if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(19))) { Rotations.RemoveAt(selected); selected -= 1; }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    if (selected >= Rotations.Count) selected = 0;
                    if (selected < 0) selected = Rotations.Count - 1;

                    Rotations[selected] = EditorGUILayout.Vector3Field(GUIContent.none, Rotations[selected]);

                }

                baseSerializedObject.ApplyModifiedProperties();
            }

            if (!_EditorFoldout) EditorGUILayout.HelpBox("Iterate " + Rotations.Count + " rotations", MessageType.None);
        }

#endif
    }
}