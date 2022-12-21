using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.ModGenerating
{

    public class MR_TileDesigner : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Tile Designer" : "Tile Designer"; }
        public override string GetNodeTooltipDescription { get { return "Using Tile Designer to generate game object which can be used to be applied to some spawn"; } }
        public override Color GetNodeColor() { return new Color(0.9f, 0.4f, 0.4f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(218, _EditorFoldout ? 102 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [HideInInspector] public TileDesign Design;
        private GameObject generatedDesign = null;
        public PGGUniversalPort Generated;

        bool generated = false;

        public override void PreGeneratePrepare()
        {
            base.PreGeneratePrepare();
            generated = false;
        }

        public override void OnStartReadingNode()
        {
            Generated.Variable.SetValue(generatedDesign);

            if (generated) return;

            var grid = MG_Grid;
            if (grid == null) return;

            if (generatedDesign) { FGenerators.DestroyObject(generatedDesign); }

            Design.FullGenerateStack();
            generatedDesign = Design.GeneratePrefab();

            generatedDesign.transform.position = new Vector3(10000, -10000, 10000);
            generatedDesign.hideFlags = HideFlags.HideAndDontSave;

            Generated.Variable.SetValue(generatedDesign);
            generated = true;
        }

#if UNITY_EDITOR
        //SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (GUILayout.Button(new GUIContent("  Open Tile Designer", FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Height(24)))
            {
                TileDesignerWindow.Init(Design, this);
            }
        }

#endif

    }
}