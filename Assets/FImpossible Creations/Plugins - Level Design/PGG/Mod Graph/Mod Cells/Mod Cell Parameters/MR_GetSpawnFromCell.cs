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

    public class MR_GetSpawnFromCell : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Spawn From Cell" : "Get Spawn From Cell"; }
        public override string GetNodeTooltipDescription { get { return "Get first or all found spawns with desired parameters."; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(220, _EditorFoldout ? 142 : 120); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, 1)] public PGGModCellPort CheckCell;
        [HideInInspector] [Port(EPortPinType.Input)] public PGGStringPort OccupiedBy;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;
        [HideInInspector] [Port(EPortPinType.Output, "Found Spawn")] public PGGSpawnPort Found;
        [HideInInspector] public bool GetAllMatchingSpawns = false;

        public override void OnStartReadingNode()
        {
            CheckCell.Clear();
            Found.Clear();
            CheckCell.TriggerReadPort(true);

            FieldCell cell = CheckCell.GetInputCellValue;

            if (CheckCell.IsConnected)  // If not connected, we can use self cell
                if (FGenerators.IsNull(cell)) { cell = MG_Cell; }

            if (FGenerators.IsNull(cell)) return;
            GetTheSpawnReference(cell);
        }

        void GetTheSpawnReference(FieldCell cell)
        {
            if (cell == null) return;

            SpawnData spawn = null;
            string tagVal = OccupiedBy.GetInputValue;

            if (string.IsNullOrEmpty(tagVal))
            {
                if (cell.GetJustCellSpawnCount() > 0)
                {
                    if (GetAllMatchingSpawns)
                    {
                        Found.ApplySpawnsGroup(cell.CollectSpawns());
                    }
                    else
                    {
                        spawn = cell.CollectSpawns()[0];
                    }
                }
            }
            else
            {
                if (cell.GetJustCellSpawnCount() > 0)
                {
                    if (GetAllMatchingSpawns)
                    {
                        Found.ApplySpawnsGroup(SpawnRuleBase.GetAllSpecificSpawns(cell, tagVal, CheckMode));
                    }
                    else
                        spawn = SpawnRuleBase.GetSpawnDataWithSpecifics(cell, tagVal, CheckMode);
                }
            }

            if (spawn != null) Found.FirstSpawnForOutputPort = spawn;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();
            OccupiedBy.AllowDragWire = false;

            if (sp == null) sp = baseSerializedObject.FindProperty("OccupiedBy");
            SerializedProperty spc = sp.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spc, GUIContent.none, GUILayout.Width(NodeSize.x - 110)); spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);

            if (_EditorFoldout)
            {
                EditorGUIUtility.labelWidth = 149;
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}