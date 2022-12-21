using FIMSpace.Generating.Checker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.RectOfFields
{
    [Serializable] // Make it displayable and memorizable inside inspector window
    public class FieldOfRect
    {
        public FieldSetup Preset;
        [Tooltip(" Zero = Infinite ")]
        public int MaxCountOfThisRoom = 0;

        public MinMax DesiredSizeInCells = new MinMax(8, 14);
        public List<InjectionSetup> Injections;

        [HideInInspector]
        public int AlreadyGenerated = 0;

        public void CheckReset()
        {
            if (DesiredSizeInCells.IsZero)
            {
                MaxCountOfThisRoom = 0;
                DesiredSizeInCells = new MinMax(8, 14);
                AlreadyGenerated = 0;
            }
        }

        internal void Refresh()
        {
            AlreadyGenerated = 0;
        }
    }

    [Serializable]
    public class FieldOfRectStatic
    {
        public FieldSetup Preset;
        public Vector2Int StaticPosition = new Vector2Int(4, 4);
        public Vector2Int Size = new Vector2Int(3, 2);
        public List<InjectionSetup> Injections;
        [Tooltip("If door walls towards new room should be different than command id number setted under 'Draw Guides Ids to Choose' tab, it's useful for example for elevator when we want to remove walls")]
        public int OverrideDoorholeCommand = -1;
        public int OverrideFromCorridorCommand = -1;
        public bool OnlyCorridorConnection = false;
        public Vector3Int PrioritizeDoorsDirection = new Vector3Int(0, 0, 0);
        public Vector2Int PrioritizeOriginOffset = new Vector2Int(0, 0);

        public void CheckReset()
        {
            if (Size == Vector2Int.zero)
            {
                StaticPosition = new Vector2Int(4, 4);
                Size = new Vector2Int(3, 2);
            }
        }
    }

    public class RectOfFieldsInstance
    {
        public FieldOfRect FieldRef;
        public FieldOfRectStatic FieldRefStatic;
        public CheckerField Checker;
        public List<RectOfFieldsInstance> Connections = new List<RectOfFieldsInstance>();
        public bool Setted = false;
        public bool IsMainCorridor = false;

        public RectOfFieldsInstance()
        {
            FieldRef = null;
            FieldRefStatic = null;
            Checker = new CheckerField();
        }

        public FieldSetup GetFieldSetup()
        {
            if (FGenerators.CheckIfExist_NOTNULL( FieldRef )) return FieldRef.Preset;
            if (FGenerators.CheckIfExist_NOTNULL(FieldRef )) return FieldRefStatic.Preset;
            return null;
        }

        internal void DrawGizmos(float scale)
        {
            Checker.DrawGizmos(scale);
        }
    }
}