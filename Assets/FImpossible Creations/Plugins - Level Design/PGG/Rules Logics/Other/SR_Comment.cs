using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Other
{
    public class SR_Comment : SpawnRuleBase, ISpawnProceduresDecorator
    {
        public override string TitleName() { if (!CommentInHeader) return "Comment"; else return Info; }
        public override string Tooltip() { return "Comment which does nothing to cells or spawns, just place here info of what is happening with rule logics in this spawner"; }
#if UNITY_EDITOR
        public override bool EditorActiveHeader() { return true; }
#endif

        public string Info = "Comment about spawner";
        public bool CommentInHeader = false;
        public Vector2Int Padding = new Vector2Int(8, 8);

#if UNITY_EDITOR
        public GUIStyle DisplayStyle { get { return EditorStyles.helpBox; } }
        public int DisplayHeight { get { return 16; } }
        public Color DisplayColor { get { return new Color(0.6f, 0.6f, 0.6f, 1f); } }
        public int UpPadding { get { return Padding.x; } }
        public int DownPadding { get { return Padding.y; } }

        public override void NodeBody(SerializedObject so)
        {
            if (GUIIgnore.Count == 0) GUIIgnore.Add("Info");
            Info = EditorGUILayout.TextArea(Info);

            FGUI_Inspector.DrawUILine(0.4f, 0.0f, 1, 7);
        }
#endif

    }
}