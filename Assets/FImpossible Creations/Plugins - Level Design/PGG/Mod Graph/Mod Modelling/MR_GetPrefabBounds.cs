using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_GetPrefabBounds : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("   Get Prefab Bounds") : "Get Prefab Bounds"; }
        public override string GetNodeTooltipDescription { get { return "Getting bounds size of the target prefab"; } }
        public override Color GetNodeColor() { return new Color(0.45f, 0.55f, 0.95f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 124 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Input, 1)] public PGGUniversalPort Prefab;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port BoundsSize;

        [HideInInspector][Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public FloatPort Diagonal;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            GameObject prefab = null;
            Prefab.TriggerReadPort(true);

            object val = Prefab.GetPortValueSafe;
            if (val == null) { return; }
            prefab = val as GameObject;

            if (prefab == null) { return; }

            Renderer r = prefab.GetComponentInChildren<Renderer>();
            if (r == null) return;

            if (BoundsSize.IsConnected)
            {
                BoundsSize.Value = r.bounds.size;
            }

            if (Diagonal.IsConnected)
            {
                Diagonal.Value = Vector3.Distance(r.bounds.min, r.bounds.max);
            }

        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Diagonal");
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                Diagonal.AllowDragWire = true;
            }
            else
            {
                Diagonal.AllowDragWire = false;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}