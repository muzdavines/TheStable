using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Special
{

    public class PR_PushInDirUntilNotCollides : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Push Until not Collides" : "Push In Dir Until Not Collides"; }
        public override string GetNodeTooltipDescription { get { return "Pushing field per cell until not collides with choosed."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        public override Vector2 NodeSize { get { return new Vector2(222, _EditorFoldout ? 124 : 112); } }
        public override bool IsFoldable { get { return true; } }
        public override Color GetNodeColor() { return new Color(0.1f, 0.7f, 1f, 0.95f); }

        [Tooltip("If 'Collision With' left empty or -1 then colliding with every field in the current plan stage")]
        [Port(EPortPinType.Input, 1)] public PGGPlannerPort CollisionWith;
        [Port(EPortPinType.Input, EPortValueDisplay.HideValue, 1)] public PGGVector3Port PushDirection;

        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGPlannerPort ToPush;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            FieldPlanner planner = GetPlannerFromPort(ToPush);
            PushDirection.TriggerReadPort(true);

            Vector3Int pushDir = PushDirection.GetInputValue.normalized.V3toV3Int();
            if (pushDir == Vector3.zero) return;

            planner.LatestChecker._IsCollidingWith_MyFirstCollisionCell = null;

            bool collideWithAll = false;
            if (CollisionWith.PortState() != EPortPinState.Connected)
            {
                if (CollisionWith.UniquePlannerID < 0)
                {
                    collideWithAll = true;
                }
            }

            if (collideWithAll)
            {
                if (ParentPlanner)
                    if (ParentPlanner.ParentBuildPlanner)
                    {
                        var bp = ParentPlanner.ParentBuildPlanner;

                        List<FieldPlanner> all = bp.CollectAllAvailablePlanners(true, true);
                        all.Remove(planner);

                        planner.LatestChecker.StepPushOutOfCollision(all, pushDir, 256);
                    }
            }
            else
            {
                List<FieldPlanner> checkCollWith = GetPlannersFromPort(CollisionWith, false, true, true);
                planner.LatestChecker.StepPushOutOfCollision(checkCollWith, pushDir, 256);
            }

        }



#if UNITY_EDITOR

        private UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                GUILayout.Space(1);

                ToPush.AllowDragWire = true;
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("ToPush");
                UnityEditor.SerializedProperty scp = sp.Copy();
                UnityEditor.EditorGUILayout.PropertyField(scp);
                //scp.Next(false); UnityEditor.EditorGUILayout.PropertyField(scp);
                baseSerializedObject.ApplyModifiedProperties();
            }
            else
            {
                ToPush.AllowDragWire = false;

                if (CollisionWith.PortState() != EPortPinState.Connected)
                    if (CollisionWith.UniquePlannerID < 0)
                    {
                        GUILayout.Space(-2);
                        UnityEditor.EditorGUILayout.HelpBox("Collide with all", UnityEditor.MessageType.None);
                    }
            }
        }

#endif

    }
}