#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_AcquireSpawn : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Acquire Spawn"; }
        public override string Tooltip() { return "Acquiring temporary spawn into spawner, useful for finishing touch with use of 'Empty' spawn"; }
        public EProcedureType Type { get { return EProcedureType.Procedure; } }

        [PGG_SingleLineSwitch("GatherBy", 70, "Select if you want to use Tags, SpawnStigma or CellData", 84)]
        [Tooltip("Gather object for spawner by tag or other feature")]
        public string Get = "";
        [HideInInspector] public ESR_Details GatherBy = ESR_Details.Tag;

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Recommended to be used with 'Empty' in 'To Spawn'", MessageType.None);
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;

            if (string.IsNullOrEmpty(Get) == false)
            {
                SpawnData gettedSpawn = CellSpawnsHaveSpecifics(cell, Get, GatherBy, spawn);
                if (FGenerators.CheckIfExist_NOTNULL(gettedSpawn))
                {
                    spawn = gettedSpawn;
                }
            }
        }

    }
}