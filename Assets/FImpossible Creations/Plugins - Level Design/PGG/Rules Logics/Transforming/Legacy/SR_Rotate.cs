#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public partial class SR_Rotate : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Rotate"; }
        public override string Tooltip() { return "Rotating target spawned prefab (can influence other objects spawned in this cell)"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }
        [PGG_SingleLineSwitch("CheckMode", 52, "Select if you want to use Tags, SpawnStigma or CellData", 148)]
        public string GetRotationFromTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public Vector3 RotationEulerOffset = Vector3.zero;
        public Vector3 RandomRotation = Vector3.zero;
        public Vector3 MaxDegreesSteps = Vector3.one;
        public bool LocalRotation = false;
        [HideInInspector]
        public bool OverrideRotation = true;

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);

            if (string.IsNullOrEmpty(GetRotationFromTagged))
                EditorGUILayout.PropertyField(so.FindProperty("OverrideRotation"));

            so.ApplyModifiedProperties();
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            bool tagged = false;
            if (string.IsNullOrEmpty(GetRotationFromTagged) == false)
            {
                SpawnData sp = CellSpawnsHaveSpecifics(cell, GetRotationFromTagged, CheckMode, spawn);
                if (sp != null) if ( sp.LocalRotationOffset == Vector3.zero) spawn.LocalRotationOffset = sp.GetRotationOffsetWithMods(false); else spawn.LocalRotationOffset = sp.GetRotationOffsetWithMods(true);
                tagged = true;
            }

            Vector3 randomOffset = Vector3.zero;
            float choosed = FGenerators.GetRandom(-RandomRotation.x, RandomRotation.x);
            if ( MaxDegreesSteps.x != 0f) choosed = OStamperSet.GetAngleFor(MaxDegreesSteps.x, 1f, choosed / MaxDegreesSteps.x);
            randomOffset.x = choosed;

            choosed = FGenerators.GetRandom(-RandomRotation.y, RandomRotation.y);
            if (MaxDegreesSteps.y != 0f) choosed = OStamperSet.GetAngleFor(MaxDegreesSteps.y, 1f, choosed / MaxDegreesSteps.y);
            randomOffset.y = choosed;

            choosed = FGenerators.GetRandom(-RandomRotation.z, RandomRotation.z);
            if (MaxDegreesSteps.z != 0f) choosed = OStamperSet.GetAngleFor(MaxDegreesSteps.z, 1f, choosed / MaxDegreesSteps.z);
            randomOffset.z = choosed;

            if (LocalRotation)
            {
                if (OverrideRotation && !tagged)
                    spawn.LocalRotationOffset = RotationEulerOffset + randomOffset;
                else
                    spawn.LocalRotationOffset += RotationEulerOffset + randomOffset;
            }
            else
            {
                if (OverrideRotation && !tagged)
                    spawn.RotationOffset = RotationEulerOffset + randomOffset;
                else
                    spawn.RotationOffset += RotationEulerOffset + randomOffset;
            }
        }
    }
}