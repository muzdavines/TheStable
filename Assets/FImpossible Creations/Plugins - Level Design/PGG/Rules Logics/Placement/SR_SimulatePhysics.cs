using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_SimulatePhysics : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Simulate Physics"; }
        public override string Tooltip() { return "After generating all objects, there will be applied unity physics simulation to spawned objects in isolated collision with detected area around\n " + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        [HideInInspector] public OStampPhysicalPlacementSetup PhysicalPlacement;

#if UNITY_EDITOR

        private SerializedProperty _spPh = null;
        //private SerializedProperty _spPhEn = null;

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            PhysicalPlacement.Enabled = true;
            base.NodeFooter(so, mod);

            EditorGUILayout.HelpBox("Physics Simulation will be applied after all objects generation.", MessageType.None);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();

            if (_spPh == null)
            {
                _spPh = so.FindProperty("PhysicalPlacement");
            }

            PhysicalPlacement._Editor_DrawSetup(_spPh, false);
            GUILayout.Space(4);

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(this);

        }
#endif

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {

            Action<GameObject> simulation =
            (o) =>
            {
                if (o == null) return;

                //if (wasSyncing)
                //{
                //    Physics.SyncTransforms();
                //    wasSyncing = true;
                //}
                
                // Strange way but for now only da way
                preset.AddAfterGeneratingEvent(() => { PhysicalPlacement.ProceedOn(o); });
            };

            thisSpawn.OnGeneratedEvents.Add(simulation);

        }

        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            base.PreGenerateResetRule(grid, preset, callFrom);
            //wasSyncing = false;
        }

        //static bool wasSyncing = false;
    }
}