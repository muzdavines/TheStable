using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_SetScale : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Set Spawn Scale" : "Set Spawn Scale"; }
        public override string GetNodeTooltipDescription { get { return "Setting new scale of spawn"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(240, _EditorFoldout ? 122 : 84); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public enum EOperation
        {
            Set, Add, Subtract
        }

        [Port(EPortPinType.Input, 1)] public PGGVector3Port Scale;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGSpawnPort Spawn;
        [HideInInspector] public EOperation Operation = EOperation.Set;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void OnCreated()
        {
            base.OnCreated();
            Scale.Value = Vector3.one;
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Scale.TriggerReadPort(true);
            Vector3 scaleVal = Scale.GetInputValue;

            Spawn.TriggerReadPort(true);
            SpawnData sp = Spawn.GetInputCellValue as SpawnData;
            if (FGenerators.IsNull(sp)) sp = MG_Spawn;
            if (FGenerators.IsNull(sp)) return;

            if ( Operation == EOperation.Set)
            {
                sp.LocalScaleMul = scaleVal;
            }
            else if (Operation == EOperation.Add)
            {
                sp.LocalScaleMul += scaleVal;
            }
            else if (Operation == EOperation.Subtract)
            {
                sp.LocalScaleMul -= scaleVal;
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
                if (sp == null) sp = baseSerializedObject.FindProperty("Spawn");
                EditorGUILayout.PropertyField(sp);
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}