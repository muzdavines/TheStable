
namespace FIMSpace.Generating
{
    [System.Serializable]
    public struct SpawnRestriction
    {
        public bool UseRestrictSpawnForTags;
        public string RestrictSpawnForTags;
        public string UsePresetsDefsByName;
        public InstructionDefinition CustomDefinition;

        public bool IsRestricting()
        {
            if (UseRestrictSpawnForTags) if (string.IsNullOrEmpty(RestrictSpawnForTags) == false) return true;
            if (string.IsNullOrEmpty(UsePresetsDefsByName) == false) return true;
            if (CustomDefinition != null) if (CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None) return true;
            return false;
        }

        InstructionDefinition tempDef ;
        InstructionDefinition tempFindDef ;
        public InstructionDefinition GetSpawnInstructionDefinition(FieldSetup setup)
        {
            if ( UseRestrictSpawnForTags)
            {
                if (tempDef == null || tempDef.InstructionType == InstructionDefinition.EInstruction.None) tempDef = new InstructionDefinition() { InstructionType = InstructionDefinition.EInstruction.PreventSpawnSelective, Tags = RestrictSpawnForTags };
                return tempDef;
            }
            
            if (CustomDefinition != null) if (CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None) return CustomDefinition;

            if (string.IsNullOrEmpty(UsePresetsDefsByName) == false)
            {
                if (tempFindDef == null) tempFindDef = setup.FindCellInstruction(UsePresetsDefsByName);
                return tempFindDef;
            }

            return null;
        }
    }
}