//using UnityEngine;

//namespace FIMSpace.Generating.Rules.Transform
//{
//    public class SR_GetRoomDimensions : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Get Coordinates"; }

//        public EProcedureType Type => EProcedureType.Event;
//        public RoomModification GetFrom;
//        public GameObject GetFromPrefab;
//        public string GetFromTagged;

//        [Space(5)]
//        public Vector3 WorldOffset = Vector3.zero;
//        public Vector3 DirectionalOffset = Vector3.zero;
//        [Space(5)]
//        public Vector3 RotationEulerOffset = Vector3.zero;
//        [Space(5)]
//        public Vector3 ScaleMultiplier = Vector3.one;
//        [Space(5)]
//        public Vector3 RandomOffsets = Vector3.zero;
//        public Vector3 RandomRotation = Vector3.zero;
//        public Vector3 RandomScale = Vector3.zero;
//        [Space(5)]
//        public Vector3 PivotOffset = Vector3.zero;

//        public override void CellInfluence(RoomPreset preset, RoomModification mod, RoomCell cell, ref SpawnData spawn, FGenGraph<RoomCell, FGenVertex> grid)
//        {
//            SpawnData getSpawn = null;

//            for (int i = 0; i < cell.Spawns.Count; i++)
//            {
//                if (cell.Spawns[i] == null) continue;
//                getSpawn = GetConditionalSpawnData(cell, GetFromTagged, GetFromPrefab, GetFrom);
//            }

//            if (getSpawn != null)
//            {
//                spawn.Offset = getSpawn.Offset + WorldOffset;
//                spawn.DirectionalOffset = getSpawn.DirectionalOffset + DirectionalOffset;
//                spawn.RotationOffset = getSpawn.RotationOffset + RotationEulerOffset;
//                spawn.LocalScaleMul = Vector3.Scale(getSpawn.LocalScaleMul, ScaleMultiplier);
//            }


//            //if (RandomOffsets.sqrMagnitude > 0)
//            if (WorldOffset.sqrMagnitude >= DirectionalOffset.sqrMagnitude)
//            {
//                spawn.Offset += new Vector3(
//                    FGenerators.GetRandom(-RandomOffsets.x, RandomOffsets.x),
//                    FGenerators.GetRandom(-RandomOffsets.y, RandomOffsets.y),
//                    FGenerators.GetRandom(-RandomOffsets.z, RandomOffsets.z)
//                    );
//            }
//            else
//            {
//                spawn.DirectionalOffset += new Vector3(
//                    FGenerators.GetRandom(-RandomOffsets.x, RandomOffsets.x),
//                    FGenerators.GetRandom(-RandomOffsets.y, RandomOffsets.y),
//                    FGenerators.GetRandom(-RandomOffsets.z, RandomOffsets.z)
//                    );
//            }

//            //if (RandomRotation.sqrMagnitude > 0)
//            {
//                spawn.RotationOffset += new Vector3(
//                    FGenerators.GetRandom(-RandomRotation.x, RandomRotation.x),
//                    FGenerators.GetRandom(-RandomRotation.y, RandomRotation.y),
//                    FGenerators.GetRandom(-RandomRotation.z, RandomRotation.z)
//                    );
//            }

//            //if (RandomScale.sqrMagnitude > 0)
//            {
//                spawn.LocalScaleMul += new Vector3(
//                    FGenerators.GetRandom(-RandomScale.x, RandomScale.x),
//                    FGenerators.GetRandom(-RandomScale.y, RandomScale.y),
//                    FGenerators.GetRandom(-RandomScale.z, RandomScale.z)
//                    );
//            }

//            if (DirectionalOffset.sqrMagnitude > 0f)
//                spawn.DirectionalOffset += Vector3.Scale(PivotOffset, ScaleMultiplier);
//        }
//    }
//}