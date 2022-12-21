#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_DirectOffset : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Direct Offset"; }
        public override string Tooltip() { return "Offsetting spawn position according to current spawn ROTATION parameters"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [HideInInspector] [Tooltip("Push offset instead of overriding it")] public bool Append = false;
        //[HideInInspector] public bool Override = false;
        [HideInInspector] public bool Randomize = false;

        [PGG_SingleLineSwitch("CheckMode", 52, "Select if you want to use Tags, SpawnStigma or CellData", 140)]
        public string GetOffsetFromTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        public Vector3 Offset = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        [HideInInspector] public Vector3 RandomOffset = Vector3.zero;

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            EditorGUILayout.BeginHorizontal();

            //if (string.IsNullOrEmpty(GetOffsetFromTagged))
            EditorGUILayout.PropertyField(so.FindProperty("Append"));

            EditorGUILayout.PropertyField(so.FindProperty("Randomize"));

            EditorGUILayout.EndHorizontal();

            if (Randomize) EditorGUILayout.PropertyField(so.FindProperty("RandomOffset"));
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            SpawnData getSpawn = CellSpawnsHaveSpecifics(cell, GetOffsetFromTagged, CheckMode, spawn);

            Vector3 rOffset = new Vector3
            (
                FGenerators.GetRandom(-RandomOffset.x, RandomOffset.x),
                FGenerators.GetRandom(-RandomOffset.y, RandomOffset.y),
                FGenerators.GetRandom(-RandomOffset.z, RandomOffset.z)
            );

            if (Randomize == false) rOffset = Vector3.zero;

            Vector3 fullOffset = Offset + rOffset;
            fullOffset = GetUnitOffset(fullOffset, OffsetMode, preset);

            if (Append)
            {
                spawn.DirectionalOffset += fullOffset;
            }
            else
            {
                if (FGenerators.CheckIfExist_NOTNULL(getSpawn))
                    spawn.Offset = getSpawn.Offset + Quaternion.Euler(getSpawn.RotationOffset) * (fullOffset);
                else
                    spawn.DirectionalOffset = fullOffset;
            }
        }
    }
}