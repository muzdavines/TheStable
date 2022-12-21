using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public struct SpawnInstruction
    {
        /// <summary> cell's grid position </summary>
        public Vector3Int gridPosition;
        /// <summary> Enable 'useDirection' to enable this variable </summary>
        public Vector3Int desiredDirection;
        /// <summary> To use 'desiredDirection' </summary>
        public bool useDirection;

        /// <summary> Important definition for the guide to be used by FieldSetup on provided grid </summary>
        public InstructionDefinition definition;

        /// <summary> Helper additional variable for generation helpers </summary>
        public EHelperGuideType helperType;
        /// <summary> Helper additional variable for generation helpers </summary>
        public PlanHelper.ConnectionRect helperConnection;
        /// <summary> Helper additional variable for generation helpers </summary>
        public Vector3Int helperCoords;
        /// <summary> Helper index to use with custom code </summary>
        public int HelperID;

        public Vector3 FlatV3Pos
        { get { return new Vector3(gridPosition.x, gridPosition.y, gridPosition.z); } }
        public Vector3 FlatDirection
        { get { return new Vector3(desiredDirection.x, desiredDirection.y, desiredDirection.z); } }

        public bool IsPreDefinition
        { get { if (definition == null) return false; if (definition.InstructionType == InstructionDefinition.EInstruction.PreRunModificator || definition.InstructionType == InstructionDefinition.EInstruction.DoorHole || definition.InstructionType == InstructionDefinition.EInstruction.SetGhostCell) return true; return false; } }

        public bool IsPostDefinition
        { get { if (definition == null) return false; if (definition.InstructionType == InstructionDefinition.EInstruction.PostRunModificator) return true; return false; } }

        public bool IsPreSpawn
        { get { if (definition == null) return false; if (definition.InstructionType == InstructionDefinition.EInstruction.PreventAllSpawn || definition.InstructionType == InstructionDefinition.EInstruction.PreventSpawnSelective ) return true; return false; } }

        public bool IsPostSpawn
        { get { if (definition == null) return false; if (definition.InstructionType == InstructionDefinition.EInstruction.InjectStigma ) return true; return false; } }

        public bool IsModRunner
        { get { if (definition == null) return false; if (definition.InstructionType == InstructionDefinition.EInstruction.PreRunModificator || definition.InstructionType == InstructionDefinition.EInstruction.PostRunModificator) return true; return false; } }
    }
}