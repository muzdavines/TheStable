#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Count
{
    public partial class SR_LimitSpawnCount : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Limit Spawning Count"; }
        public override string Tooltip() { return "Limiting spawning count on field by spawning count of this spawner\n[Lightweight]"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public MinMax Count = new MinMax(1, 1);
        [HideInInspector] public SpawnerVariableHelper CountMulVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Number);

        //[HideInInspector] public bool UsePresetCountMultiplier = false;

        [Tooltip("Inheriting by grid's cells count")]
        [HideInInspector] public bool InheritFromCellsCount = false;
        [HideInInspector] public MinMax RoomCellsDivBy = new MinMax(4, 6);
        //[HideInInspector] public SpawnerVariableHelper RoomCellsMulVariable;

        public int created { get; private set; }
        int max = 0;

        private int lastCount = 0;

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            AllConditionsMet = false;
            created = 0;

            if (InheritFromCellsCount == false)
                max = FGenerators.GetRandom(Count);
            else
            {
                if (RoomCellsDivBy.Min < 0 || RoomCellsDivBy.Max < 0) RoomCellsDivBy = new MinMax(1, 1);
                max = grid.AllApprovedCells.Count / FGenerators.GetRandom(RoomCellsDivBy);
            }
            if (max > Count.Max) max = Count.Max;
            if (max < Count.Min) max = Count.Min;

            float mul = CountMulVariable.GetValue(1f);
            max = Mathf.RoundToInt(max * mul);

            lastCount = grid.AllApprovedCells.Count;
            base.ResetRule(grid, preset);
        }

        public override List<SpawnerVariableHelper> GetVariables() 
        {
            return CountMulVariable.GetListedVariable();
            //List<SpawnerVariableHelper> hl = new List<SpawnerVariableHelper>();
            //SpawnerVariableHelper h = CountMulVariable.GetVariable();
            //if (h is null == false) hl.Add(h);
            //h = RoomCellsMulVariable.GetVariable();
            //if (h is null == false) hl.Add(h);
            //if (hl.Count == 0) return null;
            //return hl;
        }


#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            if (GUIIgnore.Count == 0) { GUIIgnore.Add("Count"); }
            SerializedProperty sp = so.FindProperty("Count");
            if (CountMulVariable != null) CountMulVariable.requiredType = FieldVariable.EVarType.Number;
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp);
            sp.Next(false);
            EditorGUILayout.PropertyField(sp);
            GUILayout.Space(1);
            //ModificatorsPack.DrawFieldSetupInheritToggle(sp, "Use FieldSetup CountMultiplier parameter to change count value in this rule");
            //EditorGUILayout.EndHorizontal();

            //GUILayout.Space(4);
            sp.Next(false);
            EditorGUIUtility.labelWidth = 180;
            EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 0;

            if ( sp.boolValue)
            {
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);
            }
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            if (InheritFromCellsCount)
                //EditorGUILayout.LabelField("Limit spawn count of Room Modification", EditorStyles.centeredGreyMiniLabel);
                if (lastCount != 0)
                {
                    EditorGUILayout.LabelField("Last grid's cells count: " + lastCount, EditorStyles.centeredGreyMiniLabel);

                    int min = lastCount / RoomCellsDivBy.Max;
                    int max = lastCount / RoomCellsDivBy.Min;
                    if (max > Count.Max) max = Count.Max;
                    if (min < Count.Min) min = Count.Min;

                    if (RoomCellsDivBy.Min > 0 && RoomCellsDivBy.Max > 0)
                        EditorGUILayout.LabelField("Spawns " + (min) + " up to " + (max), EditorStyles.centeredGreyMiniLabel);

                    if (RoomCellsDivBy.Min < 0) RoomCellsDivBy.Min = 1;
                    if (RoomCellsDivBy.Max < 0) RoomCellsDivBy.Max = 1;
                }
        }
#endif
        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (created < max) CellAllow = true; else CellAllow = false;
        }

        public override void OnAddSpawnUsingRule(FieldModification mod, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            created++;
            if (created >= max) AllConditionsMet = true;
        }

    }
}