using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetOtherCellInDistance : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Distance to other cells" : "Distance to other cells"; }
        public override string GetNodeTooltipDescription { get { return "Measuring distance to other cells with desired parameters if detected on a way."; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(250, _EditorFoldout ? 190 : 144); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public ESR_DistanceRule DistanceMustBe = ESR_DistanceRule.Greater;

        [HideInInspector][Port(EPortPinType.Input)] public FloatPort Distance;
        [HideInInspector] public ESR_Measuring MeasureIn = ESR_Measuring.Cells;

        [HideInInspector][Port(EPortPinType.Input)] public PGGStringPort With;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        //[HideInInspector] [Port(EPortPinType.Output)] public PGGCellPort GatheredCells;
        [HideInInspector][Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public BoolPort Result;
        [HideInInspector] public bool IgnoreSelf = true;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGCellPort OriginCell;


        public override void OnCreated()
        {
            base.OnCreated();
            Distance.Value = 3f;
        }

        public override void OnStartReadingNode()
        {
            Distance.TriggerReadPort(true);
            Result.Value = false;

            if (Distance.Value > 32) Distance.Value = 32;

            With.TriggerReadPort(true);

            OriginCell.TriggerReadPort(true);

            FieldCell targetCell;
            if (OriginCell.IsConnected == false) targetCell = MG_Cell;
            else targetCell = OriginCell.GetInputCellValue;

            if (FGenerators.IsNull(targetCell)) return;

            RefreshDistanceLimitsValue();

            float desiredDist = Distance.GetInputValue;
            int indexDistance = (int)desiredDist;
            float worldDistance = desiredDist;

            if (MeasureIn == ESR_Measuring.Units)
            {
                indexDistance = Mathf.CeilToInt(desiredDist / MG_Preset.CellSize);
                if (indexDistance < 1) indexDistance = 1;
            }
            else // World distance multiplied by cell size of the room preset
            {
                worldDistance = desiredDist * MG_Preset.CellSize;
            }

            ESR_DistanceRule? result = null;

            List<FieldCell> cells = MG_Grid.GetDistanceSquare2DList(targetCell, indexDistance, MG_Preset.CellSize, worldDistance);
            if (IgnoreSelf) if (cells.Contains(targetCell)) cells.Remove(targetCell);

            for (int i = 0; i < cells.Count; i++)
            {
                var dCell = cells[i];
                var spawns = dCell.CollectSpawns(MG_Spawner.ScaleAccess);

                for (int s = 0; s < spawns.Count; s++)
                {
                    SpawnData spwn = spawns[s];
                    bool prInRange = false;
                    bool modInRange = false;
                    bool tagInRange = false;

                    if (SpawnRuleBase.SpawnHaveSpecifics(spwn, With.GetInputValue, CheckMode)) tagInRange = true;

                    if (prInRange || modInRange || tagInRange)
                    {
                        if (MeasureIn == ESR_Measuring.Cells)
                        {
                            float distance = (Vector3.Distance((Vector3)dCell.Pos * MG_Preset.CellSize, (Vector3)targetCell.Pos * MG_Preset.CellSize));
                            if (/*Mathf.RoundToInt*/(distance) == worldDistance) result = ESR_DistanceRule.Equal;
                            else if (distance > worldDistance) result = ESR_DistanceRule.Greater;
                            else result = ESR_DistanceRule.Lower;
                        }
                        else // Measure in units including spawn's offsets positioning
                        {
                            Vector3 targetPos = spwn.GetWorldPositionWithFullOffset(MG_Preset);
                            float distance = (Vector3.Distance(targetCell.WorldPos(MG_Preset.CellSize), targetPos));
                            if (distance == desiredDist) result = ESR_DistanceRule.Equal;
                            else if (distance > desiredDist) result = ESR_DistanceRule.Greater;
                            else result = ESR_DistanceRule.Lower;
                        }

                        if (result == DistanceMustBe)
                        {
                            Result.Value = true;
                            return;
                        }
                    }
                }
            }

            if (result == null) // Not found any cell for conditions - far away or too near
            {
                if (DistanceMustBe == ESR_DistanceRule.Greater)
                    Result.Value = true;
                else
                    Result.Value = false;
            }
            UnityEngine.Debug.Log("dida " + Result.Value);
        }

        void RefreshDistanceLimitsValue()
        {
            if (MeasureIn == ESR_Measuring.Cells)
            {
                if (Distance.Value > 32) Distance.Value = 32;
            }
            else
                if (Distance.Value > 64) Distance.Value = 64;

            if (Distance.Value < 0) Distance.Value = 0;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Distance");
            SerializedProperty spc = sp.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 120)); spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 120)); spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.PropertyField(spc); spc.Next(false);

            if (_EditorFoldout)
            {
                //EditorGUILayout.PropertyField(spc); spc.Next(false);
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
            }

            RefreshDistanceLimitsValue();

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}