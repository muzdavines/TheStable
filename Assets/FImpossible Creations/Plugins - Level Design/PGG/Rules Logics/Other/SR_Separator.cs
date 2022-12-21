using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Other
{
    public class SR_Separator : SpawnRuleBase, ISpawnProceduresDecorator
    {
        public override string TitleName() { return Header; }
        public override string Tooltip() { return TooltipText; }
#if UNITY_EDITOR
        public override bool EditorActiveHeader() { return true; }
#endif

        public string Header = "Separator";
        public string Info = "Just to separate few rules";
        public string TooltipText = "Configure this separator as custom decorator for displaying rules in this spawner";
        [Range(0, 32)] public int Padding = 4;
        [Range(14, 64)] public int Height = 16;
        public Color color = new Color(0.7f, 0.7f, 0.7f, 1f);
        public enum ESR_SeparatorStyle { Basic, HelpBox, Label, MiniLabel }
        public ESR_SeparatorStyle Style = ESR_SeparatorStyle.Basic;
#if UNITY_EDITOR
        public GUIStyle DisplayStyle { get 
            {
                switch (Style)
                {
                    case ESR_SeparatorStyle.HelpBox: return EditorStyles.helpBox;
                    case ESR_SeparatorStyle.Label: return EditorStyles.boldLabel;
                    case ESR_SeparatorStyle.MiniLabel: return EditorStyles.centeredGreyMiniLabel;
                }

                return FGUI_Resources.BGInBoxStyleH; 
            } }

        public int DisplayHeight { get { return Height; } }
        public Color DisplayColor { get { return color; } }
        public int UpPadding { get { return Padding; } }
        public int DownPadding { get { return Padding; } }

        public override void NodeBody(SerializedObject so)
        {
            if (GUIIgnore.Count == 0) GUIIgnore.Add("Info");
            Info = EditorGUILayout.TextArea(Info);
            GUILayout.Space(8);

            base.NodeBody(so);
        }
#endif

    }
}