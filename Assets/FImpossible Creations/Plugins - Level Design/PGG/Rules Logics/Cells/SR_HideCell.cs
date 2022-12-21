#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules.Helpers;

namespace FIMSpace.Generating.Rules.Cells.Legacy
{
    public class SR_HideCell : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.Event; } }
        public override string TitleName() { return "Hide Cell"; }
        public override string Tooltip() { return "Helpful with custom guides"; }

        public int HideAfterCheck = 1;

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
            EditorGUILayout.HelpBox("Makes cell empty, helpful for adding single empty cells through guides", MessageType.None);
            //EditorGUILayout.HelpBox("Makes cell in if used 'In Grid' check but still treated as 'Out of grid'", MessageType.None);
            //EditorGUILayout.HelpBox("Makes cell out if used 'In Grid' check but not treated as 'Out of grid'", MessageType.None);
        }
#endif

        int counter = 0;
        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            counter = HideAfterCheck;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            counter--;
            if (counter < 0)
            {
                for (int i = cell.GetSpawnsJustInsideCell().Count - 1; i >= 0; i--)
                    cell.GetSpawnsJustInsideCell().RemoveAt(i);

                spawn = null;
            }
        }

    }
}