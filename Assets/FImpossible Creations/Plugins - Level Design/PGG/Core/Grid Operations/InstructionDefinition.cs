using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class InstructionDefinition
    {

        public string Title = "Instruction";

        public enum EInstruction { None, DoorHole, PreRunModificator, PostRunModificator, PreventAllSpawn, PreventSpawnSelective, InjectStigma, InjectDataString, IsolatedGrid, SetGhostCell/*, Custom*/ }
        public EInstruction InstructionType;

        public FieldModification TargetModification;

        public string Tags = "";

        public string InstructionArgument;
        public bool Foldout = true;

        [Tooltip("Extra modificator variable, used to prevent spawning selective modificators")]
        public FieldModification extraMod;
        [Tooltip("Extra mod pack variable, used to prevent spawning selective mod packs")]
        public ModificatorsPack extraPack;

    }
}
