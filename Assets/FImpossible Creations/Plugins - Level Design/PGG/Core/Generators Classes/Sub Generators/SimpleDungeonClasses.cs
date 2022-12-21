using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.RectOfFields
{
    [System.Serializable] // Make it displayable and memorizable inside inspector window
    public class FieldOfDungeon
    {
        public FieldSetup Preset;

        [Space(2)]
        public MinMax SizeX = new MinMax(2, 3);
        public MinMax SizeY = new MinMax(3, 6);
        public MinMax DistanceFromCorridor = new MinMax(8, 12);

        [Space(2)]
        [Range(1,8)] public int HowManyToCreate = 1;
        [Range(1,4)] public int SeparateFromOthers = 1;
        [Range(0, 1)] public float ConnectWithOtherPropability = 0f;

        [Space(2)]
        //public FieldSetup PathTo_Preset;
        [Range(1, 5)] public int PathTo_Thickness = 1;
        [Range(0f, 1f)] internal float PathTo_ChangeDirCost = .35f;

        [HideInInspector]
        public int AlreadyGenerated = 0;

        [HideInInspector] public SimplePathGuide pathFind;

        internal void Refresh()
        {
            if (HowManyToCreate == 0) HowManyToCreate = 3;
            if (SeparateFromOthers == 0) SeparateFromOthers = 1;
            if (SizeX.IsZero) SizeX = new MinMax(3, 3);
            if (SizeY.IsZero) SizeY = new MinMax(3, 3);
            if (DistanceFromCorridor.IsZero) DistanceFromCorridor = new MinMax(8, 12);
            if (PathTo_ChangeDirCost <= 0f) PathTo_ChangeDirCost = 0.3f;
            if (PathTo_Thickness <= 0) PathTo_Thickness = 1;
            AlreadyGenerated = 0;
        }

        internal Bounds GetBounds()
        {
            return GetBounds(SizeX, SizeY);
        }

        internal Bounds GetBounds(MinMax sizeX, MinMax sizeY, bool rotate = false)
        {
            Bounds b;
            
            if (!rotate)
                b = new Bounds(Vector3.zero, new Vector3(sizeX.GetRandom(), 0, sizeY.GetRandom()));
            else
                b = new Bounds(Vector3.zero, new Vector3(sizeY.GetRandom(), 0, sizeX.GetRandom()));

            OffsetToGrid(ref b);

            return b;
        }

        public static void OffsetToGrid(ref Bounds b)
        {
            if (b.size.x % 2 == 0) b.center += new Vector3(0.5f, 0f, 0f);
            if (b.size.z % 2 == 0) b.center -= new Vector3(0.0f, 0f, 0.5f);
        }

    }

    public class FieldOfDungeonInstance 
    {
        public FieldOfDungeon FieldRef;
        public CheckerField Checker;

        public bool IsMainCorridor = false;
        public bool HaveConnection = false;

        public FieldOfDungeonInstance(FieldOfDungeon fieldRef/*, Bounds bound*/)
        {
            FieldRef = fieldRef;
            Checker = new CheckerField();
        }

        public List<FieldOfDungeonInstance> Connections = new List<FieldOfDungeonInstance>();

        internal void DrawGizmos(float scale, Vector3 offset)
        {
            Checker.DrawGizmos(scale);
        }

    }

}