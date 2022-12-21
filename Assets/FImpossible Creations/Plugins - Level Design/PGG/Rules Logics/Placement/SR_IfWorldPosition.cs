using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_IfWorldPosition : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "If World Position"; }
        public override string Tooltip() { return "Allowing or disallowing running this spawner when this cell have certain position in world space\n[Lightweight] " + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public enum EPositionMode { ExactPosition, Greater, Lower, Between, EveryFew_Modulo }

        [PGG_SingleLineTwoProperties("Axis", 100, 40, 10, -80)]
        public EPositionMode Mode = EPositionMode.ExactPosition;

        public enum EAxis { X, Y, Z }
        [HideInInspector] public EAxis Axis = EAxis.Y;
        public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        [HideInInspector] public float Exact = 0;
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


        #region Editor Related
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


        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            float refInf = GetUnitOffset(GetAxis(Exact), OffsetMode, preset).magnitude;
            //int refInf = Mathf.RoundToInt(Exact * ValueMulVariable.GetValue(1f));
            float refOff = GetUnitOffset(GetAxis(Offset), OffsetMode, preset).magnitude;
            //int refOff = Mathf.RoundToInt(Offset * ValueMulVariable.GetValue(1f));

            var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

            for (int s = 0; s < spawns.Count; s++)
            {
                var sp = spawns[s];

                Vector3 spawnPos = sp.GetWorldPositionWithFullOffset(preset);

                if (Mode == EPositionMode.ExactPosition)
                {
                    if (GetAxisValue(spawnPos) == refInf)
                    {
                        CellAllow = true;
                    }
                }
                else if (Mode == EPositionMode.Greater)
                {
                    if (GetAxisValue(spawnPos) > refInf)
                    {
                        CellAllow = true;
                    }
                }
                else if (Mode == EPositionMode.Lower)
                {
                    if (GetAxisValue(spawnPos) < refInf)
                    {
                        CellAllow = true;
                    }
                }
                else if (Mode == EPositionMode.Between)
                {
                    float val = GetAxisValue(spawnPos);
                    if (val > refOff - Range.x && val < refOff + Range.y)
                    {
                        CellAllow = true;
                    }
                }
                else if (Mode == EPositionMode.EveryFew_Modulo)
                {
                    if (refInf == 0) refInf = 2;
                    float val = GetAxisValue(spawnPos) + refOff;
                    if (val % refInf == 0)
                    {
                        CellAllow = true;
                    }
                }
            }

        }

        Vector3 GetAxis(float value)
        {
            if (Axis == EAxis.X) return new Vector3(value, 0, 0);
            else if (Axis == EAxis.Y) return new Vector3(0, value, 0);
            else return new Vector3(0, 0, value);
        }

        float GetAxisValue(Vector3 value)
        {
            if (Axis == EAxis.X) return value.x;
            else if (Axis == EAxis.Y) return value.y;
            else return value.z;
        }
    }
}