using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_GetCommandDirection : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Command Direction" : "Get Command Direction"; }
        public override string GetNodeTooltipDescription { get { return "Getting direction of command if available (direction defined by cell command rotation)"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 186 : 180, _EditorFoldout ? 106 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ModGraphNode; } }

        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port Direction;

        public override void OnStartReadingNode()
        {
            if (Rules.QuickSolutions.SR_ModGraph.Graph_RestrictDir != null)
                Direction.Value = Rules.QuickSolutions.SR_ModGraph.Graph_RestrictDir.Value;
            else
                Direction.Value = Vector3.zero;
        }


#if UNITY_EDITOR
        //SerializedProperty sp = null;
        //public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        //{
        //    GUILayout.BeginHorizontal();
        //    float wdth = NodeSize.x - 88;

        //    GUILayout.Space(8);
        //    GetMode = (EGetMode)EditorGUILayout.EnumPopup(GetMode, GUILayout.Width(wdth));
        //    GUILayout.Space(31);
        //    GUILayout.EndHorizontal();

        //    base.Editor_OnNodeBodyGUI(setup);
        //    baseSerializedObject.Update();

        //    if (sp == null) sp = baseSerializedObject.FindProperty("Position");
        //    GUILayout.Space(-19);
        //    EditorGUILayout.PropertyField(sp);

        //    if (_EditorFoldout)
        //    {
        //        SerializedProperty spc = sp.Copy(); spc.Next(false);
        //        EditorGUILayout.PropertyField(spc); 
        //    }

        //    baseSerializedObject.ApplyModifiedProperties();
        //}

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Position Value: " + Direction.Value);
        }
#endif

    }
}