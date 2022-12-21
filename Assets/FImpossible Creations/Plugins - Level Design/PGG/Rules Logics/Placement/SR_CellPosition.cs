using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Placement
{
    public partial class SR_CellPosition : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Grid Position"; }
        public override string Tooltip() { return "Allowing or disallowing running this spawner when this cell have certain position on grid\n[Lightweight] " + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public enum EPositionMode { ExactPosition, Greater, Lower, Between, InCenter, GridMax, GridMin, EveryFew_Modulo }

        [PGG_SingleLineTwoProperties("Axis", 100, 40, 10, -80)]
        public EPositionMode Mode = EPositionMode.ExactPosition;

        public enum EAxis { X, Y, Z }
        [HideInInspector] public EAxis Axis = EAxis.Y;

        [HideInInspector] public int Exact = 0;
        public override List<SpawnerVariableHelper> GetVariables() { return ValueMulVariable.GetListedVariable(); }
        [HideInInspector] public int Offset = 0;
        [HideInInspector] public Vector2Int Range = new Vector2Int(-3, 3);
        [HideInInspector] public SpawnerVariableHelper ValueMulVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Number);

        #region Back Compability thing
#if UNITY_EDITOR
        public override void NodeBody(UnityEditor.SerializedObject so)
        {
            base.NodeBody(so);
            ValueMulVariable.requiredType = FieldVariable.EVarType.Number;
        }
#endif
        #endregion

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);
            GUILayout.Space(5);
            SerializedProperty sp = so.FindProperty("Exact");
            if (Mode == EPositionMode.ExactPosition || Mode == EPositionMode.EveryFew_Modulo)
            {
                if (Mode == EPositionMode.EveryFew_Modulo)
                {
                    EditorGUILayout.PropertyField(sp, new GUIContent("Every"));
                    sp.Next(false); EditorGUILayout.PropertyField(sp);
                    sp.Next(false); sp.Next(false); EditorGUILayout.PropertyField(sp);
                }
                else
                {
                    EditorGUILayout.PropertyField(sp);
                    sp.Next(false); sp.Next(false); sp.Next(false); EditorGUILayout.PropertyField(sp);
                }
            }
            else
            if (Mode == EPositionMode.Greater || Mode == EPositionMode.Lower)
            {
                EditorGUILayout.PropertyField(sp, new GUIContent("Than"));
                sp.Next(false); sp.Next(false); sp.Next(false); EditorGUILayout.PropertyField(sp);
            }
            else
            {
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
            }


            if (Mode == EPositionMode.InCenter)
            {
                EditorGUILayout.HelpBox("Set ranges like 1,1 not -1,1", MessageType.None);
            }
        }
#endif

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            int refInf = Mathf.RoundToInt(Exact * ValueMulVariable.GetValue(1f));
            int refOff = Mathf.RoundToInt(Offset * ValueMulVariable.GetValue(1f));

            if (Mode == EPositionMode.ExactPosition)
            {
                if (GetAxisValue(cell.Pos) == refInf)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.Greater)
            {
                if (GetAxisValue(cell.Pos) > refInf)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.Lower)
            {
                if (GetAxisValue(cell.Pos) < refInf)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.Between)
            {
                int val = GetAxisValue(cell.Pos);
                if (val > refOff - Range.x && val < refOff + Range.y)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.InCenter)
            {
                int val = GetAxisValue(cell.Pos);
                int off = GetAxisValue(grid.GetCenter()) + refOff;
                if (val > off - Range.x && val < off + Range.y)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.GridMin)
            {
                int val = GetAxisValue(cell.Pos);
                int off = GetAxisValue(grid.GetMin()) + refOff;

                if (Range == Vector2.zero)
                {
                    if (val == GetAxisValue(grid.GetMin()) + refOff) CellAllow = true;
                }
                else
                if (val > off - Range.x && val < off + Range.y)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.GridMax)
            {
                int val = GetAxisValue(cell.Pos);
                int off = GetAxisValue(grid.GetMax()) + refOff;

                if (Range == Vector2.zero)
                {
                    if (val == GetAxisValue(grid.GetMax()) + refOff) CellAllow = true;
                }
                else
                if (val > off - Range.x && val < off + Range.y)
                {
                    CellAllow = true;
                }
            }
            else if (Mode == EPositionMode.EveryFew_Modulo)
            {
                if (refInf == 0) refInf = 2;
                int val = GetAxisValue(cell.Pos) + refOff;
                if (val % refInf == 0)
                {
                    CellAllow = true;
                }
            }
        }

        int GetAxisValue(Vector3Int pos)
        {
            if (Axis == EAxis.X) return pos.x;
            else if (Axis == EAxis.Y) return pos.y;
            else return pos.z;
        }
    }
}