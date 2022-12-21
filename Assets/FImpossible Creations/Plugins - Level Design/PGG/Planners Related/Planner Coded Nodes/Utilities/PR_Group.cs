using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Utilities
{

    public class PR_Group : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  " + GroupTitle : "Nodes Group"; }
        public override string GetNodeTooltipDescription { get { return "Just container for multiple nodes to hide / unhide them"; } }
        public override Color GetNodeColor() { return GroupColor; }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? ResizedScale.x : titleWidth, _EditorFoldout ? ResizedScale.y : 92); } }
        public override bool IsFoldable { get { return true; } }
        public override bool IsResizable { get { return _EditorFoldout; } }
        public override bool IsContainable { get { return true; } }

        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Debug; } }

        [HideInInspector] public string GroupTitle = "Nodes Group";
        [HideInInspector] public Color GroupColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        private float titleWidth = 200;

        public override void OnCreated()
        {
            if (!wasCreated) _EditorFoldout = true;
            base.OnCreated();
        }


#if UNITY_EDITOR


        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            Color preC = GUI.color;

            base.Editor_OnNodeBodyGUI(setup);

            titleWidth = Mathf.Max(200f, EditorStyles.label.CalcSize(new GUIContent(GroupTitle)).x + 110);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename), GUILayout.Width(26), GUILayout.Height(19)))
                {
                    string newTitle = FGenerators.RenamePopup(null, GroupTitle, false);
                    if (!string.IsNullOrEmpty(newTitle)) GroupTitle = newTitle;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GroupColor = EditorGUILayout.ColorField(GroupColor, GUILayout.Width(32));
                GUILayout.Space(5);
                EditorGUILayout.HelpBox("Hiding " + _EditorInContainedRange.Count + " Nodes inside the Group", MessageType.None);
                EditorGUILayout.EndHorizontal();
            }

            baseSerializedObject.ApplyModifiedProperties();
            GUI.color = preC;

        }

#endif

    }
}