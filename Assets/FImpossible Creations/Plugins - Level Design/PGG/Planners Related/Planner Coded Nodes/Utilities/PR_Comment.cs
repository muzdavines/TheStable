using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Utilities
{

    public class PR_Comment : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return Header; }
        public override Color GetNodeColor() { return FrameColor; }
#if UNITY_EDITOR
        public override Vector2 NodeSize { get { return new Vector2(defaultWidth, _EditorFoldout ? foldoutHeight : defaultHeight) + AddSizeTweak; } }
#endif
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }
        public override bool IsFoldableFix { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Cosmetics; } }

        [HideInInspector] public string Header = "Comment";
        [HideInInspector] public string CommentText = "Unfold to write your comment";

        [HideInInspector] public Color FrameColor = new Color(0.3f, 0.3f, .3f, 1f);
        [HideInInspector] public Color TextColor = new Color(1f, 1f, 1f, 0.95f);
        [HideInInspector] public Vector2 AddSizeTweak = new Vector2(0, 0);
        //[HideInInspector] public bool InfoIcon = true;


#if UNITY_EDITOR
        SerializedProperty sp;
        float foldoutHeight = 150;
        float defaultHeight = 80;
        float defaultWidth = 180;
        GUIContent cont = new GUIContent();
        //GUIContent infoIc = null;
        //if (InfoIcon)
        //{
        //    if (infoIc == null) infoIc = new GUIContent(FGUI_Resources.Tex_Info);
        //    EditorGUILayout.LabelField(infoIc);
        //}

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if (sp == null) sp = baseSerializedObject.FindProperty("Header");

            if (_EditorFoldout)
            {
                EditorGUIUtility.labelWidth = 50;
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc); spc.Next(false);

                spc.stringValue = EditorGUILayout.TextArea(spc.stringValue);
                spc.Next(false);

                cont.text = CommentText;
                Vector2 fsz = EditorStyles.textArea.CalcSize(cont);

                // Params when foldout
                foldoutHeight = 148 + fsz.y;
                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                //EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                GUILayout.Space(-28);
                Color preC = GUI.color;
                GUI.color = TextColor;
                EditorGUILayout.LabelField(CommentText, FGUI_Resources.HeaderStyle, GUILayout.ExpandHeight(true));
                GUI.color = preC;
            }

            cont.text = CommentText;
            Vector2 calSz = FGUI_Resources.HeaderStyle.CalcSize(cont);
            defaultWidth = calSz.x;
            defaultHeight = calSz.y;

            cont.text = Header;
            calSz = FGUI_Resources.HeaderStyle.CalcSize(cont);

            defaultWidth = Mathf.Max(calSz.x, defaultWidth);
            defaultWidth += 67;
            defaultHeight = Mathf.Max(calSz.y, defaultHeight);

            if (string.IsNullOrEmpty(CommentText))
                defaultHeight += 38;
            else
                defaultHeight += 68;

            defaultWidth = Mathf.Max(defaultWidth, 150);
        }
#endif

    }
}