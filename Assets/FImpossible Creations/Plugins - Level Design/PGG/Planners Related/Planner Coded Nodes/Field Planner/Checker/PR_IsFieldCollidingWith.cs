using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Checker
{

    public class PR_IsFieldCollidingWith : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Is Colliding With" : "Is Field Colliding With"; }
        public override string GetNodeTooltipDescription { get { return "Check if field collides with another"; } }
        public override Color GetNodeColor() { return new Color(0.07f, 0.66f, 0.56f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 246 : 200, _EditorFoldout ? 126 : 102); } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort CollidingWith;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue, 1)] [Tooltip("If collision occured then true, if no then false")] public BoolPort IsColliding;
        [HideInInspector] [Port(EPortPinType.Input, 1)] [Tooltip("Using self if no input")] public PGGPlannerPort FirstColliderField;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            FieldPlanner aPlanner = GetPlannerFromPort(FirstColliderField);
            List<FieldPlanner> bPlanners = GetPlannersFromPort(CollidingWith);

            IsColliding.Value = false;

            bool collided = false;

            FieldPlanner collWith = aPlanner.LatestChecker.IsCollidingWith(bPlanners);

            if (collWith != null)
            {
                collided = true;
            }

            IsColliding.Value = collided;

            if (Debugging)
            {
                if (collWith != null)
                {
                    DebuggingInfo = "Checking collision and detected with " + collWith.name + " " + collWith.ArrayNameString;
                }
                else
                {
                    DebuggingInfo = "Checking collision but no collision detected";
                }

                print._debugLatestExecuted = aPlanner.LatestResult.Checker;
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (!_EditorFoldout) return;

            if (_EditorFoldout)
            {
                FirstColliderField.AllowDragWire = true;
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("FirstColliderField");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc);
                baseSerializedObject.ApplyModifiedProperties();
            }
            else
            {
                FirstColliderField.AllowDragWire = false;
            }

        }
#endif

    }
}