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

    public class MR_GetCellStateOnGrid : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Cell State" : "Cell State on Grid"; }
        public override string GetNodeTooltipDescription { get { return "Get cell state on grid, if it's in grid, out or occupiued by some tag.\nSupports multiple connections and multiple cells lists provided through connections."; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { float extra = multiModeDraw ? 18f : 0f; return new Vector2(CellMustBe == ESR_Space.Occupied ? 262 : 216, (CellMustBe == ESR_Space.Occupied ? 164 : 144) + extra + (_EditorFoldout ? 20 : 0)); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input)] public PGGModCellPort CheckCell;
        public ESR_Space CellMustBe = ESR_Space.Empty;

        //[PGG_SingleLineSwitch("CheckMode", 50, "Select if you want to use Tags, SpawnStigma or CellData", 100)]
        [HideInInspector] [Port(EPortPinType.Input, "Occupied By:")] public PGGStringPort OccupiedBy;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.NotEditable, "Is True")] public BoolPort IsTrue;
        [HideInInspector] public bool NegateResult = false;

        [HideInInspector] public ESR_NeightbourCondition MultiCheck = ESR_NeightbourCondition.AllNeeded;
        [HideInInspector] [Port(EPortPinType.Output, "Found Spawn")] public PGGSpawnPort FoundSpawn;
        bool multiModeDraw = false;

        public override void OnStartReadingNode()
        {
            CheckCell.Clear();
            FoundSpawn.Clear();
            CheckCell.TriggerReadPort(true);

            multiModeDraw = false;
            if (CheckCell.ConnectedWithMultipleCells) multiModeDraw = true;

            if (!multiModeDraw)
            {
                FieldCell cell = CheckCell.GetInputCellValue;

                if (CheckCell.IsConnected == false)  // If not connected, we can use self cell
                    if (FGenerators.IsNull(cell)) { cell = MG_Cell; }

                bool isTrue = SpawnRules.CheckNeightbourCellAllow(CellMustBe, cell, OccupiedBy.GetInputValue, CheckMode);
                IsTrue.Value = NegateResult ? !isTrue : isTrue;
                if (FoundSpawn.IsConnected) GetTheSpawnReference(cell);
            }
            else
            {
                var toCheck = CheckCell.GetAllConnectedCellsList();

                // Checking cells with logics
                bool allCorrect = false;
                if (MultiCheck == ESR_NeightbourCondition.AllNeeded) allCorrect = true;
                string occupiedByTag = OccupiedBy.GetInputValue;

                for (int i = 0; i < toCheck.Count; i++)
                {
                    if (MultiCheck == ESR_NeightbourCondition.AtLeastOne)
                    {
                        if (SpawnRules.CheckNeightbourCellAllow(CellMustBe, toCheck[i], occupiedByTag, CheckMode))
                        {
                            allCorrect = true;
                            if (FoundSpawn.IsConnected) GetTheSpawnReference(toCheck[i]);
                            break;
                        }
                    }
                    else if (MultiCheck == ESR_NeightbourCondition.AllNeeded)
                    {
                        if (!SpawnRules.CheckNeightbourCellAllow(CellMustBe, toCheck[i], occupiedByTag, CheckMode))
                        {
                            allCorrect = false;
                            if (FoundSpawn.IsConnected) GetTheSpawnReference(toCheck[i]);
                            break;
                        }
                    }
                }

                IsTrue.Value = NegateResult ? !allCorrect : allCorrect;

            }
        }

        void GetTheSpawnReference(FieldCell cell)
        {
            if (cell == null) return;

            SpawnData spawn = null;
            FoundSpawn.Clear();
            if (CellMustBe == ESR_Space.Empty) { }
            else if (CellMustBe == ESR_Space.InGrid)
            {
                if (cell.GetJustCellSpawnCount() > 0) spawn = cell.CollectSpawns()[0];
            }
            else if (CellMustBe == ESR_Space.OutOfGrid)
            { }
            else if (CellMustBe == ESR_Space.Occupied)
            {
                string tagVal = OccupiedBy.GetInputValue;

                if (string.IsNullOrEmpty(tagVal))
                {
                    if (cell.GetJustCellSpawnCount() > 0) spawn = cell.CollectSpawns()[0];
                }
                else
                {
                    if (cell.GetJustCellSpawnCount() > 0)
                    {
                        spawn = SpawnRuleBase.GetSpawnDataWithSpecifics(cell, tagVal, CheckMode);
                    }
                }
            }

            if (spawn != null) FoundSpawn.FirstSpawnForOutputPort = spawn;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();
            OccupiedBy.AllowDragWire = false;

            multiModeDraw = false;
            if (CheckCell.ConnectedWithMultipleCells) multiModeDraw = true;

            if (sp == null) sp = baseSerializedObject.FindProperty("OccupiedBy");
            SerializedProperty spc = sp.Copy();


            if (CellMustBe == ESR_Space.Occupied)
            {
                EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 100)); spc.Next(false);
                GUILayout.Space(-EditorGUIUtility.singleLineHeight);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(NodeSize.x - 96); EditorGUILayout.PropertyField(spc, GUIContent.none, GUILayout.Width(42));
                EditorGUILayout.EndHorizontal();
                spc.Next(false);
                OccupiedBy.AllowDragWire = true;
            }
            else
            {
                spc.Next(false); spc.Next(false);
            }

            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc); spc.Next(false);

            if (multiModeDraw)
            {
                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            if (_EditorFoldout)
            {
                spc.Next(false);
                EditorGUILayout.PropertyField(spc);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}